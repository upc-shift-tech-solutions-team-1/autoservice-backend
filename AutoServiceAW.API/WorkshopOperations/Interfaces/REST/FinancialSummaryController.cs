using AutoServiceAW.API.WorkshopOperations.Domain.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WorkshopTask =
    AutoServiceAW.API.WorkshopOperations.Domain.Model.Aggregates.Task;

namespace AutoServiceAW.API.WorkshopOperations.Interfaces.REST;

public record FinancialPeriodPointResource(
    string Date,
    string Label,
    decimal Revenue,
    decimal Cost,
    decimal Profit
);

public record OrderProfitabilityResource(
    int WorkOrderId,
    string TrackingCode,
    decimal Revenue,
    decimal Cost,
    decimal Profit,
    decimal MarginPercentage
);

public record FinancialSummaryResource(
    decimal ProjectedRevenue,
    decimal RealizedRevenue,
    decimal PendingRevenue,
    decimal OperatingCost,
    decimal GrossProfit,
    decimal MarginPercentage,
    int ProfitableOrders,
    int LossOrders,
    decimal AverageTicket,
    IReadOnlyCollection<FinancialPeriodPointResource> PeriodSeries,
    IReadOnlyCollection<OrderProfitabilityResource> OrderProfitability
);

[ApiController]
[Route("api/v1/financial-summary")]
[Authorize]
public class FinancialSummaryController(
    IWorkOrderService workOrderService,
    ITaskService taskService
) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetSummary(
        [FromQuery] int days = 7
    )
    {
        days = Math.Clamp(days, 7, 90);

        var workshopId = User.Claims
            .FirstOrDefault(
                claim =>
                    claim.Type == "WorkshopId"
            )
            ?.Value;

        if (string.IsNullOrWhiteSpace(workshopId))
        {
            return Unauthorized();
        }

        var orders =
            (
                await workOrderService
                    .ListByTenantIdAsync(workshopId)
            )
            .Where(
                order =>
                    !string.Equals(
                        order.Status,
                        "CANCELLED",
                        StringComparison.OrdinalIgnoreCase
                    )
            )
            .ToList();

        var orderIds = orders
            .Select(order => order.Id)
            .ToHashSet();

        var tasks =
            (
                await taskService.ListAsync()
            )
            .Where(
                task =>
                    orderIds.Contains(
                        task.WorkOrderId
                    )
            )
            .ToList();

        var projectedProfitability = orders
            .Select(
                order =>
                    BuildOrderProfitability(
                        order.Id,
                        order.TrackingCode,
                        tasks.Where(
                            task =>
                                task.WorkOrderId ==
                                order.Id
                        )
                    )
            )
            .ToList();

        var deliveredOrderIds = orders
            .Where(
                order =>
                    string.Equals(
                        order.Status,
                        "DELIVERED",
                        StringComparison.OrdinalIgnoreCase
                    )
            )
            .Select(order => order.Id)
            .ToHashSet();

        var realizedTasks = tasks
            .Where(
                task =>
                    deliveredOrderIds.Contains(
                        task.WorkOrderId
                    ) ||
                    IsValidatedTask(task)
            )
            .ToList();

        var realizedProfitability = orders
            .Select(
                order =>
                    BuildOrderProfitability(
                        order.Id,
                        order.TrackingCode,
                        realizedTasks.Where(
                            task =>
                                task.WorkOrderId ==
                                order.Id
                        )
                    )
            )
            .Where(
                item =>
                    item.Revenue != 0 ||
                    item.Cost != 0
            )
            .ToList();

        var projectedRevenue =
            projectedProfitability.Sum(
                item => item.Revenue
            );

        var realizedRevenue =
            realizedProfitability.Sum(
                item => item.Revenue
            );

        var pendingRevenue = Math.Max(
            0,
            projectedRevenue - realizedRevenue
        );

        var operatingCost =
            realizedProfitability.Sum(
                item => item.Cost
            );

        var grossProfit =
            realizedRevenue - operatingCost;

        var marginPercentage =
            realizedRevenue == 0
                ? 0
                : Math.Round(
                    (
                        grossProfit /
                        realizedRevenue
                    ) * 100,
                    2
                );

        var startDate =
            DateTime.UtcNow.Date
                .AddDays(-(days - 1));

        var periodSeries = Enumerable
            .Range(0, days)
            .Select(
                offset =>
                    startDate.AddDays(offset)
            )
            .Select(
                date =>
                {
                    var dateOrderIds = orders
                        .Where(
                            order =>
                                DateOnly.TryParse(
                                    order.StartDate,
                                    out var parsedDate
                                ) &&
                                parsedDate ==
                                DateOnly.FromDateTime(date)
                        )
                        .Select(order => order.Id)
                        .ToHashSet();

                    var dateProfitability =
                        realizedProfitability
                            .Where(
                                item =>
                                    dateOrderIds.Contains(
                                        item.WorkOrderId
                                    )
                            )
                            .ToList();

                    var revenue =
                        dateProfitability.Sum(
                            item => item.Revenue
                        );

                    var cost =
                        dateProfitability.Sum(
                            item => item.Cost
                        );

                    return new FinancialPeriodPointResource(
                        date.ToString("yyyy-MM-dd"),
                        date.ToString("ddd")
                            .ToUpperInvariant(),
                        revenue,
                        cost,
                        revenue - cost
                    );
                }
            )
            .ToList();

        var profitableOrders =
            realizedProfitability.Count(
                item => item.Profit > 0
            );

        var lossOrders =
            realizedProfitability.Count(
                item => item.Profit < 0
            );

        var averageTicket =
            realizedProfitability.Count == 0
                ? 0
                : Math.Round(
                    realizedRevenue /
                    realizedProfitability.Count,
                    2
                );

        return Ok(
            new FinancialSummaryResource(
                projectedRevenue,
                realizedRevenue,
                pendingRevenue,
                operatingCost,
                grossProfit,
                marginPercentage,
                profitableOrders,
                lossOrders,
                averageTicket,
                periodSeries,
                realizedProfitability
                    .OrderByDescending(
                        item => item.Profit
                    )
                    .ToList()
            )
        );
    }

    private static bool IsValidatedTask(
        WorkshopTask task
    )
    {
        return string.Equals(
                   task.Status,
                   "COMPLETED",
                   StringComparison.OrdinalIgnoreCase
               ) &&
               string.Equals(
                   task.AdminReviewStatus,
                   "APPROVED",
                   StringComparison.OrdinalIgnoreCase
               );
    }

    private static OrderProfitabilityResource
        BuildOrderProfitability(
            int workOrderId,
            string trackingCode,
            IEnumerable<WorkshopTask> tasks
        )
    {
        var taskList = tasks.ToList();

        var revenue =
            taskList.Sum(
                task => task.TotalCost
            );

        var cost =
            taskList.Sum(
                task => task.TotalInternalCost
            );

        var profit =
            revenue - cost;

        var marginPercentage =
            revenue == 0
                ? 0
                : Math.Round(
                    (
                        profit /
                        revenue
                    ) * 100,
                    2
                );

        return new OrderProfitabilityResource(
            workOrderId,
            trackingCode,
            revenue,
            cost,
            profit,
            marginPercentage
        );
    }
}
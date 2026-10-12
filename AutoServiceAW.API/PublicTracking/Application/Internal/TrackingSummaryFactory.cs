using AutoServiceAW.API.PublicTracking.Interfaces.REST;
using AutoServiceAW.API.WorkshopOperations.Domain.Model.Aggregates;
using WorkshopTask = AutoServiceAW.API.WorkshopOperations.Domain.Model.Aggregates.Task;

namespace AutoServiceAW.API.PublicTracking.Application.Internal;

/// <summary>
/// Builds the safe customer-facing view of a work order and its tasks.
/// </summary>
public static class TrackingSummaryFactory
{
    public static TrackingSummaryResource Create(
        WorkOrder workOrder,
        IEnumerable<WorkshopTask> tasks
    )
    {
        var taskList = tasks.ToList();
        var laborSubtotal = taskList.Sum(task => task.LaborPrice);
        var materialsSubtotal = taskList.Sum(task => task.MaterialsCost);

        var publicTasks = taskList
            .Select(task => new TrackingTaskResource(
                task.Description,
                task.Status,
                task.TechnicalDiagnosis,
                task.CustomerExplanation,
                task.EvidenceRegistered,
                task.LaborPrice,
                task.MaterialsCost,
                task.TotalCost,
                task.Parts
                    .Select(part => new TrackingTaskPartResource(
                        part.Name,
                        part.Quantity,
                        part.UnitPrice
                    ))
                    .ToList()
            ))
            .ToList();

        var history = workOrder.StatusHistory
            .OrderBy(stage => stage.ChangedAtUtc)
            .Select(stage => new TrackingStageResource(
                stage.Status,
                DateTime.SpecifyKind(stage.ChangedAtUtc, DateTimeKind.Utc)
            ))
            .ToList();

        return new TrackingSummaryResource(
            workOrder.TrackingCode,
            workOrder.Status,
            CalculateProgress(taskList),
            workOrder.EstimatedDate,
            publicTasks,
            new TrackingCostBreakdownResource(
                laborSubtotal,
                materialsSubtotal,
                laborSubtotal + materialsSubtotal
            ),
            history
        );
    }

    public static decimal CalculateProgress(IEnumerable<WorkshopTask> tasks)
    {
        var taskList = tasks.ToList();
        if (taskList.Count == 0)
        {
            return 0m;
        }

        var completedCount = taskList.Count(task =>
            string.Equals(task.Status, "COMPLETED", StringComparison.OrdinalIgnoreCase)
        );

        return Math.Round((decimal)completedCount / taskList.Count * 100m, 2);
    }
}

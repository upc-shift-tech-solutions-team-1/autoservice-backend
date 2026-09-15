using AutoServiceAW.API.InventoryManagement.Domain.Model.Aggregates;
using AutoServiceAW.API.InventoryManagement.Domain.Services;
using AutoServiceAW.API.WorkshopOperations.Domain.Model.Aggregates;
using AutoServiceAW.API.WorkshopOperations.Domain.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WorkshopTask =
    AutoServiceAW.API.WorkshopOperations.Domain.Model.Aggregates.Task;

namespace AutoServiceAW.API.WorkshopOperations.Interfaces.REST;

public record CreateTaskPartResource(
    int InventoryItemId,
    int Quantity
);

public record CreateTaskResource(
    int WorkOrderId,
    int? MechanicId,
    string Description,
    string Priority,
    int EstimatedTime,
    decimal LaborPrice,
    List<CreateTaskPartResource>? Parts,
    decimal LaborCost = 0,
    string? TechnicalDiagnosis = null,
    string? AdminReviewStatus = null
);

public record UpdateTaskResource(
    string Description,
    string Status,
    string Priority,
    int EstimatedTime,
    decimal LaborPrice,
    int? MechanicId,
    decimal LaborCost = 0,
    string? AdminReviewStatus = null
);

public record PatchTaskResource(
    string? Status,
    string? TechnicalDiagnosis,
    string? CustomerExplanation,
    string? InternalObservation,
    string? EvidenceRegistered,
    string? AdminReviewStatus
);

[ApiController]
[Route("api/v1/[controller]")]
[Authorize]
public class TasksController(
    ITaskService taskService,
    IWorkOrderService workOrderService,
    IInventoryItemService inventoryItemService
) : ControllerBase
{
    private const string PendingStatus = "PENDING";
    private const string InProgressStatus = "IN_PROGRESS";
    private const string ApprovedReviewStatus = "APPROVED";

    [HttpPost]
    public async Task<IActionResult> CreateTask(
        [FromBody] CreateTaskResource resource
    )
    {
        var requestedParts =
            resource.Parts ?? [];

        var selectedItems =
            new List<(
                CreateTaskPartResource Request,
                InventoryItem Item
            )>();

        foreach (var part in requestedParts)
        {
            if (part.Quantity <= 0)
            {
                return BadRequest(
                    new
                    {
                        message =
                            "La cantidad del material debe ser mayor que cero."
                    }
                );
            }

            var inventoryItem =
                await inventoryItemService.GetByIdAsync(
                    part.InventoryItemId
                );

            if (inventoryItem == null)
            {
                return BadRequest(
                    new
                    {
                        message =
                            $"No se encontró el material con id {part.InventoryItemId}."
                    }
                );
            }

            selectedItems.Add(
                (
                    part,
                    inventoryItem
                )
            );
        }

        var task = new WorkshopTask(
            resource.WorkOrderId,
            resource.MechanicId,
            resource.Description,
            PendingStatus,
            resource.Priority,
            resource.EstimatedTime,
            resource.LaborPrice,
            resource.LaborCost
        );

        decimal materialsSaleTotal = 0;
        decimal materialsPurchaseTotal = 0;

        foreach (var selection in selectedItems)
        {
            var request =
                selection.Request;

            var inventoryItem =
                selection.Item;

            task.AddPart(
                new TaskPart(
                    0,
                    inventoryItem.Id,
                    inventoryItem.Name,
                    request.Quantity,
                    inventoryItem.UnitPrice,
                    inventoryItem.PurchasePrice,
                    inventoryItem.Brand,
                    inventoryItem.QualityTier
                )
            );

            materialsSaleTotal +=
                inventoryItem.UnitPrice *
                request.Quantity;

            materialsPurchaseTotal +=
                inventoryItem.PurchasePrice *
                request.Quantity;
        }

        task.UpdateMaterialsCost(
            materialsSaleTotal,
            materialsPurchaseTotal
        );

        task.PatchTechnicalData(
            string.Empty,
            resource.TechnicalDiagnosis ??
            string.Empty,
            string.Empty,
            string.Empty,
            string.Empty,
            resource.AdminReviewStatus ??
            string.Empty
        );

        var result =
            await taskService.CreateAsync(
                task
            );

        return StatusCode(
            StatusCodes.Status201Created,
            result
        );
    }

    [HttpGet]
    public async Task<IActionResult> GetTasks(
        [FromQuery] int? workOrderId,
        [FromQuery] int? mechanicId
    )
    {
        var workshopId = User.Claims
            .FirstOrDefault(
                claim =>
                    claim.Type == "WorkshopId"
            )
            ?.Value;

        if (string.IsNullOrEmpty(workshopId))
        {
            return Unauthorized();
        }

        IEnumerable<WorkshopTask> tasks;

        if (workOrderId.HasValue)
        {
            tasks =
                await taskService
                    .ListByWorkOrderIdAsync(
                        workOrderId.Value
                    );
        }
        else if (mechanicId.HasValue)
        {
            tasks =
                await taskService
                    .ListByMechanicIdAsync(
                        mechanicId.Value
                    );
        }
        else
        {
            var allTasks =
                await taskService.ListAsync();

            var workOrders =
                await workOrderService.ListAsync();

            var workshopOrderIds =
                workOrders
                    .Where(
                        workOrder =>
                            workOrder.WorkshopId ==
                            workshopId
                    )
                    .Select(
                        workOrder =>
                            workOrder.Id
                    )
                    .ToHashSet();

            tasks = allTasks.Where(
                task =>
                    workshopOrderIds.Contains(
                        task.WorkOrderId
                    )
            );
        }

        return Ok(tasks);
    }

   [HttpPut("{id:int}")]
public async Task<IActionResult> UpdateTask(
    int id,
    [FromBody] UpdateTaskResource resource
)
{
    var task = new WorkshopTask(
        0,
        resource.MechanicId,
        resource.Description,
        resource.Status,
        resource.Priority,
        resource.EstimatedTime,
        resource.LaborPrice,
        resource.LaborCost
    );

    var result =
        await taskService.UpdateAsync(
            id,
            task
        );

    if (result == null)
    {
        return NotFound();
    }

    if (
        !string.IsNullOrWhiteSpace(
            resource.AdminReviewStatus
        )
    )
    {
        result =
            await taskService.PatchStatusAsync(
                id,
                string.Empty,
                string.Empty,
                string.Empty,
                string.Empty,
                string.Empty,
                resource.AdminReviewStatus
            );

        if (result == null)
        {
            return NotFound();
        }
    }

    return Ok(result);
}

    [HttpPatch("{id:int}")]
    public async Task<IActionResult> PatchTask(
        int id,
        [FromBody] PatchTaskResource resource
    )
    {
        var requestedStatus =
            NormalizeCode(
                resource.Status
            );

        if (requestedStatus == InProgressStatus)
        {
            var currentTask =
                await taskService.GetByIdAsync(
                    id
                );

            if (currentTask == null)
            {
                return NotFound();
            }

            var currentStatus =
                NormalizeCode(
                    currentTask.Status
                );

            var currentReviewStatus =
                NormalizeCode(
                    currentTask.AdminReviewStatus
                );

            if (currentStatus != InProgressStatus)
            {
                if (currentStatus != PendingStatus)
                {
                    return BadRequest(
                        new
                        {
                            message =
                                "Solo se puede iniciar una tarea pendiente.",
                            currentStatus =
                                currentTask.Status
                        }
                    );
                }

                if (
                    currentReviewStatus !=
                    ApprovedReviewStatus
                )
                {
                    return BadRequest(
                        new
                        {
                            message =
                                "La tarea debe ser aprobada antes de iniciarse.",
                            adminReviewStatus =
                                currentTask.AdminReviewStatus
                        }
                    );
                }

                var stockError =
                    await ConsumeTaskMaterialsAsync(
                        currentTask
                    );

                if (stockError != null)
                {
                    return stockError;
                }
            }
        }

        var result =
            await taskService.PatchStatusAsync(
                id,
                resource.Status ??
                string.Empty,
                resource.TechnicalDiagnosis ??
                string.Empty,
                resource.CustomerExplanation ??
                string.Empty,
                resource.InternalObservation ??
                string.Empty,
                resource.EvidenceRegistered ??
                string.Empty,
                resource.AdminReviewStatus ??
                string.Empty
            );

        return result == null
            ? NotFound()
            : Ok(result);
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> DeleteTask(
        int id
    )
    {
        await taskService.DeleteAsync(id);

        return NoContent();
    }

    private async Task<IActionResult?> ConsumeTaskMaterialsAsync(
        WorkshopTask task
    )
    {
        var groupedParts =
            task.Parts
                .GroupBy(
                    part =>
                        part.InventoryItemId
                )
                .Select(
                    group =>
                        new
                        {
                            InventoryItemId =
                                group.Key,

                            Quantity =
                                group.Sum(
                                    part =>
                                        part.Quantity
                                )
                        }
                )
                .ToList();

        var stockRequirements =
            new List<(
                InventoryItem Item,
                int Quantity
            )>();

        foreach (var requirement in groupedParts)
        {
            var inventoryItem =
                await inventoryItemService.GetByIdAsync(
                    requirement.InventoryItemId
                );

            if (inventoryItem == null)
            {
                return BadRequest(
                    new
                    {
                        message =
                            $"El material con id {requirement.InventoryItemId} ya no existe."
                    }
                );
            }

            if (
                inventoryItem.Stock <
                requirement.Quantity
            )
            {
                return BadRequest(
                    new
                    {
                        message =
                            $"Stock insuficiente para {inventoryItem.Name}.",
                        inventoryItemId =
                            inventoryItem.Id,
                        available =
                            inventoryItem.Stock,
                        requested =
                            requirement.Quantity
                    }
                );
            }

            stockRequirements.Add(
                (
                    inventoryItem,
                    requirement.Quantity
                )
            );
        }

        foreach (var requirement in stockRequirements)
        {
            requirement.Item.ConsumeStock(
                requirement.Quantity
            );

            await inventoryItemService.UpdateAsync(
                requirement.Item.Id,
                requirement.Item
            );
        }

        return null;
    }

    private static string NormalizeCode(
        string? value
    )
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return string.Empty;
        }

        return value
            .Trim()
            .ToUpperInvariant()
            .Replace('-', '_')
            .Replace(' ', '_');
    }
}
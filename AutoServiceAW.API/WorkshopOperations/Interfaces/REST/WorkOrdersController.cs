using AutoServiceAW.API.WorkshopOperations.Domain.Model.Aggregates;
using AutoServiceAW.API.WorkshopOperations.Domain.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AutoServiceAW.API.WorkshopOperations.Interfaces.REST;

/// <summary>
/// Data Transfer Object (DTO) schema framework wrapper used to request a new vehicle service entry order file tracking sequence.
/// </summary>
/// <param name="VehicleId">The target automotive unit asset identification tracking primary key index.</param>
/// <param name="CustomerId">The unique profile index reference identifier tracking the client owner actor.</param>
/// <param name="MechanicId">The unique technical member identifier tracking the main supervisor lead allocation.</param>
/// <param name="Description">The initial diagnostic evaluation scope script narrative or user request comments.</param>
/// <param name="EstimatedDate">The target commitment timeline delivery indicator text string.</param>
public record CreateWorkOrderResource(int VehicleId, int CustomerId, int MechanicId, string Description, string EstimatedDate);

/// <summary>
/// Data Transfer Object (DTO) layout containing complete metadata, quality tracking checklists, status flags, and final pricing matrices configurations.
/// </summary>
/// <param name="Description">The updated folder description details analysis report statement.</param>
/// <param name="EstimatedDate">The revised target commitment delivery date parameters layout.</param>
/// <param name="Price">The final consolidated total calculation pricing metric tracking labor and parts costs fee.</param>
/// <param name="Status">The altered macro operational state tracking code string label context.</param>
/// <param name="TasksCompleted">The quality audit verification step tracking task collection completions status indicator flag.</param>
/// <param name="SparePartsChecked">The quality audit verification step tracking warehouse component resource allocations checkoff flag.</param>
/// <param name="DiagnosisValidated">The quality audit verification step tracking analytical mechanical assessment signature verification flag.</param>
/// <param name="CleaningDone">The final delivery quality checkpoint tracking vehicle car cosmetic cleanup completion flag.</param>
/// <param name="FinalTestDone">The final delivery quality checkpoint tracking road trial driving test verification flag.</param>
public record UpdateWorkOrderResource(string Description, string EstimatedDate, decimal Price, string Status, bool TasksCompleted, bool SparePartsChecked, bool DiagnosisValidated, bool CleaningDone, bool FinalTestDone);

/// <summary>
/// Exposes RESTful endpoints for initiating main maintenance work folders, evaluating checklist metrics data grids, and fetching multi-tenant collections arrays.
/// </summary>
[ApiController]
[Route("api/v1/[controller]")]
[Authorize]
public class WorkOrdersController(IWorkOrderService workOrderService) : ControllerBase
{
    #region Methods

    /// <summary>
    /// Creates a new maintenance folder file sequence under the active authenticated tenant corporate context identifier.
    /// </summary>
    /// <param name="resource">The incoming mapping schema tracking customer requests, mechanics inputs, and initial descriptions.</param>
    /// <returns>An <see cref="IActionResult"/> with 210 Created tracking records results, or 400 Bad Request providing error string summaries.</returns>
    [HttpPost]
    public async Task<IActionResult> CreateWorkOrder([FromBody] CreateWorkOrderResource resource)
    {
        try
        {
            var tenantId = User.Claims.FirstOrDefault(c => c.Type == "WorkshopId")?.Value ?? string.Empty;
            
            var workOrder = new WorkOrder(
                tenantId, 
                resource.VehicleId, 
                resource.CustomerId, 
                resource.MechanicId,
                resource.Description, 
                resource.EstimatedDate, 
                0 
            );
            
            var result = await workOrderService.CreateAsync(workOrder);
            return StatusCode(201, result);
        }
        catch (Exception ex)
        {
            var errorMessage = ex.InnerException != null ? ex.InnerException.Message : ex.Message;
            return BadRequest(new { message = "Database internal error: " + errorMessage });
        }
    }

    /// <summary>
    /// Gathers all registered work order aggregate sequence folders matching the active authenticated workshop corporate tenant token.
    /// </summary>
    /// <returns>An <see cref="IActionResult"/> with 200 OK containing the array sequence block listings.</returns>
    [HttpGet]
    public async Task<IActionResult> GetWorkOrders()
    {
        var tenantId = User.Claims.FirstOrDefault(c => c.Type == "WorkshopId")?.Value ?? string.Empty;
        var workOrders = await workOrderService.ListByTenantIdAsync(tenantId);
        return Ok(workOrders);
    }

    /// <summary>
    /// Discovers an individual work order folder metadata block matching directly to its tracking numerical identifier row key sequence.
    /// </summary>
    /// <param name="id">The unique lookup primary index row code tracking sequence key target parameters.</param>
    /// <returns>An <see cref="IActionResult"/> with 200 OK containing the data details folder mapping payload, or 404 Not Found.</returns>
    [HttpGet("{id}")]
    public async Task<IActionResult> GetWorkOrder(int id)
    {
        var workOrder = await workOrderService.GetByIdAsync(id);
        if (workOrder == null) return NotFound();
        return Ok(workOrder);
    }

    /// <summary>
    /// Updates structural details description logs, commitment dates, billing total financial scales, progress milestones, and audit checking controls flags.
    /// </summary>
    /// <param name="id">The target tracking primary index database row reference key index code sequence.</param>
    /// <param name="resource">The validation layout model carrier transporting incoming property metrics states configuration data.</param>
    /// <returns>An <see cref="IActionResult"/> with 200 OK tracking the edited context results, or 404 Not Found status.</returns>
    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateWorkOrder(int id, [FromBody] UpdateWorkOrderResource resource)
    {
        var workOrder = new WorkOrder(
            string.Empty, 
            0, 
            0, 
            0,
            resource.Description, 
            resource.EstimatedDate, 
            resource.Price
        );
        
        workOrder.UpdateChecklist(
            resource.TasksCompleted, 
            resource.SparePartsChecked, 
            resource.DiagnosisValidated, 
            resource.CleaningDone, 
            resource.FinalTestDone
        );
        
        workOrder.UpdateStatus(resource.Status);

        var result = await workOrderService.UpdateAsync(id, workOrder);
        return result == null ? NotFound() : Ok(result);
    }

    #endregion
}
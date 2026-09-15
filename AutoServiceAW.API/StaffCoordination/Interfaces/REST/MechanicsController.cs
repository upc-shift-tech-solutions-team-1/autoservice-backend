using System.Security.Claims;
using AutoServiceAW.API.StaffCoordination.Domain.Model.Aggregates;
using AutoServiceAW.API.StaffCoordination.Domain.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AutoServiceAW.API.StaffCoordination.Interfaces.REST;

/// <summary>
/// Data Transfer Object (DTO) for creating a new mechanic record, including credentials.
/// </summary>
/// <param name="FullName">The full legal or preferred name profile value.</param>
/// <param name="Specialty">The automotive technician specialization branch indicator.</param>
/// <param name="MaxCapacity">The maximum concurrent task threshold scoring capability metric.</param>
/// <param name="Email">The communication identity tracking coordinates address.</param>
/// <param name="Password">The entry credentials registration key value.</param>
public record CreateMechanicResource(string FullName, string Specialty, int MaxCapacity, string Email, string Password);

/// <summary>
/// Data Transfer Object (DTO) for updating an existing mechanic record profile properties.
/// </summary>
/// <param name="FullName">The updated full name text value.</param>
/// <param name="Specialty">The updated core skill specialization sector.</param>
/// <param name="MaxCapacity">The updated work coordination queue threshold capability limits.</param>
/// <param name="Email">The updated platform notification contact address coordinates.</param>
public record UpdateMechanicResource(string FullName, string Specialty, int MaxCapacity, string Email);

/// <summary>
/// Exposes RESTful endpoints for managing technical staff mechanics profiles within a workshop.
/// </summary>
[ApiController]
[Route("api/v1/[controller]")]
[Authorize]
public class MechanicsController(IMechanicService mechanicService) : ControllerBase
{
    #region Methods

    /// <summary>
    /// Creates a new mechanic profile mapping under the authenticated workshop domain context.
    /// </summary>
    /// <param name="resource">The incoming model contract holding operational capacity metrics configuration bounds data.</param>
    /// <returns>An <see cref="IActionResult"/> with 201 Created and the entity data, or 400 Bad Request if an execution mismatch occurs.</returns>
    [HttpPost]
    public async Task<IActionResult> CreateMechanic([FromBody] CreateMechanicResource resource)
    {
        var workshopId = User.Claims.FirstOrDefault(c => c.Type == "WorkshopId")?.Value ?? "WS-1";
        var mechanic = new Mechanic(resource.FullName, resource.Specialty, resource.MaxCapacity, resource.Email, workshopId, resource.Password);
        
        try 
        {
            var result = await mechanicService.CreateAsync(mechanic, resource.Password, workshopId);
            return StatusCode(201, result);
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    /// <summary>
    /// Retrieves all technical staff mechanic profiles associated explicitly with the authenticated workshop scope.
    /// </summary>
    /// <returns>An <see cref="IActionResult"/> containing the sequence collection payload listing of active technicians.</returns>
    [HttpGet]
    public async Task<IActionResult> GetAllMechanics()
    {
        var workshopId = User.Claims.FirstOrDefault(c => c.Type == "WorkshopId")?.Value;
        
        var mechanics = await mechanicService.ListAsync();
        var filteredMechanics = mechanics.Where(m => m.WorkshopId == workshopId);
        
        return Ok(filteredMechanics);
    }

    /// <summary>
    /// Retrieves a technical member profile record directly matching their numerical target reference identifier key sequence.
    /// </summary>
    /// <param name="id">The unique numerical lookup profile target key index.</param>
    /// <returns>An <see cref="IActionResult"/> with 200 OK containing the profile payload, or 404 Not Found.</returns>
    [HttpGet("{id}")]
    public async Task<IActionResult> GetMechanicById(int id)
    {
        var mechanic = await mechanicService.GetByIdAsync(id);
        return mechanic == null ? NotFound() : Ok(mechanic);
    }

    /// <summary>
    /// Updates specific performance metadata attributes, contacts, or full name text values of an active technician profile.
    /// </summary>
    /// <param name="id">The unique record lookup tracking identifier index to scan.</param>
    /// <param name="resource">The modified profile parameter validation scheme mapping envelope object.</param>
    /// <returns>An <see cref="IActionResult"/> with 200 OK containing the updated result payload, or 404 Not Found.</returns>
    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateMechanic(int id, [FromBody] UpdateMechanicResource resource)
    {
        var mechanic = new Mechanic(resource.FullName, resource.Specialty, resource.MaxCapacity, resource.Email, string.Empty, string.Empty);
        var result = await mechanicService.UpdateAsync(id, mechanic);
        return result == null ? NotFound() : Ok(result);
    }

    /// <summary>
    /// Removes a physical technician registration from the workshop management directory records entirely.
    /// </summary>
    /// <param name="id">The system profile tracking index lookup target key parameters to destroy.</param>
    /// <returns>An <see cref="IActionResult"/> with 204 No Content status code payload configuration.</returns>
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteMechanic(int id)
    {
        await mechanicService.DeleteAsync(id);
        return NoContent(); 
    }

    #endregion
}
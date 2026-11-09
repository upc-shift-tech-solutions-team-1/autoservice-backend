
using System.Text.Json.Serialization;
using AutoServiceAW.API.CustomerManagement.Domain.Services;
using AutoServiceAW.API.FleetManagement.Domain.Model.Aggregates;
using AutoServiceAW.API.FleetManagement.Domain.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AutoServiceAW.API.FleetManagement.Interfaces.REST;

/// <summary>
/// DTO for creating or updating a vehicle.
/// </summary>
public record CreateVehicleResource(
    string Plate,
    string Brand,
    string Model,
    string Year,
    string Color,
    string Status,
    string Image,
    [property: JsonRequired] int CustomerId
);

/// <summary>
/// Provides vehicle management endpoints with workshop isolation.
/// </summary>
[ApiController]
[Route("api/v1/[controller]")]
[Authorize]
public class VehiclesController(
    IVehicleService vehicleService,
    ICustomerService customerService
) : ControllerBase
{
    #region Methods

    /// <summary>
    /// Creates a vehicle for a customer belonging
    /// to the authenticated user's workshop.
    /// </summary>
    [HttpPost]
    public async Task<IActionResult> CreateVehicle(
        [FromBody] CreateVehicleResource resource
    )
    {
        var workshopId = GetAuthenticatedWorkshopId();

        if (string.IsNullOrWhiteSpace(workshopId))
        {
            return Unauthorized();
        }

        if (resource.CustomerId <= 0)
        {
            return BadRequest(
                "CustomerId must be greater than zero."
            );
        }

        var customer =
            await customerService.GetByIdAsync(
                resource.CustomerId
            );

        if (customer == null ||
            customer.WorkshopId != workshopId)
        {
            return NotFound(
                "Customer not found."
            );
        }

        var vehicle = new Vehicle(
            resource.Plate,
            resource.Brand,
            resource.Model,
            resource.Year,
            resource.Color,
            resource.Status,
            resource.Image,
            resource.CustomerId
        );

        var result =
            await vehicleService.CreateAsync(vehicle);

        return StatusCode(201, result);
    }

    /// <summary>
    /// Returns only vehicles belonging to customers
    /// of the authenticated user's workshop.
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> GetAllVehicles()
    {
        var workshopId = GetAuthenticatedWorkshopId();

        if (string.IsNullOrWhiteSpace(workshopId))
        {
            return Unauthorized();
        }

        var vehicles =
            await vehicleService.ListAsync();

        var customers =
            await customerService.ListAsync();

        var workshopCustomerIds = customers
            .Where(c => c.WorkshopId == workshopId)
            .Select(c => c.Id)
            .ToHashSet();

        var filteredVehicles = vehicles
            .Where(v =>
                workshopCustomerIds.Contains(
                    v.CustomerId
                )
            );

        return Ok(filteredVehicles);
    }

    /// <summary>
    /// Retrieves a vehicle only if it belongs
    /// to the authenticated user's workshop.
    /// </summary>
    [HttpGet("{id}")]
    public async Task<IActionResult> GetVehicleById(
        int id
    )
    {
        var workshopId = GetAuthenticatedWorkshopId();

        if (string.IsNullOrWhiteSpace(workshopId))
        {
            return Unauthorized();
        }

        var vehicle =
            await vehicleService.GetByIdAsync(id);

        if (vehicle == null)
        {
            return NotFound();
        }

        var customer =
            await customerService.GetByIdAsync(
                vehicle.CustomerId
            );

        if (customer == null ||
            customer.WorkshopId != workshopId)
        {
            return NotFound();
        }

        return Ok(vehicle);
    }

    /// <summary>
    /// Updates a vehicle only when both its current
    /// owner and requested owner belong to the
    /// authenticated user's workshop.
    /// </summary>
    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateVehicle(
        int id,
        [FromBody] CreateVehicleResource resource
    )
    {
        var workshopId = GetAuthenticatedWorkshopId();

        if (string.IsNullOrWhiteSpace(workshopId))
        {
            return Unauthorized();
        }

        if (resource.CustomerId <= 0)
        {
            return BadRequest(
                "CustomerId must be greater than zero."
            );
        }

        var existingVehicle =
            await vehicleService.GetByIdAsync(id);

        if (existingVehicle == null)
        {
            return NotFound();
        }

        var existingCustomer =
            await customerService.GetByIdAsync(
                existingVehicle.CustomerId
            );

        if (existingCustomer == null ||
            existingCustomer.WorkshopId != workshopId)
        {
            return NotFound();
        }

        var newCustomer =
            await customerService.GetByIdAsync(
                resource.CustomerId
            );

        if (newCustomer == null ||
            newCustomer.WorkshopId != workshopId)
        {
            return NotFound(
                "Customer not found."
            );
        }

        var vehicle = new Vehicle(
            resource.Plate,
            resource.Brand,
            resource.Model,
            resource.Year,
            resource.Color,
            resource.Status,
            resource.Image,
            resource.CustomerId
        );

        var result =
            await vehicleService.UpdateAsync(
                id,
                vehicle
            );

        return result == null
            ? NotFound()
            : Ok(result);
    }

    /// <summary>
    /// Deletes a vehicle only when its owner belongs
    /// to the authenticated user's workshop.
    /// </summary>
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteCustomer(
        int id
    )
    {
        var workshopId = GetAuthenticatedWorkshopId();

        if (string.IsNullOrWhiteSpace(workshopId))
        {
            return Unauthorized();
        }

        var vehicle =
            await vehicleService.GetByIdAsync(id);

        if (vehicle == null)
        {
            return NotFound();
        }

        var customer =
            await customerService.GetByIdAsync(
                vehicle.CustomerId
            );

        if (customer == null ||
            customer.WorkshopId != workshopId)
        {
            return NotFound();
        }

        await vehicleService.DeleteAsync(id);

        return NoContent();
    }

    #endregion

    #region Helpers

    /// <summary>
    /// Gets the workshop identifier from
    /// the authenticated user's JWT claims.
    /// </summary>
    private string? GetAuthenticatedWorkshopId()
    {
        return User.FindFirst("WorkshopId")?.Value;
    }

    #endregion
}

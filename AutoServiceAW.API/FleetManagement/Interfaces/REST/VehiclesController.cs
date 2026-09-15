using AutoServiceAW.API.CustomerManagement.Domain.Services;
using AutoServiceAW.API.FleetManagement.Domain.Model.Aggregates;
using AutoServiceAW.API.FleetManagement.Domain.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;

namespace AutoServiceAW.API.FleetManagement.Interfaces.REST;

/// <summary>
/// Data Transfer Object (DTO) for creating or updating a vehicle resource.
/// </summary>
/// <param name="Plate">The license plate number of the vehicle.</param>
/// <param name="Brand">The manufacturer or brand of the vehicle.</param>
/// <param name="Model">The specific model of the vehicle.</param>
/// <param name="year">The manufacturing year of the vehicle.</param>
/// <param name="Color">The exterior color of the vehicle.</param>
/// <param name="Status">The current operational or repair status.</param>
/// <param name="Image">The image URL or path resource identifier.</param>
/// <param name="CustomerId">The unique identifier of the customer who owns the vehicle.</param>
public record CreateVehicleResource(string Plate, string Brand, string Model, string Year, string Color, string Status, string Image, int CustomerId);

/// <summary>
/// Exposes RESTful endpoints for managing vehicles inside the fleet context.
/// </summary>
[ApiController]
[Route("api/v1/[controller]")]
[Authorize]
public class VehiclesController(IVehicleService vehicleService, ICustomerService customerService) : ControllerBase
{
    #region Methods

    /// <summary>
    /// Registers a new vehicle into the fleet management context.
    /// </summary>
    /// <param name="resource">The vehicle data transfer object containing entry details.</param>
    /// <returns>An <see cref="IActionResult"/> with a 201 Created status code and the created vehicle data.</returns>
    [HttpPost]
    public async Task<IActionResult> CreateVehicle([FromBody] CreateVehicleResource resource)
    {
        var vehicle = new Vehicle(resource.Plate, resource.Brand, resource.Model, resource.Year, resource.Color, resource.Status, resource.Image, resource.CustomerId);
        var result = await vehicleService.CreateAsync(vehicle);
        return StatusCode(201, result);
    }

    /// <summary>
    /// Retrieves all vehicles that belong to customers associated with the authenticated user's workshop.
    /// </summary>
    /// <returns>An <see cref="IActionResult"/> containing the list of filtered vehicles, or 401 Unauthorized if the token is invalid.</returns>
    [HttpGet]
    public async Task<IActionResult> GetAllVehicles()
    {
        var workshopId = User.Claims.FirstOrDefault(c => c.Type == "WorkshopId")?.Value;
        if (string.IsNullOrEmpty(workshopId)) return Unauthorized();

        var vehicles = await vehicleService.ListAsync();
        var customers = await customerService.ListAsync();
        
        var workshopCustomerIds = customers
            .Where(c => c.WorkshopId == workshopId)
            .Select(c => c.Id)
            .ToList();

        var filteredVehicles = vehicles.Where(v => workshopCustomerIds.Contains(v.CustomerId));
        
        return Ok(filteredVehicles);
    }
    
    /// <summary>
    /// Retrieves a specific vehicle by its unique identifier.
    /// </summary>
    /// <param name="id">The unique identifier of the vehicle.</param>
    /// <returns>An <see cref="IActionResult"/> with 200 OK and the vehicle data, or 404 Not Found if the vehicle does not exist.</returns>
    [HttpGet("{id}")]
    public async Task<IActionResult> GetVehicleById(int id)
    {
        var vehicle = await vehicleService.GetByIdAsync(id);
        return vehicle == null ? NotFound() : Ok(vehicle);
    }

    /// <summary>
    /// Updates an existing vehicle's technical details and owner information.
    /// </summary>
    /// <param name="id">The unique identifier of the vehicle to update.</param>
    /// <param name="resource">The vehicle data transfer object containing updated details.</param>
    /// <returns>An <see cref="IActionResult"/> with 200 OK and the updated vehicle data, or 404 Not Found if the update failed.</returns>
    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateVehicle(int id, [FromBody] CreateVehicleResource resource)
    {
        var vehicle = new Vehicle(resource.Plate, resource.Brand, resource.Model, resource.Year, resource.Color, resource.Status, resource.Image, resource.CustomerId);
        var result = await vehicleService.UpdateAsync(id, vehicle);
        return result == null ? NotFound() : Ok(result);
    }

    /// <summary>
    /// Deletes a vehicle from the fleet database by its unique identifier.
    /// </summary>
    /// <param name="id">The unique identifier of the vehicle to remove.</param>
    /// <returns>An <see cref="IActionResult"/> with 204 No Content status code.</returns>
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteCustomer(int id)
    {
        await vehicleService.DeleteAsync(id);
        return NoContent();
    }

    #endregion
}
using AutoServiceAW.API.CustomerManagement.Domain.Model.Aggregates;
using AutoServiceAW.API.CustomerManagement.Domain.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;

namespace AutoServiceAW.API.CustomerManagement.Interfaces.REST;

/// <summary>
/// Data Transfer Object (DTO) for creating a new customer.
/// </summary>
/// <param name="FullName">The full name of the customer.</param>
/// <param name="Dni">The National Identity Document (DNI) of the customer.</param>
/// <param name="Email">The email address of the customer.</param>
/// <param name="Phone">The phone number of the customer.</param>
public record CreateCustomerResource(string FullName, string Dni, string Email, string Phone);

/// <summary>
/// Data Transfer Object (DTO) for updating an existing customer's details.
/// </summary>
/// <param name="FullName">The updated full name of the customer.</param>
/// <param name="Dni">The updated National Identity Document (DNI) of the customer.</param>
/// <param name="Email">The updated email address of the customer.</param>
/// <param name="Phone">The updated phone number of the customer.</param>
public record UpdateCustomerResource(string FullName, string Dni, string Email, string Phone);

/// <summary>
/// Exposes RESTful endpoints for managing customers within the API.
/// </summary>
[ApiController]
[Route("api/v1/[controller]")]
public class CustomersController(ICustomerService customerService) : ControllerBase
{
    #region Methods

    /// <summary>
    /// Creates a new customer associated with the workshop of the authenticated user.
    /// </summary>
    /// <param name="resource">The customer data transfer object containing creation details.</param>
    /// <returns>An <see cref="IActionResult"/> with a 201 Created status code and the created customer data.</returns>
    [HttpPost]
    public async Task<IActionResult> CreateCustomer([FromBody] CreateCustomerResource resource)
    {
        var workshopId = User.FindFirst("WorkshopId")?.Value ?? "WS-1"; // Sacado del Token
        var customer = new Customer(workshopId, resource.FullName, resource.Dni, resource.Email, resource.Phone);
        
        var result = await customerService.CreateAsync(customer);
        return StatusCode(201, result);
    }

    /// <summary>
    /// Retrieves all customers filtered by the workshop ID of the authenticated user.
    /// </summary>
    /// <returns>An <see cref="IActionResult"/> containing the list of filtered customers.</returns>
    [HttpGet]
    public async Task<IActionResult> GetAllCustomers()
    {
        var workshopId = User.FindFirst("WorkshopId")?.Value;
        var customers = await customerService.ListAsync();
        
        var filteredCustomers = customers.Where(c => c.WorkshopId == workshopId);
        return Ok(filteredCustomers);
    }
    
    /// <summary>
    /// Retrieves a specific customer by their unique identifier.
    /// </summary>
    /// <param name="id">The unique identifier of the customer.</param>
    /// <returns>An <see cref="IActionResult"/> with 200 OK and the customer data, or 404 Not Found if the customer does not exist.</returns>
    [HttpGet("{id}")]
    public async Task<IActionResult> GetCustomerById(int id)
    {
        var customer = await customerService.GetByIdAsync(id);
        return customer == null ? NotFound() : Ok(customer);
    }

    /// <summary>
    /// Updates an existing customer's details by their unique identifier.
    /// </summary>
    /// <param name="id">The unique identifier of the customer to update.</param>
    /// <param name="resource">The customer data transfer object containing updated details.</param>
    /// <returns>An <see cref="IActionResult"/> with 200 OK and the updated customer data, or 404 Not Found if the update failed.</returns>
    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateCustomer(int id, [FromBody] UpdateCustomerResource resource)
    {
        var customerToUpdate = new Customer("", resource.FullName, resource.Dni, resource.Email, resource.Phone);
        var result = await customerService.UpdateAsync(id, customerToUpdate);
        return result == null ? NotFound() : Ok(result);
    }

    /// <summary>
    /// Deletes a customer by their unique identifier.
    /// </summary>
    /// <param name="id">The unique identifier of the customer to remove.</param>
    /// <returns>An <see cref="IActionResult"/> with 204 No Content status code.</returns>
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteCustomer(int id)
    {
        await customerService.DeleteAsync(id);
        return NoContent();
    }

    #endregion
}
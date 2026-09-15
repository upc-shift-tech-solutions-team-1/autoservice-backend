using AutoServiceAW.API.IAM.Domain.Services;
using AutoServiceAW.API.TenantManagement.Domain.Model.Aggregates;
using AutoServiceAW.API.TenantManagement.Domain.Services;
using AutoServiceAW.API.StaffCoordination.Domain.Repositories;
using Microsoft.AspNetCore.Mvc;

namespace AutoServiceAW.API.IAM.Interfaces.REST;

/// <summary>
/// Data Transfer Object (DTO) for authenticating an existing user.
/// </summary>
/// <param name="Email">The registered email address of the user.</param>
/// <param name="Password">The plain-text authentication password.</param>
public record SignInResource(string Email, string Password);

/// <summary>
/// Data Transfer Object (DTO) for registering a new user manually under an existing workshop.
/// </summary>
/// <param name="Email">The unique email address for the user.</param>
/// <param name="Password">The plain-text password to be encrypted.</param>
/// <param name="Role">The system security role configuration.</param>
/// <param name="WorkshopId">The target workshop identifier context.</param>
public record SignUpResource(string Email, string Password, string Role, string WorkshopId);

/// <summary>
/// Data Transfer Object (DTO) for simultaneously registering a new workshop and its admin user account.
/// </summary>
/// <param name="WorkshopName">The business name of the new workshop.</param>
/// <param name="Email">The administrative user's email address.</param>
/// <param name="Password">The administrative user's security password.</param>
public record SignUpWorkshopResource(string WorkshopName, string Email, string Password);

/// <summary>
/// Exposes RESTful authentication endpoints for the Identity and Access Management (IAM) context.
/// </summary>
[ApiController]
[Route("api/v1/[controller]")]
public class AuthController(
    IAuthService authService,
    IWorkshopService workshopService,
    IMechanicRepository mechanicRepository) : ControllerBase
{
    #region Methods

    /// <summary>
    /// Validates user credentials and returns an authentication token along with context-specific data.
    /// </summary>
    /// <param name="resource">The login credentials resource data transfer object.</param>
    /// <returns>An <see cref="IActionResult"/> with 200 OK and the token envelope, or 401 Unauthorized on authentication failure.</returns>
    [HttpPost("sign-in")]
    public async Task<IActionResult> SignIn([FromBody] SignInResource resource)
    {
        var result = await authService.SignInAsync(resource.Email, resource.Password);

        if (result == null)
            return Unauthorized(new { message = "Email or password is incorrect" });

        int? mechanicId = null;

        if (result.Value.User.Role == "mechanic")
        {
            var mechanic = await mechanicRepository.FindByEmailAsync(result.Value.User.Email);

            if (mechanic != null)
                mechanicId = mechanic.Id;
        }

        return Ok(new
        {
            id = result.Value.User.Id,
            email = result.Value.User.Email,
            role = result.Value.User.Role,
            workshopId = result.Value.User.WorkshopId,
            mechanicId = mechanicId,
            token = result.Value.Token
        });
    }

    /// <summary>
    /// Registers a new user manually inside an existing business scope context.
    /// </summary>
    /// <param name="resource">The profile entry registration data transfer object.</param>
    /// <returns>An <see cref="IActionResult"/> with 201 Created and the unique identifier, or 400 Bad Request if an operational error occurs.</returns>
    [HttpPost("sign-up")]
    public async Task<IActionResult> SignUp([FromBody] SignUpResource resource)
    {
        try
        {
            var user = await authService.SignUpAsync(resource.Email, resource.Password, resource.Role, resource.WorkshopId);
            return StatusCode(201, new { message = "User created successfully", userId = user?.Id });
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    /// <summary>
    /// Executes a master transactional routine creating both a tenant workshop structure and its primary admin identity profile.
    /// </summary>
    /// <param name="resource">The composite setup structure parameters resource object.</param>
    /// <returns>An <see cref="IActionResult"/> with 201 Created status code and relationship summaries, or 400 Bad Request on failure.</returns>
    [HttpPost("register-workshop")]
    public async Task<IActionResult> RegisterWorkshop([FromBody] SignUpWorkshopResource resource)
    {
        try
        {
            // Creamos el taller
            var workshop = new Workshop(resource.WorkshopName);
            var createdWorkshop = await workshopService.CreateAsync(workshop);

            if (createdWorkshop == null) return BadRequest(new { message = "Could not create workshop" });

            // Creamos al admin
            var user = await authService.SignUpAsync(resource.Email, resource.Password, "admin", createdWorkshop.TenantId);

            return StatusCode(201, new 
            { 
                message = "Workshop and admin account created successfully", 
                workshopId = createdWorkshop.TenantId,
                userId = user?.Id 
            });
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    #endregion
}
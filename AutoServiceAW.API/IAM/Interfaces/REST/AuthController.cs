using AutoServiceAW.API.IAM.Domain.Model.ValueObjects;
using AutoServiceAW.API.IAM.Domain.Services;
using AutoServiceAW.API.StaffCoordination.Domain.Repositories;
using AutoServiceAW.API.TenantManagement.Domain.Model.Aggregates;
using AutoServiceAW.API.TenantManagement.Domain.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AutoServiceAW.API.IAM.Interfaces.REST;

/// <summary>
/// Data Transfer Object (DTO) for authenticating an existing user.
/// </summary>
/// <param name="Email">The registered email address of the user.</param>
/// <param name="Password">The authentication password.</param>
public record SignInResource(
    string Email,
    string Password
);

/// <summary>
/// Data Transfer Object (DTO) for registering a user
/// inside the authenticated administrator's workshop.
/// </summary>
/// <param name="Email">The unique email address for the user.</param>
/// <param name="Password">The password to be securely processed.</param>
/// <param name="Role">The system authorization role.</param>
/// <param name="WorkshopId">
/// Legacy field kept for client compatibility.
/// The server determines the workshop from the authenticated JWT.
/// </param>
public record SignUpResource(
    string Email,
    string Password,
    string Role,
    string WorkshopId
);

/// <summary>
/// Data Transfer Object (DTO) for registering a new
/// workshop together with its administrator account.
/// </summary>
/// <param name="WorkshopName">
/// The business name of the workshop.
/// </param>
/// <param name="Email">
/// The administrator email address.
/// </param>
/// <param name="Password">
/// The administrator password.
/// </param>
public record SignUpWorkshopResource(
    string WorkshopName,
    string Email,
    string Password
);

/// <summary>
/// Exposes RESTful authentication endpoints for the
/// Identity and Access Management bounded context.
/// </summary>
[ApiController]
[Route("api/v1/[controller]")]
public class AuthController(
    IAuthService authService,
    IWorkshopService workshopService,
    IMechanicRepository mechanicRepository
) : ControllerBase
{
    #region Methods

    /// <summary>
    /// Authenticates an existing user and returns
    /// identity, workshop and authorization information.
    /// </summary>
    /// <param name="resource">
    /// User authentication credentials.
    /// </param>
    /// <returns>
    /// 200 OK with authentication information,
    /// or 401 Unauthorized when credentials are invalid.
    /// </returns>
    [AllowAnonymous]
    [HttpPost("sign-in")]
    public async Task<IActionResult> SignIn(
        [FromBody] SignInResource resource
    )
    {
        var result =
            await authService.SignInAsync(
                resource.Email,
                resource.Password
            );

        if (result == null)
        {
            return Unauthorized(
                new
                {
                    message =
                        "Email or password is incorrect"
                }
            );
        }

        int? mechanicId = null;

        if (string.Equals(
                result.Value.User.Role,
                UserRole.Mechanic,
                StringComparison.OrdinalIgnoreCase
            ))
        {
            var mechanic =
                await mechanicRepository.FindByEmailAsync(
                    result.Value.User.Email
                );

            if (mechanic != null)
            {
                mechanicId = mechanic.Id;
            }
        }

        return Ok(
            new
            {
                id = result.Value.User.Id,
                email = result.Value.User.Email,
                role = result.Value.User.Role,
                workshopId =
                    result.Value.User.WorkshopId,
                mechanicId,
                token = result.Value.Token
            }
        );
    }

    /// <summary>
    /// Registers a user inside the workshop associated
    /// with the authenticated administrator.
    /// </summary>
    /// <param name="resource">
    /// User registration information.
    /// </param>
    /// <returns>
    /// 201 Created when registration succeeds,
    /// 401 when the workshop context is unavailable,
    /// or 400 when validation fails.
    /// </returns>
    [Authorize(Roles = UserRole.Admin)]
    [HttpPost("sign-up")]
    public async Task<IActionResult> SignUp(
        [FromBody] SignUpResource resource
    )
    {
        try
        {
            var workshopId =
                User.FindFirst("WorkshopId")?.Value;

            if (string.IsNullOrWhiteSpace(workshopId))
            {
                return Unauthorized(
                    new
                    {
                        message =
                            "Workshop context is missing."
                    }
                );
            }

            var user =
                await authService.SignUpAsync(
                    resource.Email,
                    resource.Password,
                    resource.Role,
                    workshopId
                );

            return StatusCode(
                StatusCodes.Status201Created,
                new
                {
                    message =
                        "User created successfully",
                    userId = user?.Id
                }
            );
        }
        catch (ArgumentException ex)
        {
            return BadRequest(
                new
                {
                    message = ex.Message
                }
            );
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(
                new
                {
                    message = ex.Message
                }
            );
        }
    }

    /// <summary>
    /// Registers a new workshop together with its
    /// initial administrator identity.
    /// </summary>
    /// <param name="resource">
    /// Workshop and administrator registration data.
    /// </param>
    /// <returns>
    /// 201 Created when both records are created,
    /// otherwise 400 Bad Request.
    /// </returns>
    [AllowAnonymous]
    [HttpPost("register-workshop")]
    public async Task<IActionResult> RegisterWorkshop(
        [FromBody] SignUpWorkshopResource resource
    )
    {
        try
        {
            var workshop =
                new Workshop(resource.WorkshopName);

            var createdWorkshop =
                await workshopService.CreateAsync(
                    workshop
                );

            if (createdWorkshop == null)
            {
                return BadRequest(
                    new
                    {
                        message =
                            "Could not create workshop"
                    }
                );
            }

            var user =
                await authService.SignUpAsync(
                    resource.Email,
                    resource.Password,
                    UserRole.Admin,
                    createdWorkshop.TenantId
                );

            return StatusCode(
                StatusCodes.Status201Created,
                new
                {
                    message =
                        "Workshop and admin account created successfully",
                    workshopId =
                        createdWorkshop.TenantId,
                    userId = user?.Id
                }
            );
        }
        catch (ArgumentException ex)
        {
            return BadRequest(
                new
                {
                    message = ex.Message
                }
            );
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(
                new
                {
                    message = ex.Message
                }
            );
        }
    }

    #endregion
}
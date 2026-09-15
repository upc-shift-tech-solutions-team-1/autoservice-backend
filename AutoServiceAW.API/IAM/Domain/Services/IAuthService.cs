using AutoServiceAW.API.IAM.Domain.Model.Aggregates;

namespace AutoServiceAW.API.IAM.Domain.Services;

/// <summary>
/// Defines the service contract for handling authentication, credentials verification, and registration processes.
/// </summary>
public interface IAuthService
{
    #region Methods

    /// <summary>
    /// Coordinates the sign-up process for registering a new user within the system.
    /// </summary>
    /// <param name="email">The unique email for the new identity.</param>
    /// <param name="password">The plain text password to be processed.</param>
    /// <param name="role">The security role mapping.</param>
    /// <param name="workshopId">The workshop identity assignment.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains the created <see cref="User"/> entity.</returns>
    Task<User?> SignUpAsync(string email, string password, string role, string workshopId);

    /// <summary>
    /// Coordinates the sign-in process for validating credentials and logging a user into the platform.
    /// </summary>
    /// <param name="email">The user email address.</param>
    /// <param name="password">The plain text security password.</param>
    /// <returns>
    /// A task that represents the asynchronous operation. The task result contains a tuple containing the valid <see cref="User"/> 
    /// and their bearer access Token string, or <see langword="null"/> if authentication fails.
    /// </returns>
    Task<(User User, string Token)?> SignInAsync(string email, string password);

    #endregion
}
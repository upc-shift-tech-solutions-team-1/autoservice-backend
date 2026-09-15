using AutoServiceAW.API.IAM.Domain.Model.Aggregates;
using AutoServiceAW.API.Shared.Domain.Repositories;

namespace AutoServiceAW.API.IAM.Domain.Repositories;

/// <summary>
/// Defines the repository contract for managing <see cref="User"/> entities.
/// Inherits basic CRUD operations from <see cref="IBaseRepository{User}"/>.
/// </summary>
public interface IUserRepository : IBaseRepository<User>
{
    #region Methods

    /// <summary>
    /// Searches for a user entity by their unique email address asynchronously.
    /// </summary>
    /// <param name="email">The email address to search for.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains the matching <see cref="User"/>, or <see langword="null"/> if no user is found.</returns>
    Task<User?> FindByEmailAsync(string email);

    #endregion
}
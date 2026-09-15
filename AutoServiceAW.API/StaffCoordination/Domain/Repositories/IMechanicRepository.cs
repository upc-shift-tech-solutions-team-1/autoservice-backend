using AutoServiceAW.API.Shared.Domain.Repositories;
using AutoServiceAW.API.StaffCoordination.Domain.Model.Aggregates;

namespace AutoServiceAW.API.StaffCoordination.Domain.Repositories;

/// <summary>
/// Defines the repository contract for managing <see cref="Mechanic"/> entities.
/// Inherits basic CRUD operations from <see cref="IBaseRepository{Mechanic}"/>.
/// </summary>
public interface IMechanicRepository : IBaseRepository<Mechanic>
{
    #region Methods

    /// <summary>
    /// Searches for a unique mechanic entry configuration by querying their identity tracking email address asynchronously.
    /// </summary>
    /// <param name="email">The reference email criteria to evaluate.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains the matching <see cref="Mechanic"/>, or <see langword="null"/>.</returns>
    Task<Mechanic?> FindByEmailAsync(string email);

    #endregion
}
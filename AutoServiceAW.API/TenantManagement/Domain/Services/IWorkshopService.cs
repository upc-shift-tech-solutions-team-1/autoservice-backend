using AutoServiceAW.API.TenantManagement.Domain.Model.Aggregates;

namespace AutoServiceAW.API.TenantManagement.Domain.Services;

/// <summary>
/// Defines the service contract for coordinating structural tenant workshop registration processes.
/// </summary>
public interface IWorkshopService
{
    #region Methods

    /// <summary>
    /// Coordinates the physical creation routine steps of a multi-tenant workshop architecture model.
    /// </summary>
    /// <param name="workshop">The target initialization parameters structure data context.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains the created <see cref="Workshop"/> instance model layer.</returns>
    Task<Workshop?> CreateAsync(Workshop workshop);

    #endregion
}
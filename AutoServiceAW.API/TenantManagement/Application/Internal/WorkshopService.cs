using AutoServiceAW.API.Shared.Domain.Repositories;
using AutoServiceAW.API.TenantManagement.Domain.Model.Aggregates;
using AutoServiceAW.API.TenantManagement.Domain.Repositories;
using AutoServiceAW.API.TenantManagement.Domain.Services;

namespace AutoServiceAW.API.TenantManagement.Application.Internal;

/// <summary>
/// Provides internal application services for managing tenant workshop structures and initialization.
/// </summary>
public class WorkshopService(IWorkshopRepository workshopRepository, IUnitOfWork unitOfWork) : IWorkshopService
{
    #region Methods

    /// <summary>
    /// Creates a new tenant workshop structure inside the system domain asynchronously.
    /// </summary>
    /// <param name="workshop">The workshop aggregate root metadata blueprint instance.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains the created <see cref="Workshop"/> instance.</returns>
    public async Task<Workshop?> CreateAsync(Workshop workshop)
    {
        await workshopRepository.AddAsync(workshop);
        await unitOfWork.CompleteAsync();
        return workshop;
    }

    #endregion
}
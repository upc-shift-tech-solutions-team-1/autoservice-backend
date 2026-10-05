using AutoServiceAW.API.Shared.Domain.Repositories;
using AutoServiceAW.API.WorkshopOperations.Domain.Model.Aggregates;

namespace AutoServiceAW.API.WorkshopOperations.Domain.Repositories;

/// <summary>
/// Defines the repository contract for managing <see cref="WorkOrder"/> entities.
/// Inherits basic CRUD operations from <see cref="IBaseRepository{WorkOrder}"/>.
/// </summary>
public interface IWorkOrderRepository : IBaseRepository<WorkOrder>
{
    #region Methods

    /// <summary>
    /// Retrieves all work orders belonging to a specific tenant workshop boundary identifier asynchronously.
    /// </summary>
    /// <param name="workshopId">The custom string tenant tracking identification code.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains an enumerable collection of matching <see cref="WorkOrder"/> aggregates.</returns>
    Task<IEnumerable<WorkOrder>> FindByWorkshopIdAsync(string workshopId);

    /// <summary>Finds a work order by its public tracking code and loads its status history.</summary>
    Task<WorkOrder?> FindByTrackingCodeWithHistoryAsync(string trackingCode);

    /// <summary>Finds a work order by ID and loads its status history for status updates.</summary>
    Task<WorkOrder?> FindByIdWithHistoryAsync(int id);

    #endregion
}

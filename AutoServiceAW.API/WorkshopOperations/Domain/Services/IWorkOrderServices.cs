using AutoServiceAW.API.WorkshopOperations.Domain.Model.Aggregates;

namespace AutoServiceAW.API.WorkshopOperations.Domain.Services;

/// <summary>
/// Defines the application service contract for coordinating high-level vehicle maintenance folders and tracking validation checklists.
/// </summary>
public interface IWorkOrderService
{
    #region Methods

    /// <summary>
    /// Coordinates structural validation bounds needed to persist a new maintenance work folder tracker pipeline.
    /// </summary>
    /// <param name="workOrder">The core work order initial parameter template layout domain instance.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains the saved context structure metrics.</returns>
    System.Threading.Tasks.Task<WorkOrder?> CreateAsync(WorkOrder workOrder);

    /// <summary>
    /// Coordinates the retrieval compilation layer gathering every registered work order folder across data repositories.
    /// </summary>
    /// <returns>A task that represents the asynchronous operation. The task result contains an enumerable list mapping global work orders.</returns>
    System.Threading.Tasks.Task<IEnumerable<WorkOrder>> ListAsync();

    /// <summary>
    /// Coordinates query pipeline structures to aggregate work order collections isolated explicitly under a targeted tenant shop zone.
    /// </summary>
    /// <param name="tenantId">The custom string corporate tenant identity token parameter mapping index tool.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains the tenant tracking collection folder array.</returns>
    System.Threading.Tasks.Task<IEnumerable<WorkOrder>> ListByTenantIdAsync(string tenantId);

    /// <summary>
    /// Discovers an individual work order tracking folder directly targeting its numerical internal lookup key row sequence.
    /// </summary>
    /// <param name="id">The core search criteria tracking integer row primary index key parameters.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains the matching asset data instance envelope model, or null.</returns>
    System.Threading.Tasks.Task<WorkOrder?> GetByIdAsync(int id);

    /// <summary>
    /// Coordinates complete synchronization transformations over baseline descriptions, milestones, checklists, and macro states.
    /// </summary>
    /// <param name="id">The targeted lookup tracking identifier index parameters to match and alter.</param>
    /// <param name="workOrder">The temporary instance layout container carrying modification parameter updates.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains the updated entity tracking state result structure.</returns>
    System.Threading.Tasks.Task<WorkOrder?> UpdateAsync(int id, WorkOrder workOrder);

    /// <summary>
    /// Coordinates complete removal workflows erasing a vehicle tracking work order record completely from persistence registers.
    /// </summary>
    /// <param name="id">The system service folder identification index reference criteria to wipe.</param>
    /// <returns>A task that represents the asynchronous operation handling promise resolution blocks.</returns>
    System.Threading.Tasks.Task DeleteAsync(int id);

    #endregion
}
using Task = AutoServiceAW.API.WorkshopOperations.Domain.Model.Aggregates.Task;

namespace AutoServiceAW.API.WorkshopOperations.Domain.Services;

/// <summary>
/// Defines the application service contract for coordinating individual task lifecycles, tracking statuses, and mapping technician feedback.
/// </summary>
public interface ITaskService
{
    #region Methods

    /// <summary>
    /// Coordinates the persistence mapping of a newly requested operational workshop task.
    /// </summary>
    /// <param name="task">The task domain aggregate data packet reference.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains the registered entity state mapping.</returns>
    System.Threading.Tasks.Task<Task?> CreateAsync(Task task);

    /// <summary>
    /// Coordinates the retrieval collection pipeline for every registered task across the application infrastructure.
    /// </summary>
    /// <returns>A task that represents the asynchronous operation. The task result contains an enumerable sequence collection of tasks.</returns>
    System.Threading.Tasks.Task<IEnumerable<Task>> ListAsync();

    /// <summary>
    /// Coordinates the query extraction filter layer to gather tasks matching a specific work order parent node.
    /// </summary>
    /// <param name="workOrderId">The numerical parent structural tracker code index filter.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains the matching tasks collection sequence.</returns>
    System.Threading.Tasks.Task<IEnumerable<Task>> ListByWorkOrderIdAsync(int workOrderId);

    /// <summary>
    /// Coordinates the query extraction filter layer to gather tasks currently assigned to a unique technician operator.
    /// </summary>
    /// <param name="mechanicId">The target technician operator tracking database integer key.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains the collection sequence assigned to the operator.</returns>
    System.Threading.Tasks.Task<IEnumerable<Task>> ListByMechanicIdAsync(int mechanicId);

    /// <summary>
    /// Discovers an individual task record match tracking strictly to its baseline index identifier key sequence.
    /// </summary>
    /// <param name="id">The lookup identifier database target parameter index key.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains the mapped domain model envelope data, or null.</returns>
    System.Threading.Tasks.Task<Task?> GetByIdAsync(int id);

    /// <summary>
    /// Coordinates full modifications over foundational task instruction schemas, metrics configurations, and allocations.
    /// </summary>
    /// <param name="id">The target database record identifier key sequence to update.</param>
    /// <param name="task">The altered structural metrics parameters layout data mapping.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains the modified model result envelope state.</returns>
    System.Threading.Tasks.Task<Task?> UpdateAsync(int id, Task task);

    /// <summary>
    /// Coordinates precise atomic changes over technician operational feedback trackers, diagnostics logs, and proof records.
    /// </summary>
    /// <param name="id">The target database task row reference tracking index.</param>
    /// <param name="status">The updated progress workflow status text string label.</param>
    /// <param name="diagnosis">The mechanic's diagnostic analysis summary notes logging text.</param>
    /// <param name="explanation">The customer transparency explanation text narrative parameters.</param>
    /// <param name="observation">The internal-only technician workshop auditing metrics notes text.</param>
    /// <param name="evidence">The media database storage access location reference paths string.</param>
    /// <param name="reviewStatus">The supervisory administration validation approval phase marker state string.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains the patched entity model state instance execution.</returns>
    System.Threading.Tasks.Task<Task?> PatchStatusAsync(int id, string status, string diagnosis, string explanation, string observation, string evidence, string reviewStatus);

    /// <summary>
    /// Coordinates target wipe execution sequences to drop a concrete task completely from tracking indexes.
    /// </summary>
    /// <param name="id">The target row identification index structural parameter to wipe.</param>
    /// <returns>A task that represents the asynchronous operation flow executor lifecycle.</returns>
    System.Threading.Tasks.Task DeleteAsync(int id);

    #endregion
}
using AutoServiceAW.API.Shared.Domain.Repositories;
using AutoServiceAW.API.WorkshopOperations.Domain.Repositories;
using AutoServiceAW.API.WorkshopOperations.Domain.Services;
using WorkshopTask =
    AutoServiceAW.API.WorkshopOperations.Domain.Model.Aggregates.Task;

namespace AutoServiceAW.API.WorkshopOperations.Application.Internal;

/// <summary>
/// Provides internal application services for managing individual diagnostic
/// and repair tasks within workshop operations.
/// </summary>
public class TaskService(
    ITaskRepository taskRepository,
    IUnitOfWork unitOfWork
) : ITaskService
{
    #region Methods

    /// <summary>
    /// Creates a new operational task in the system asynchronously.
    /// </summary>
    /// <param name="task">
    /// The task entity aggregate root containing initial structural
    /// specifications.
    /// </param>
    /// <returns>
    /// A task that represents the asynchronous operation.
    /// The task result contains the created task entity.
    /// </returns>
    public async Task<WorkshopTask?> CreateAsync(
        WorkshopTask task
    )
    {
        await taskRepository.AddAsync(task);
        await unitOfWork.CompleteAsync();

        return task;
    }

    /// <summary>
    /// Retrieves all operational tasks registered globally across the
    /// network asynchronously, including their associated parts.
    /// </summary>
    /// <returns>
    /// A task that represents the asynchronous operation.
    /// The task result contains an enumerable collection of task entities.
    /// </returns>
    public async Task<IEnumerable<WorkshopTask>> ListAsync()
    {
        return await taskRepository.ListWithPartsAsync();
    }

    /// <summary>
    /// Retrieves all operational tasks grouped under a specific parent
    /// work order tracker asynchronously.
    /// </summary>
    /// <param name="workOrderId">
    /// The unique identifier of the target work order.
    /// </param>
    /// <returns>
    /// A task that represents the asynchronous operation.
    /// The task result contains an enumerable collection of matching
    /// task entities.
    /// </returns>
    public async Task<IEnumerable<WorkshopTask>>
        ListByWorkOrderIdAsync(
            int workOrderId
        )
    {
        return await taskRepository
            .FindByWorkOrderIdAsync(workOrderId);
    }

    /// <summary>
    /// Retrieves all operational tasks currently assigned to a specific
    /// mechanic technician asynchronously.
    /// </summary>
    /// <param name="mechanicId">
    /// The unique identifier of the target mechanic technical user.
    /// </param>
    /// <returns>
    /// A task that represents the asynchronous operation.
    /// The task result contains an enumerable collection of matching
    /// task entities.
    /// </returns>
    public async Task<IEnumerable<WorkshopTask>>
        ListByMechanicIdAsync(
            int mechanicId
        )
    {
        return await taskRepository
            .FindByMechanicIdAsync(mechanicId);
    }

    /// <summary>
    /// Retrieves a specific operational task by its unique identifier
    /// tracking key asynchronously, including its associated parts.
    /// </summary>
    /// <param name="id">
    /// The unique identifier integer key of the task row.
    /// </param>
    /// <returns>
    /// A task that represents the asynchronous operation.
    /// The task result contains the found task, or null.
    /// </returns>
    public async Task<WorkshopTask?> GetByIdAsync(
        int id
    )
    {
        return await taskRepository
            .FindByIdWithPartsAsync(id);
    }

    /// <summary>
    /// Updates structural metadata, time distributions, pricing metrics,
    /// and mechanics allocation states asynchronously.
    /// </summary>
    public async Task<WorkshopTask?> UpdateAsync(
        int id,
        WorkshopTask updatedTask
    )
    {
        var existingTask = await taskRepository
            .FindByIdWithPartsAsync(id);

        if (existingTask == null)
        {
            return null;
        }

        existingTask.Update(
            updatedTask.Description,
            updatedTask.Status,
            updatedTask.Priority,
            updatedTask.EstimatedTime,
            updatedTask.LaborPrice,
            updatedTask.MechanicId,
            updatedTask.LaborCost
        );
        existingTask.Parts.Clear();
        
        if (updatedTask.Parts != null && updatedTask.Parts.Any())
        {
            foreach (var newPart in updatedTask.Parts)
            {
                existingTask.Parts.Add(newPart);
            }
        }

        // 3. Guardamos los cambios
        taskRepository.Update(existingTask);
        await unitOfWork.CompleteAsync();

        return existingTask;
    }

    /// <summary>
    /// Patches technical diagnostics indicators, observations, review
    /// feedback flags, and proof evidence tracking logs asynchronously.
    /// </summary>
    /// <param name="id">
    /// The unique identifier of the operational task to patch.
    /// </param>
    /// <param name="status">
    /// The updated progress status indicator string state.
    /// </param>
    /// <param name="diagnosis">
    /// The technical diagnostic analysis descriptive log summaries.
    /// </param>
    /// <param name="explanation">
    /// The customer-facing description explaining the functional damage
    /// or status updates.
    /// </param>
    /// <param name="observation">
    /// The internal-only technician workshop observation logging notes.
    /// </param>
    /// <param name="evidence">
    /// The media reference or digital link evidence logs captured for
    /// customer transparency validation.
    /// </param>
    /// <param name="reviewStatus">
    /// The administration supervisor review authorization evaluation
    /// status value.
    /// </param>
    /// <returns>
    /// A task that represents the asynchronous operation.
    /// The task result contains the patched task entity, or null.
    /// </returns>
    public async Task<WorkshopTask?> PatchStatusAsync(
        int id,
        string status,
        string diagnosis,
        string explanation,
        string observation,
        string evidence,
        string reviewStatus
    )
    {
        var existingTask = await taskRepository
            .FindByIdWithPartsAsync(id);

        if (existingTask == null)
        {
            return null;
        }

        var resolvedStatus =
            string.IsNullOrWhiteSpace(status)
                ? existingTask.Status
                : status;

        var resolvedDiagnosis =
            string.IsNullOrWhiteSpace(diagnosis)
                ? existingTask.TechnicalDiagnosis
                : diagnosis;

        var resolvedExplanation =
            string.IsNullOrWhiteSpace(explanation)
                ? existingTask.CustomerExplanation
                : explanation;

        var resolvedObservation =
            string.IsNullOrWhiteSpace(observation)
                ? existingTask.InternalObservation
                : observation;

        var resolvedEvidence =
            string.IsNullOrWhiteSpace(evidence)
                ? existingTask.EvidenceRegistered
                : evidence;

        var resolvedReviewStatus =
            string.IsNullOrWhiteSpace(reviewStatus)
                ? existingTask.AdminReviewStatus
                : reviewStatus;

        existingTask.PatchTechnicalData(
            resolvedStatus,
            resolvedDiagnosis,
            resolvedExplanation,
            resolvedObservation,
            resolvedEvidence,
            resolvedReviewStatus
        );

        taskRepository.Update(existingTask);
        await unitOfWork.CompleteAsync();

        return existingTask;
    }

    /// <summary>
    /// Deletes a specific operational task from record tracking systems
    /// using its unique identification key asynchronously.
    /// </summary>
    /// <param name="id">
    /// The unique tracking sequence row index to destroy.
    /// </param>
    /// <returns>
    /// A task that represents the asynchronous execution flow handler.
    /// </returns>
    public async Task DeleteAsync(
        int id
    )
    {
        var existingTask = await taskRepository
            .FindByIdWithPartsAsync(id);

        if (existingTask == null)
        {
            return;
        }

        taskRepository.Remove(existingTask);
        await unitOfWork.CompleteAsync();
    }

    #endregion
}
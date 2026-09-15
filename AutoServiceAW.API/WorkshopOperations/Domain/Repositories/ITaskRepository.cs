using AutoServiceAW.API.Shared.Domain.Repositories;
using Task = AutoServiceAW.API.WorkshopOperations.Domain.Model.Aggregates.Task;

namespace AutoServiceAW.API.WorkshopOperations.Domain.Repositories;

/// <summary>
/// Defines the repository contract for managing <see cref="Task"/> entities.
/// Inherits basic CRUD operations from <see cref="IBaseRepository{Task}"/>.
/// </summary>
public interface ITaskRepository : IBaseRepository<Task>
{
    #region Methods

    /// <summary>
    /// Retrieves all registered tasks including their associated parts.
    /// </summary>
    /// <returns>
    /// A collection containing all tasks with their parts loaded.
    /// </returns>
    Task<IEnumerable<Task>> ListWithPartsAsync();

    /// <summary>
    /// Retrieves a task by its identifier including its associated parts.
    /// </summary>
    /// <param name="id">The task identifier.</param>
    /// <returns>
    /// The matching task with its parts loaded, or null.
    /// </returns>
    Task<Task?> FindByIdWithPartsAsync(int id);

    /// <summary>
    /// Retrieves all tasks linked to a specific work order.
    /// </summary>
    /// <param name="workOrderId">The work order identifier.</param>
    /// <returns>
    /// The matching tasks with their parts loaded.
    /// </returns>
    Task<IEnumerable<Task>> FindByWorkOrderIdAsync(
        int workOrderId
    );

    /// <summary>
    /// Retrieves all tasks assigned to a specific mechanic.
    /// </summary>
    /// <param name="mechanicId">The mechanic identifier.</param>
    /// <returns>
    /// The matching tasks with their parts loaded.
    /// </returns>
    Task<IEnumerable<Task>> FindByMechanicIdAsync(
        int mechanicId
    );

    #endregion
}
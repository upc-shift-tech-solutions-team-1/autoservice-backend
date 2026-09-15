using AutoServiceAW.API.Shared.Infrastructure.Persistence.EFC.Configuration;
using AutoServiceAW.API.Shared.Infrastructure.Persistence.EFC.Repositories;
using AutoServiceAW.API.WorkshopOperations.Domain.Repositories;
using Microsoft.EntityFrameworkCore;
using Task =
    AutoServiceAW.API.WorkshopOperations.Domain.Model.Aggregates.Task;

namespace AutoServiceAW.API.WorkshopOperations.Infrastructure.Persistence.EFC.Repositories;

/// <summary>
/// Infrastructure implementation of the <see cref="ITaskRepository"/> contract
/// using Entity Framework Core and inheriting generic operations from
/// <see cref="BaseRepository{Task}"/>.
/// </summary>
public class TaskRepository(AppDbContext context)
    : BaseRepository<Task>(context), ITaskRepository
{
    #region Methods

    /// <summary>
    /// Retrieves all registered tasks including their associated parts.
    /// </summary>
    /// <returns>
    /// A collection containing all tasks with their parts loaded.
    /// </returns>
    public async Task<IEnumerable<Task>> ListWithPartsAsync()
    {
        return await Context.Tasks
            .Include(task => task.Parts)
            .ToListAsync();
    }

    /// <summary>
    /// Retrieves a task by its identifier including its associated parts.
    /// </summary>
    /// <param name="id">
    /// The unique identifier of the task.
    /// </param>
    /// <returns>
    /// The matching task with its parts loaded,
    /// or null when it does not exist.
    /// </returns>
    public async Task<Task?> FindByIdWithPartsAsync(
        int id
    )
    {
        return await Context.Tasks
            .Include(task => task.Parts)
            .FirstOrDefaultAsync(
                task => task.Id == id
            );
    }

    /// <summary>
    /// Retrieves all tasks linked to a specific work order.
    /// </summary>
    /// <param name="workOrderId">
    /// The parent work order identifier.
    /// </param>
    /// <returns>
    /// The matching tasks with their parts loaded.
    /// </returns>
    public async Task<IEnumerable<Task>> FindByWorkOrderIdAsync(
        int workOrderId
    )
    {
        return await Context.Tasks
            .Include(task => task.Parts)
            .Where(
                task =>
                    task.WorkOrderId == workOrderId
            )
            .ToListAsync();
    }

    /// <summary>
    /// Retrieves all tasks assigned to a specific mechanic.
    /// </summary>
    /// <param name="mechanicId">
    /// The mechanic identifier.
    /// </param>
    /// <returns>
    /// The matching tasks with their parts loaded.
    /// </returns>
    public async Task<IEnumerable<Task>> FindByMechanicIdAsync(
        int mechanicId
    )
    {
        return await Context.Tasks
            .Include(task => task.Parts)
            .Where(
                task =>
                    task.MechanicId == mechanicId
            )
            .ToListAsync();
    }

    #endregion
}
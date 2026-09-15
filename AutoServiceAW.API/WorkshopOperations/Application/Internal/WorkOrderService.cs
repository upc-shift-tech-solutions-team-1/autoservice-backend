using AutoServiceAW.API.Shared.Domain.Repositories;
using AutoServiceAW.API.WorkshopOperations.Domain.Model.Aggregates;
using AutoServiceAW.API.WorkshopOperations.Domain.Repositories;
using AutoServiceAW.API.WorkshopOperations.Domain.Services;

namespace AutoServiceAW.API.WorkshopOperations.Application.Internal;

/// <summary>
/// Provides internal application services for coordinating high-level workshop work orders and vehicle processing pipelines.
/// </summary>
public class WorkOrderService(IWorkOrderRepository workOrderRepository, IUnitOfWork unitOfWork) : IWorkOrderService
{
    #region Methods

    /// <summary>
    /// Creates a new vehicle maintenance work order tracker sequence inside the persistence tier asynchronously.
    /// </summary>
    /// <param name="workOrder">The work order tracker aggregate root framework blueprint model structure.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains the created <see cref="WorkOrder"/> context layer.</returns>
    public async System.Threading.Tasks.Task<WorkOrder?> CreateAsync(WorkOrder workOrder)
    {
        await workOrderRepository.AddAsync(workOrder);
        await unitOfWork.CompleteAsync();
        return workOrder;
    }

    /// <summary>
    /// Retrieves all vehicle service work orders registered globally across tracking records databases asynchronously.
    /// </summary>
    /// <returns>A task that represents the asynchronous operation. The task result contains an enumerable collection of <see cref="WorkOrder"/> entities.</returns>
    public async System.Threading.Tasks.Task<IEnumerable<WorkOrder>> ListAsync() => await workOrderRepository.ListAsync();

    /// <summary>
    /// Retrieves all automotive work orders assigned explicitly to a particular tenant workshop zone asynchronously.
    /// </summary>
    /// <param name="tenantId">The unique custom tenant workshop boundary code index tracking token.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains an enumerable collection of matching <see cref="WorkOrder"/> models.</returns>
    public async System.Threading.Tasks.Task<IEnumerable<WorkOrder>> ListByTenantIdAsync(string tenantId) => await workOrderRepository.FindByWorkshopIdAsync(tenantId);

    /// <summary>
    /// Retrieves a specific service work order matching its baseline tracking numerical primary key identifier asynchronously.
    /// </summary>
    /// <param name="id">The unique integer row tracker identification sequence code index.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains the found <see cref="WorkOrder"/>, or <see langword="null"/>.</returns>
    public async System.Threading.Tasks.Task<WorkOrder?> GetByIdAsync(int id) => await workOrderRepository.FindByIdAsync(id);

    /// <summary>
    /// Synchronizes structural core descriptions, date milestones, billing metrics, checklists, and state progress conditions asynchronously.
    /// </summary>
    /// <param name="id">The unique tracker row lookup key sequence index of the work order to change.</param>
    /// <param name="updatedWorkOrder">The temporary instance domain container holding the updated modification changes properties package.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains the modified and persisted <see cref="WorkOrder"/> aggregate.</returns>
    public async System.Threading.Tasks.Task<WorkOrder?> UpdateAsync(int id, WorkOrder updatedWorkOrder)
    {
        var existingWorkOrder = await workOrderRepository.FindByIdAsync(id);
        if (existingWorkOrder == null) return null;

        existingWorkOrder.Update(updatedWorkOrder.Description, updatedWorkOrder.EstimatedDate, updatedWorkOrder.Price);
        existingWorkOrder.UpdateChecklist(updatedWorkOrder.TasksCompleted, updatedWorkOrder.SparePartsChecked, updatedWorkOrder.DiagnosisValidated, updatedWorkOrder.CleaningDone, updatedWorkOrder.FinalTestDone);
        existingWorkOrder.UpdateStatus(updatedWorkOrder.Status);

        workOrderRepository.Update(existingWorkOrder);
        await unitOfWork.CompleteAsync();
        return existingWorkOrder;
    }

    /// <summary>
    /// Irreversibly deletes a vehicle work order tracking record folder instance from storage context coordinates asynchronously.
    /// </summary>
    /// <param name="id">The system service row structural database identification tracking index parameters to destroy.</param>
    /// <returns>A task that represents the asynchronous operational execution flow pipeline.</returns>
    public async System.Threading.Tasks.Task DeleteAsync(int id)
    {
        var existingWorkOrder = await workOrderRepository.FindByIdAsync(id);
        if (existingWorkOrder != null)
        {
            workOrderRepository.Remove(existingWorkOrder);
            await unitOfWork.CompleteAsync();
        }
    }

    #endregion
}
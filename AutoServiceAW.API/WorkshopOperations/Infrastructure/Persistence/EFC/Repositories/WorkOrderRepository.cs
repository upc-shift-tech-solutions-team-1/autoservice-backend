using AutoServiceAW.API.Shared.Infrastructure.Persistence.EFC.Configuration;
using AutoServiceAW.API.Shared.Infrastructure.Persistence.EFC.Repositories;
using AutoServiceAW.API.WorkshopOperations.Domain.Model.Aggregates;
using AutoServiceAW.API.WorkshopOperations.Domain.Repositories;
using Microsoft.EntityFrameworkCore;

namespace AutoServiceAW.API.WorkshopOperations.Infrastructure.Persistence.EFC.Repositories;

/// <summary>
/// Infrastructure implementation of the <see cref="IWorkOrderRepository"/> contract
/// using Entity Framework Core and inheriting generic operations from <see cref="BaseRepository{WorkOrder}"/>.
/// </summary>
public class WorkOrderRepository(AppDbContext context) : BaseRepository<WorkOrder>(context), IWorkOrderRepository
{
    #region Methods

    /// <summary>
    /// Queries the database mapping context to retrieve all service work orders isolated under an explicit tenant workshop custom code.
    /// </summary>
    /// <param name="workshopId">The multi-tenant isolated target corporate identity tracking token criteria key.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains an enumerable sequence collection matching target records.</returns>
    public async Task<IEnumerable<WorkOrder>> FindByWorkshopIdAsync(string workshopId)
    {
        return await Context.WorkOrders.Where(w => w.WorkshopId == workshopId).ToListAsync();
    }

    #endregion
}
using AutoServiceAW.API.InventoryManagement.Domain.Model.Aggregates;
using AutoServiceAW.API.InventoryManagement.Domain.Repositories;
using AutoServiceAW.API.Shared.Infrastructure.Persistence.EFC.Configuration;
using AutoServiceAW.API.Shared.Infrastructure.Persistence.EFC.Repositories;

namespace AutoServiceAW.API.InventoryManagement.Infrastructure.Persistence.EFC.Repositories;

/// <summary>
/// Infrastructure implementation of the <see cref="IInventoryItemRepository"/> contract 
/// using Entity Framework Core and inheriting generic operations from <see cref="BaseRepository{InventoryItem}"/>.
/// </summary>
public class InventoryItemRepository(AppDbContext context) : BaseRepository<InventoryItem>(context), IInventoryItemRepository
{
}
using AutoServiceAW.API.InventoryManagement.Domain.Model.Aggregates;
using AutoServiceAW.API.Shared.Domain.Repositories;

namespace AutoServiceAW.API.InventoryManagement.Domain.Repositories;

/// <summary>
/// Defines the repository contract for managing <see cref="InventoryItem"/> entities.
/// Inherits basic CRUD operations from <see cref="IBaseRepository{InventoryItem}"/>.
/// </summary>
public interface IInventoryItemRepository : IBaseRepository<InventoryItem>
{
}
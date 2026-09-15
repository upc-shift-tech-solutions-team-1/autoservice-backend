using AutoServiceAW.API.InventoryManagement.Domain.Model.Aggregates;
using AutoServiceAW.API.InventoryManagement.Domain.Repositories;
using AutoServiceAW.API.InventoryManagement.Domain.Services;
using AutoServiceAW.API.Shared.Domain.Repositories;

namespace AutoServiceAW.API.InventoryManagement.Application.Internal;

/// <summary>
/// Provides internal application services for managing inventory item operations.
/// </summary>
public class InventoryItemService(
    IInventoryItemRepository inventoryItemRepository,
    IUnitOfWork unitOfWork
) : IInventoryItemService
{
    #region Methods

    /// <summary>
    /// Creates a new inventory item in the system asynchronously.
    /// </summary>
    /// <param name="item">
    /// The inventory item entity to be created.
    /// </param>
    /// <returns>
    /// The created inventory item.
    /// </returns>
    public async Task<InventoryItem?> CreateAsync(
        InventoryItem item
    )
    {
        await inventoryItemRepository.AddAsync(item);
        await unitOfWork.CompleteAsync();

        return item;
    }

    /// <summary>
    /// Retrieves all inventory items registered in the system asynchronously.
    /// </summary>
    /// <returns>
    /// All registered inventory items.
    /// </returns>
    public async Task<IEnumerable<InventoryItem>> ListAsync()
    {
        return await inventoryItemRepository.ListAsync();
    }

    /// <summary>
    /// Retrieves a specific inventory item by its unique identifier.
    /// </summary>
    /// <param name="id">
    /// The unique identifier of the inventory item.
    /// </param>
    /// <returns>
    /// The matching inventory item, or null.
    /// </returns>
    public async Task<InventoryItem?> GetByIdAsync(
        int id
    )
    {
        return await inventoryItemRepository
            .FindByIdAsync(id);
    }

    /// <summary>
    /// Updates catalog, technical and financial inventory information.
    ///
    /// The stock value received here is controlled by the backend
    /// controller and must not be freely modified by clients.
    /// </summary>
    /// <param name="id">
    /// The inventory item identifier.
    /// </param>
    /// <param name="updatedItem">
    /// The updated inventory information.
    /// </param>
    /// <returns>
    /// The updated inventory item, or null.
    /// </returns>
    public async Task<InventoryItem?> UpdateAsync(
        int id,
        InventoryItem updatedItem
    )
    {
        var existingItem =
            await inventoryItemRepository
                .FindByIdAsync(id);

        if (existingItem == null)
        {
            return null;
        }

        existingItem.Update(
            updatedItem.Name,
            updatedItem.Category,
            updatedItem.Brand,
            updatedItem.UnitPrice,
            updatedItem.Stock,
            updatedItem.MinStock,
            updatedItem.Image,
            updatedItem.PurchasePrice,
            updatedItem.QualityTier,
            updatedItem.Specification,
            updatedItem.Presentation,
            updatedItem.UnitMeasure
        );

        inventoryItemRepository.Update(existingItem);
        await unitOfWork.CompleteAsync();

        return existingItem;
    }

    /// <summary>
    /// Deletes an inventory item from the system.
    /// </summary>
    /// <param name="id">
    /// The inventory item identifier.
    /// </param>
    public async Task DeleteAsync(
        int id
    )
    {
        var existingItem =
            await inventoryItemRepository
                .FindByIdAsync(id);

        if (existingItem == null)
        {
            return;
        }

        inventoryItemRepository.Remove(existingItem);
        await unitOfWork.CompleteAsync();
    }

    /// <summary>
    /// Consumes physical stock when an approved workshop task begins.
    /// </summary>
    /// <param name="inventoryItemId">
    /// The inventory item identifier.
    /// </param>
    /// <param name="quantity">
    /// The quantity consumed.
    /// </param>
    /// <returns>
    /// True when the stock was consumed successfully.
    /// </returns>
    public async Task<bool> ConsumeStockAsync(
        int inventoryItemId,
        int quantity
    )
    {
        var item =
            await inventoryItemRepository
                .FindByIdAsync(inventoryItemId);

        if (item == null)
        {
            return false;
        }

        item.ConsumeStock(quantity);

        inventoryItemRepository.Update(item);
        await unitOfWork.CompleteAsync();

        return true;
    }

    /// <summary>
    /// Increases physical stock through a registered provider receipt.
    /// </summary>
    /// <param name="inventoryItemId">
    /// The inventory item identifier.
    /// </param>
    /// <param name="quantity">
    /// The quantity physically received.
    /// </param>
    /// <returns>
    /// The inventory item with its updated stock, or null.
    /// </returns>
    public async Task<InventoryItem?> ReceiveStockAsync(
        int inventoryItemId,
        int quantity
    )
    {
        var item =
            await inventoryItemRepository
                .FindByIdAsync(inventoryItemId);

        if (item == null)
        {
            return null;
        }

        item.AddStock(quantity);

        inventoryItemRepository.Update(item);
        await unitOfWork.CompleteAsync();

        return item;
    }

    #endregion
}
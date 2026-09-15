using AutoServiceAW.API.InventoryManagement.Domain.Model.Aggregates;

namespace AutoServiceAW.API.InventoryManagement.Domain.Services;

/// <summary>
/// Defines the service contract for managing inventory item domain operations.
/// </summary>
public interface IInventoryItemService
{
    #region Methods

    /// <summary>
    /// Coordinates the creation of a new inventory item.
    /// </summary>
    /// <param name="item">The inventory item entity data to register.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains the created <see cref="InventoryItem"/> or <see langword="null"/> if the operation fails.</returns>
    Task<InventoryItem?> CreateAsync(InventoryItem item);

    /// <summary>
    /// Coordinates the retrieval of all registered inventory items.
    /// </summary>
    /// <returns>A task that represents the asynchronous operation. The task result contains a collection of <see cref="InventoryItem"/> entities.</returns>
    Task<IEnumerable<InventoryItem>> ListAsync();

    /// <summary>
    /// Coordinates the retrieval of a specific inventory item by its unique identifier.
    /// </summary>
    /// <param name="id">The unique identifier of the inventory item.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains the found <see cref="InventoryItem"/>, or <see langword="null"/> if no match is found.</returns>
    Task<InventoryItem?> GetByIdAsync(int id);

    /// <summary>
    /// Coordinates the update of an existing inventory item's data.
    /// </summary>
    /// <param name="id">The unique identifier of the inventory item to update.</param>
    /// <param name="item">The inventory item entity containing the updated information.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains the updated <see cref="InventoryItem"/>, or <see langword="null"/> if the item does not exist.</returns>
    Task<InventoryItem?> UpdateAsync(int id, InventoryItem item);

    /// <summary>
    /// Coordinates the deletion of an inventory item from the system.
    /// </summary>
    /// <param name="id">The unique identifier of the inventory item to delete.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    Task DeleteAsync(int id);
    
    Task<bool> ConsumeStockAsync(int inventoryItemId, int quantity);
/// <summary>
/// Increases physical stock through a registered provider receipt.
/// </summary>
/// <param name="inventoryItemId">
/// The identifier of the received inventory item.
/// </param>
/// <param name="quantity">
/// The quantity physically received from the provider.
/// </param>
/// <returns>
/// The updated inventory item or null when it does not exist.
/// </returns>
Task<InventoryItem?> ReceiveStockAsync(
    int inventoryItemId,
    int quantity
);
    #endregion
}
using AutoServiceAW.API.InventoryManagement.Domain.Model.Aggregates;
using AutoServiceAW.API.InventoryManagement.Domain.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AutoServiceAW.API.InventoryManagement.Interfaces.REST;

/// <summary>
/// Represents the catalog and financial information required
/// to create or update an inventory item.
///
/// Physical stock is intentionally excluded because it can only
/// change through provider receipts or approved task consumption.
/// </summary>
public record CreateInventoryItemResource(
    string Name,
    string Category,
    string Brand,
    decimal UnitPrice,
    int MinStock,
    string? Image,
    decimal PurchasePrice = 0,
    string? QualityTier = "STANDARD",
    string? Specification = "",
    string? Presentation = "",
    string? UnitMeasure = "UNIT"
);

/// <summary>
/// Represents a physical inventory receipt from a provider.
/// </summary>
/// <param name="Quantity">
/// The number of units physically received.
/// </param>
/// <param name="ProviderName">
/// The provider responsible for delivering the materials.
/// </param>
/// <param name="DocumentNumber">
/// Optional invoice, receipt or delivery document reference.
/// </param>
/// <param name="Notes">
/// Optional observations associated with the receipt.
/// </param>
public record ReceiveInventoryStockResource(
    int Quantity,
    string ProviderName,
    string? DocumentNumber,
    string? Notes
);

/// <summary>
/// Exposes REST endpoints for managing the inventory catalog
/// and controlled physical stock receipts.
/// </summary>
[ApiController]
[Route("api/v1/[controller]")]
[Authorize]
public class InventoryItemsController(
    IInventoryItemService inventoryItemService,
    ILogger<InventoryItemsController> logger
) : ControllerBase
{
    /// <summary>
    /// Creates a new inventory catalog item with zero physical stock.
    ///
    /// Stock must later be increased through a provider receipt.
    /// </summary>
    [HttpPost]
    public async Task<IActionResult> CreateInventoryItem(
        [FromBody] CreateInventoryItemResource resource
    )
    {
        try
        {
            const int initialStock = 0;

            var item = BuildInventoryItem(
                resource,
                initialStock
            );

            var result =
                await inventoryItemService.CreateAsync(
                    item
                );

            return StatusCode(
                StatusCodes.Status201Created,
                result
            );
        }
        catch (ArgumentException exception)
        {
            return BadRequest(
                new
                {
                    message = exception.Message
                }
            );
        }
    }

    /// <summary>
    /// Retrieves all inventory catalog items.
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> GetAllInventoryItems()
    {
        var items =
            await inventoryItemService.ListAsync();

        return Ok(items);
    }

    /// <summary>
    /// Retrieves an inventory item by its identifier.
    /// </summary>
    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetInventoryItemById(
        int id
    )
    {
        var item =
            await inventoryItemService.GetByIdAsync(id);

        return item == null
            ? NotFound()
            : Ok(item);
    }

    /// <summary>
    /// Updates catalog, technical and financial information
    /// while preserving the current physical stock.
    /// </summary>
    [HttpPut("{id:int}")]
    public async Task<IActionResult> UpdateInventoryItem(
        int id,
        [FromBody] CreateInventoryItemResource resource
    )
    {
        var existingItem =
            await inventoryItemService.GetByIdAsync(id);

        if (existingItem == null)
        {
            return NotFound();
        }

        try
        {
            var item = BuildInventoryItem(
                resource,
                existingItem.Stock
            );

            var result =
                await inventoryItemService.UpdateAsync(
                    id,
                    item
                );

            return result == null
                ? NotFound()
                : Ok(result);
        }
        catch (ArgumentException exception)
        {
            return BadRequest(
                new
                {
                    message = exception.Message
                }
            );
        }
    }

    /// <summary>
    /// Registers the physical receipt of materials from a provider
    /// and increases stock by the received quantity.
    /// </summary>
    /// <param name="id">
    /// Inventory item identifier.
    /// </param>
    /// <param name="resource">
    /// Provider and receipt information.
    /// </param>
    /// <returns>
    /// The receipt result and updated inventory item.
    /// </returns>
    [HttpPost("{id:int}/receipts")]
    public async Task<IActionResult> ReceiveInventoryStock(
        int id,
        [FromBody] ReceiveInventoryStockResource resource
    )
    {
        if (resource.Quantity <= 0)
        {
            return BadRequest(
                new
                {
                    message =
                        "La cantidad recibida debe ser mayor que cero."
                }
            );
        }

        if (string.IsNullOrWhiteSpace(resource.ProviderName))
        {
            return BadRequest(
                new
                {
                    message =
                        "Debe indicar el nombre del proveedor."
                }
            );
        }

        var existingItem =
            await inventoryItemService.GetByIdAsync(id);

        if (existingItem == null)
        {
            return NotFound(
                new
                {
                    message =
                        $"No existe el artículo de inventario con id {id}."
                }
            );
        }

        var previousStock =
            existingItem.Stock;

        try
        {
            var updatedItem =
                await inventoryItemService.ReceiveStockAsync(
                    id,
                    resource.Quantity
                );

            if (updatedItem == null)
            {
                return NotFound();
            }

            var receivedAt =
                DateTime.UtcNow;

            logger.LogInformation(
                "Provider receipt registered. InventoryItemId: {InventoryItemId}, Provider: {ProviderName}, Document: {DocumentNumber}, Quantity: {Quantity}, PreviousStock: {PreviousStock}, CurrentStock: {CurrentStock}, ReceivedAt: {ReceivedAt}",
                id,
                resource.ProviderName.Trim(),
                resource.DocumentNumber,
                resource.Quantity,
                previousStock,
                updatedItem.Stock,
                receivedAt
            );

            return Ok(
                new
                {
                    message =
                        "Recepción de proveedor registrada correctamente.",

                    providerName =
                        resource.ProviderName.Trim(),

                    documentNumber =
                        resource.DocumentNumber?.Trim(),

                    notes =
                        resource.Notes?.Trim(),

                    quantityReceived =
                        resource.Quantity,

                    previousStock,

                    currentStock =
                        updatedItem.Stock,

                    receivedAt,

                    item =
                        updatedItem
                }
            );
        }
        catch (InvalidOperationException exception)
        {
            return BadRequest(
                new
                {
                    message = exception.Message
                }
            );
        }
    }

    /// <summary>
    /// Deletes an inventory catalog item.
    /// </summary>
    [HttpDelete("{id:int}")]
    public async Task<IActionResult> DeleteInventoryItem(
        int id
    )
    {
        await inventoryItemService.DeleteAsync(id);

        return NoContent();
    }

    /// <summary>
    /// Builds an inventory aggregate using a controlled stock value.
    /// </summary>
    private static InventoryItem BuildInventoryItem(
        CreateInventoryItemResource resource,
        int controlledStock
    )
    {
        return new InventoryItem(
            resource.Name,
            resource.Category,
            resource.Brand,
            resource.UnitPrice,
            controlledStock,
            resource.MinStock,
            resource.Image ?? string.Empty,
            resource.PurchasePrice,
            resource.QualityTier ?? "STANDARD",
            resource.Specification ?? string.Empty,
            resource.Presentation ?? string.Empty,
            resource.UnitMeasure ?? "UNIT"
        );
    }
}
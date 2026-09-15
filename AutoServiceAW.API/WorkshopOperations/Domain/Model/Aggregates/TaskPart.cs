using System.Text.Json.Serialization;
namespace AutoServiceAW.API.WorkshopOperations.Domain.Model.Aggregates;

/// <summary>
/// Represents a TaskPart entity representing a specific spare part resource allocated to a diagnostic task.
/// </summary>
public class TaskPart
{
    /// <summary>
    /// Gets the unique structural identifier for the task part row asset entry.
    /// </summary>
    public int Id { get; private set; }
    /// <summary>
    /// Gets the unique identifier of the target domain <see cref="Task"/> this piece resource is allocated to.
    /// </summary>

    public int TaskId { get; private set; }

    [JsonIgnore]
    public Task Task { get; private set; }
    /// <summary>
    /// Gets the unique identifier matching the target global warehouse inventory asset registry item tracking key.
    /// </summary>
    public int InventoryItemId { get; private set; }
    public string Name { get; private set; }
    /// <summary>
    /// Gets the historical snapshot commercial descriptive name of the spare piece asset item.
    /// </summary>
    public string Brand { get; private set; }
    /// <summary>
    /// Gets the structural total quantity units requested and consumed for the active task operation.
    /// </summary>
    public string QualityTier { get; private set; }
    /// <summary>
    /// Gets the static unit price record at the transaction allocation time window event block.
    /// </summary>

    public int Quantity { get; private set; }
    public decimal PurchasePrice { get; private set; }
    public decimal UnitPrice { get; private set; }

    public decimal TotalCost => PurchasePrice * Quantity;
    public decimal TotalSale => UnitPrice * Quantity;
    public decimal GrossProfit => TotalSale - TotalCost;
    /// <summary>
    /// Initializes a new instance of the <see cref="TaskPart"/> class tracking warehouse piece asset unit allocations.
    /// </summary>
    /// <param name="taskId">The parent execution task target row locator key link.</param>
    /// <param name="inventoryItemId">The reference identity mapping index code of the warehouse repository item.</param>
    /// <param name="name">The snapshot item label descriptor text value.</param>
    /// <param name="quantity">The total quantity units consumed count value.</param>
    /// <param name="unitPrice">The commercial standard asset fee valuation unit scale.</param>
    public TaskPart(
        int taskId,
        int inventoryItemId,
        string name,
        int quantity,
        decimal unitPrice,
        decimal purchasePrice = 0,
        string brand = "",
        string qualityTier = "STANDARD")
    {
        if (quantity <= 0)
            throw new ArgumentException("Quantity must be greater than zero");

        TaskId = taskId;
        InventoryItemId = inventoryItemId;
        Name = name;
        Quantity = quantity;
        UnitPrice = unitPrice;
        PurchasePrice = purchasePrice;
        Brand = brand ?? string.Empty;
        QualityTier = string.IsNullOrWhiteSpace(qualityTier) ? "STANDARD" : qualityTier;
    }
    /// <summary>
    /// Initializes a new instance of the <see cref="TaskPart"/> class with default values.
    /// Required by ORMs such as Entity Framework Core for data mapping.
    /// </summary>


    protected TaskPart()
    {
        Task = null!;
        Name = string.Empty;
        Brand = string.Empty;
        QualityTier = "STANDARD";
    }
}
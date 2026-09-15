namespace AutoServiceAW.API.InventoryManagement.Domain.Model.Aggregates;

/// <summary>
/// Represents an Inventory Item aggregate root within the Inventory Management domain.
/// </summary>
public class InventoryItem
{
    public int Id { get; private set; }
    public string Sku { get; private set; }
    public string Name { get; private set; }
    public string Category { get; private set; }
    public string Brand { get; private set; }
    public string QualityTier { get; private set; }
    public string Specification { get; private set; }
    public string Presentation { get; private set; }
    public string UnitMeasure { get; private set; }
    public decimal PurchasePrice { get; private set; }
    public decimal UnitPrice { get; private set; }
    public int Stock { get; private set; }
    public int MinStock { get; private set; }
    public string Image { get; private set; }

    public decimal SalePrice => UnitPrice;
    public decimal ProfitPerUnit => UnitPrice - PurchasePrice;
    public decimal MarginPercentage => UnitPrice == 0
        ? 0
        : Math.Round((ProfitPerUnit / UnitPrice) * 100, 2);
    public decimal InventoryCostValue => PurchasePrice * Stock;
    public decimal PotentialSalesValue => UnitPrice * Stock;
    public decimal PotentialProfitValue => ProfitPerUnit * Stock;

    public InventoryItem(
        string name,
        string category,
        string brand,
        decimal unitPrice,
        int stock,
        int minStock,
        string image,
        decimal purchasePrice = 0,
        string qualityTier = "STANDARD",
        string specification = "",
        string presentation = "",
        string unitMeasure = "UNIT")
    {
        Validate(name, unitPrice, purchasePrice, stock, minStock);

        Sku = $"SKU-{Guid.NewGuid().ToString("N")[..8].ToUpper()}";
        Name = name.Trim();
        Category = string.IsNullOrWhiteSpace(category) ? "SPARE_PART" : category.Trim().ToUpperInvariant();
        Brand = string.IsNullOrWhiteSpace(brand) ? "GENERIC" : brand.Trim();
        QualityTier = NormalizeQualityTier(qualityTier);
        Specification = specification?.Trim() ?? string.Empty;
        Presentation = presentation?.Trim() ?? string.Empty;
        UnitMeasure = string.IsNullOrWhiteSpace(unitMeasure) ? "UNIT" : unitMeasure.Trim().ToUpperInvariant();
        UnitPrice = unitPrice;
        PurchasePrice = ResolvePurchasePrice(purchasePrice, unitPrice);
        Stock = stock;
        MinStock = minStock;
        Image = image ?? string.Empty;
    }

    protected InventoryItem()
    {
        Sku = string.Empty;
        Name = string.Empty;
        Category = string.Empty;
        Brand = string.Empty;
        QualityTier = "STANDARD";
        Specification = string.Empty;
        Presentation = string.Empty;
        UnitMeasure = "UNIT";
        Image = string.Empty;
    }

    public void Update(
        string name,
        string category,
        string brand,
        decimal unitPrice,
        int stock,
        int minStock,
        string image,
        decimal purchasePrice = 0,
        string qualityTier = "STANDARD",
        string specification = "",
        string presentation = "",
        string unitMeasure = "UNIT")
    {
        Validate(name, unitPrice, purchasePrice, stock, minStock);

        Name = name.Trim();
        Category = string.IsNullOrWhiteSpace(category) ? "SPARE_PART" : category.Trim().ToUpperInvariant();
        Brand = string.IsNullOrWhiteSpace(brand) ? "GENERIC" : brand.Trim();
        QualityTier = NormalizeQualityTier(qualityTier);
        Specification = specification?.Trim() ?? string.Empty;
        Presentation = presentation?.Trim() ?? string.Empty;
        UnitMeasure = string.IsNullOrWhiteSpace(unitMeasure) ? "UNIT" : unitMeasure.Trim().ToUpperInvariant();
        UnitPrice = unitPrice;
        PurchasePrice = ResolvePurchasePrice(purchasePrice, unitPrice);
        Stock = stock;
        MinStock = minStock;
        Image = image ?? string.Empty;
    }

    public void DecreaseStock(int quantity)
    {
        if (quantity <= 0)
            throw new ArgumentException("Quantity must be greater than zero");

        if (Stock < quantity)
            throw new InvalidOperationException("Insufficient stock");

        Stock -= quantity;
    }

    public void AddStock(int quantity)
    {
        if (quantity <= 0)
            throw new InvalidOperationException("Quantity must be greater than zero");

        Stock += quantity;
    }

    public void ConsumeStock(int quantity)
    {
        if (quantity <= 0)
            return;

        DecreaseStock(quantity);
    }

    private static decimal ResolvePurchasePrice(decimal purchasePrice, decimal unitPrice)
    {
        if (purchasePrice > 0)
            return purchasePrice;

        return unitPrice > 0 ? Math.Round(unitPrice * 0.70m, 2) : 0;
    }

    private static string NormalizeQualityTier(string qualityTier)
    {
        var normalized = string.IsNullOrWhiteSpace(qualityTier)
            ? "STANDARD"
            : qualityTier.Trim().ToUpperInvariant();

        return normalized is "ECONOMY" or "STANDARD" or "PREMIUM"
            ? normalized
            : "STANDARD";
    }

    private static void Validate(string name, decimal unitPrice, decimal purchasePrice, int stock, int minStock)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Name is required");
        if (unitPrice < 0 || purchasePrice < 0)
            throw new ArgumentException("Prices cannot be negative");
        if (stock < 0 || minStock < 0)
            throw new ArgumentException("Stock values cannot be negative");
    }
}
using AutoServiceAW.API.WorkshopOperations.Domain.Model.Aggregates;

namespace AutoServiceAW.API.Tests.WorkshopOperations.Domain.Model.Aggregates;

[TestClass]
public class TaskPartTests
{
    /// <summary>
    /// Verifies that the part retains its inventory details and that costs,
    /// sale price, and profit are calculated from quantity and prices.
    /// </summary>
    [TestMethod]
    public void Constructor_WithValidQuantity_ShouldInitializeSnapshotAndCalculateTotals()
    {
        // Arrange

        // Act
        var taskPart = new TaskPart(5, 7, "Brake pad", 2, 80m, 50m, "Acme", "PREMIUM");

        // Assert
        Assert.AreEqual(5, taskPart.TaskId);
        Assert.AreEqual(7, taskPart.InventoryItemId);
        Assert.AreEqual("Brake pad", taskPart.Name);
        Assert.AreEqual(2, taskPart.Quantity);
        Assert.AreEqual(80m, taskPart.UnitPrice);
        Assert.AreEqual(50m, taskPart.PurchasePrice);
        Assert.AreEqual("Acme", taskPart.Brand);
        Assert.AreEqual("PREMIUM", taskPart.QualityTier);
        Assert.AreEqual(100m, taskPart.TotalCost);
        Assert.AreEqual(160m, taskPart.TotalSale);
        Assert.AreEqual(60m, taskPart.GrossProfit);
    }

    /// <summary>
    /// Verifies the default values for optional fields and totals
    /// when no purchase price is provided.
    /// </summary>
    [TestMethod]
    public void Constructor_WithDefaultOptionalValues_ShouldUseStandardDefaults()
    {
        // Arrange

        // Act
        var taskPart = new TaskPart(5, 7, "Brake pad", 1, 80m);

        // Assert
        Assert.AreEqual(0m, taskPart.PurchasePrice);
        Assert.AreEqual(string.Empty, taskPart.Brand);
        Assert.AreEqual("STANDARD", taskPart.QualityTier);
        Assert.AreEqual(0m, taskPart.TotalCost);
        Assert.AreEqual(80m, taskPart.TotalSale);
        Assert.AreEqual(80m, taskPart.GrossProfit);
    }

    /// <summary>
    /// Verifies that the constructor rejects zero or negative quantities,
    /// since they do not represent a valid part allocation.
    /// </summary>
    [TestMethod]
    [DataRow(0)]
    [DataRow(-1)]
    public void Constructor_WithNonPositiveQuantity_ShouldThrowArgumentException(int quantity)
    {
        // Arrange
        Action createTaskPart = () => new TaskPart(5, 7, "Brake pad", quantity, 80m);

        // Act and Assert
        Assert.Throws<ArgumentException>(
            createTaskPart
        );
    }

    /// <summary>
    /// Verifies that a null brand is normalized to an empty string and
    /// a blank quality tier falls back to the standard value.
    /// </summary>
    [TestMethod]
    public void Constructor_WithNullBrandAndBlankQualityTier_ShouldNormalizeOptionalText()
    {
        // Arrange

        // Act
        var taskPart = new TaskPart(5, 7, "Brake pad", 1, 80m, brand: null!, qualityTier: " ");

        // Assert
        Assert.AreEqual(string.Empty, taskPart.Brand);
        Assert.AreEqual("STANDARD", taskPart.QualityTier);
    }
}

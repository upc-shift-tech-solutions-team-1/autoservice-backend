using AutoServiceAW.API.WorkshopOperations.Domain.Model.Aggregates;

namespace AutoServiceAW.API.Tests.WorkshopOperations.Domain.Model.Aggregates;

[TestClass]
public class TaskPartTests
{
    
    [TestMethod]
    public void Constructor_WithValidQuantity_ShouldInitializeSnapshotAndCalculateTotals()
    {
        var taskPart = new TaskPart(5, 7, "Brake pad", 2, 80m, 50m, "Acme", "PREMIUM");

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

    [TestMethod]
    public void Constructor_WithDefaultOptionalValues_ShouldUseStandardDefaults()
    {
        var taskPart = new TaskPart(5, 7, "Brake pad", 1, 80m);

        Assert.AreEqual(0m, taskPart.PurchasePrice);
        Assert.AreEqual(string.Empty, taskPart.Brand);
        Assert.AreEqual("STANDARD", taskPart.QualityTier);
        Assert.AreEqual(0m, taskPart.TotalCost);
        Assert.AreEqual(80m, taskPart.TotalSale);
        Assert.AreEqual(80m, taskPart.GrossProfit);
    }

    [TestMethod]
    [DataRow(0)]
    [DataRow(-1)]
    public void Constructor_WithNonPositiveQuantity_ShouldThrowArgumentException(int quantity)
    {
        Assert.Throws<ArgumentException>(
            () => new TaskPart(5, 7, "Brake pad", quantity, 80m)
        );
    }

    [TestMethod]
    public void Constructor_WithNullBrandAndBlankQualityTier_ShouldNormalizeOptionalText()
    {
        var taskPart = new TaskPart(5, 7, "Brake pad", 1, 80m, brand: null!, qualityTier: " ");

        Assert.AreEqual(string.Empty, taskPart.Brand);
        Assert.AreEqual("STANDARD", taskPart.QualityTier);
    }
}

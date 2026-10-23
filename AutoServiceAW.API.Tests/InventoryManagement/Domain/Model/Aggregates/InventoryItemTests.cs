using AutoServiceAW.API.InventoryManagement.Domain.Model.Aggregates;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace AutoServiceAW.API.Tests.InventoryManagement.Domain.Model.Aggregates;

[TestClass]
public class InventoryItemTests
{
    [TestMethod]
    public void Constructor_ShouldCalculateDefaultPurchasePrice()
    {
        // Arrange & Act
        var item = CreateItem();

        // Assert
        Assert.AreEqual(70m, item.PurchasePrice);
    }

    [TestMethod]
    public void DecreaseStock_ShouldReduceAvailableStock()
    {
        // Arrange
        var item = CreateItem();

        // Act
        item.DecreaseStock(3);

        // Assert
        Assert.AreEqual(7, item.Stock);
    }

    [TestMethod]
    public void DecreaseStock_ShouldThrowExceptionWhenStockIsInsufficient()
    {
        // Arrange
        var item = CreateItem(stock: 2, minimumStock: 1);

        // Act & Assert
        Assert.ThrowsExactly<InvalidOperationException>(
            () => item.DecreaseStock(5));
    }

    [TestMethod]
    public void AddStock_ShouldIncreaseAvailableStock()
    {
        // Arrange
        var item = CreateItem();

        // Act
        item.AddStock(5);

        // Assert
        Assert.AreEqual(15, item.Stock);
    }

    private static InventoryItem CreateItem(
        int stock = 10,
        int minimumStock = 2)
    {
        return new InventoryItem(
            "Brake Pad",
            "Brakes",
            "Bosch",
            100m,
            stock,
            minimumStock,
            "brake-pad.jpg");
    }
}
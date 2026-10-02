using AutoServiceAW.API.InventoryManagement.Domain.Model.Aggregates;

namespace AutoServiceAW.Tests.Unit;

public class InventoryItemTests
{
    [Fact]
    public void Constructor_ShouldCalculateDefaultPurchasePrice()
    {
        // Arrange & Act
        var item = new InventoryItem(
            "Brake Pad",
            "Brakes",
            "Bosch",
            100m,
            10,
            2,
            "brake-pad.jpg");

        // Assert
        Assert.Equal(70m, item.PurchasePrice);
    }

    [Fact]
    public void DecreaseStock_ShouldReduceAvailableStock()
    {
        // Arrange
        var item = new InventoryItem(
            "Brake Pad",
            "Brakes",
            "Bosch",
            100m,
            10,
            2,
            "brake-pad.jpg");

        // Act
        item.DecreaseStock(3);

        // Assert
        Assert.Equal(7, item.Stock);
    }

    [Fact]
    public void DecreaseStock_ShouldThrowExceptionWhenStockIsInsufficient()
    {
        // Arrange
        var item = new InventoryItem(
            "Brake Pad",
            "Brakes",
            "Bosch",
            100m,
            2,
            1,
            "brake-pad.jpg");

        // Act & Assert
        Assert.Throws<InvalidOperationException>(
            () => item.DecreaseStock(5));
    }

    [Fact]
    public void AddStock_ShouldIncreaseAvailableStock()
    {
        // Arrange
        var item = new InventoryItem(
            "Brake Pad",
            "Brakes",
            "Bosch",
            100m,
            10,
            2,
            "brake-pad.jpg");

        // Act
        item.AddStock(5);

        // Assert
        Assert.Equal(15, item.Stock);
    }
}
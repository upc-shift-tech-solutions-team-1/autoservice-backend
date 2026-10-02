using WorkshopTask =
    AutoServiceAW.API.WorkshopOperations.Domain.Model.Aggregates.Task;

namespace AutoServiceAW.Tests.Unit;

public class TaskTests
{
    [Fact]
    public void Constructor_ShouldCalculateDefaultLaborCost()
    {
        // Arrange & Act
        var task = new WorkshopTask(
            1,
            10,
            "Oil change",
            "PENDING",
            "HIGH",
            60,
            200m);

        // Assert
        Assert.Equal(100m, task.LaborCost);
        Assert.Equal(200m, task.LaborPrice);
    }

    [Fact]
    public void UpdateMaterialsCost_ShouldUpdateMaterialCosts()
    {
        // Arrange
        var task = new WorkshopTask(
            1,
            10,
            "Oil change",
            "PENDING",
            "HIGH",
            60,
            200m);

        // Act
        task.UpdateMaterialsCost(150m, 100m);

        // Assert
        Assert.Equal(150m, task.MaterialsCost);
        Assert.Equal(100m, task.MaterialsPurchaseCost);
    }

    [Fact]
    public void GrossProfit_ShouldCalculateDifferenceBetweenRevenueAndInternalCost()
    {
        // Arrange
        var task = new WorkshopTask(
            1,
            10,
            "Brake replacement",
            "PENDING",
            "HIGH",
            120,
            300m,
            150m);

        task.UpdateMaterialsCost(200m, 100m);

        // Act
        var grossProfit = task.GrossProfit;

        // Assert
        Assert.Equal(250m, grossProfit);
    }
}
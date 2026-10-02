using AutoServiceAW.API.StaffCoordination.Domain.Model.Aggregates;
using AutoServiceAW.API.TenantManagement.Domain.Model.Aggregates;
using AutoServiceAW.API.WorkshopOperations.Domain.Model.Aggregates;

namespace AutoServiceAW.Tests.Aggregates;

public class RemainingAggregatesTests
{
    // =========================================================
    // TaskPart
    // =========================================================

    [Fact]
    public void Constructor_WithValidData_ShouldCreateTaskPart()
    {
        // Arrange
        var taskId = 10;
        var inventoryItemId = 20;
        var name = "Brake Pad";
        var quantity = 2;
        var unitPrice = 150m;
        var purchasePrice = 100m;

        // Act
        var taskPart = new TaskPart(
            taskId,
            inventoryItemId,
            name,
            quantity,
            unitPrice,
            purchasePrice,
            "Brembo",
            "PREMIUM");

        // Assert
        Assert.Equal(taskId, taskPart.TaskId);
        Assert.Equal(inventoryItemId, taskPart.InventoryItemId);
        Assert.Equal(name, taskPart.Name);
        Assert.Equal(quantity, taskPart.Quantity);
        Assert.Equal(unitPrice, taskPart.UnitPrice);
        Assert.Equal(purchasePrice, taskPart.PurchasePrice);
        Assert.Equal("Brembo", taskPart.Brand);
        Assert.Equal("PREMIUM", taskPart.QualityTier);
    }

    [Fact]
    public void Constructor_WithInvalidQuantity_ShouldThrowException()
    {
        // Arrange
        var quantity = 0;

        // Act
        var exception = Assert.Throws<ArgumentException>(() =>
            new TaskPart(
                1,
                2,
                "Brake Pad",
                quantity,
                150m));

        // Assert
        Assert.Equal(
            "Quantity must be greater than zero",
            exception.Message);
    }

    [Fact]
    public void TaskPart_ShouldCalculateCostsAndProfitCorrectly()
    {
        // Arrange
        var taskPart = new TaskPart(
            1,
            2,
            "Brake Pad",
            3,
            150m,
            100m);

        // Act
        var totalCost = taskPart.TotalCost;
        var totalSale = taskPart.TotalSale;
        var grossProfit = taskPart.GrossProfit;

        // Assert
        Assert.Equal(300m, totalCost);
        Assert.Equal(450m, totalSale);
        Assert.Equal(150m, grossProfit);
    }

    [Fact]
    public void TaskPart_WithEmptyQualityTier_ShouldUseStandard()
    {
        // Arrange & Act
        var taskPart = new TaskPart(
            1,
            2,
            "Brake Pad",
            1,
            100m,
            qualityTier: "   ");

        // Assert
        Assert.Equal("STANDARD", taskPart.QualityTier);
    }

    // =========================================================
    // Mechanic
    // =========================================================

    [Fact]
    public void Mechanic_Constructor_ShouldSetProperties()
    {
        // Arrange & Act
        var mechanic = new Mechanic(
            "Juan Perez",
            "Brakes",
            5,
            "juan@example.com",
            "WS-1234",
            "password123");

        // Assert
        Assert.Equal("Juan Perez", mechanic.FullName);
        Assert.Equal("Brakes", mechanic.Specialty);
        Assert.Equal(5, mechanic.MaxCapacity);
        Assert.Equal("juan@example.com", mechanic.Email);
        Assert.Equal("WS-1234", mechanic.WorkshopId);
        Assert.Equal("password123", mechanic.Password);
    }

    [Fact]
    public void Mechanic_Update_ShouldChangeProfileData()
    {
        // Arrange
        var mechanic = new Mechanic(
            "Juan Perez",
            "Brakes",
            5,
            "juan@example.com",
            "WS-1234",
            "oldPassword");

        // Act
        mechanic.Update(
            "Carlos Gomez",
            "Engine",
            8,
            "carlos@example.com",
            "newPassword");

        // Assert
        Assert.Equal("Carlos Gomez", mechanic.FullName);
        Assert.Equal("Engine", mechanic.Specialty);
        Assert.Equal(8, mechanic.MaxCapacity);
        Assert.Equal("carlos@example.com", mechanic.Email);
        Assert.Equal("newPassword", mechanic.Password);
    }

    // =========================================================
    // Workshop
    // =========================================================

    [Fact]
    public void Workshop_Constructor_ShouldGenerateTenantId()
    {
        // Arrange
        var name = "AutoService Central";

        // Act
        var workshop = new Workshop(name);

        // Assert
        Assert.Equal(name, workshop.Name);
        Assert.NotNull(workshop.TenantId);
        Assert.Matches(
            "^WS-[A-Z0-9]{4}$",
            workshop.TenantId);
    }

    // =========================================================
    // WorkOrder
    // =========================================================

    [Fact]
    public void WorkOrder_Constructor_ShouldSetInitialValues()
    {
        // Arrange
        var workshopId = "WS-1234";
        var vehicleId = 10;
        var customerId = 20;
        var mechanicId = 30;
        var description = "Engine diagnosis";
        var estimatedDate = "2026-10-10";
        var price = 500m;

        // Act
        var workOrder = new WorkOrder(
            workshopId,
            vehicleId,
            customerId,
            mechanicId,
            description,
            estimatedDate,
            price);

        // Assert
        Assert.Equal(workshopId, workOrder.WorkshopId);
        Assert.Equal(vehicleId, workOrder.VehicleId);
        Assert.Equal(customerId, workOrder.CustomerId);
        Assert.Equal(mechanicId, workOrder.MechanicId);
        Assert.Equal(description, workOrder.Description);
        Assert.Equal(estimatedDate, workOrder.EstimatedDate);
        Assert.Equal(price, workOrder.Price);
        Assert.Equal("PENDING", workOrder.Status);
        Assert.False(workOrder.TasksCompleted);
        Assert.False(workOrder.SparePartsChecked);
        Assert.False(workOrder.DiagnosisValidated);
        Assert.False(workOrder.CleaningDone);
        Assert.False(workOrder.FinalTestDone);
    }

    [Fact]
    public void WorkOrder_Constructor_ShouldGenerateTrackingCode()
    {
        // Arrange & Act
        var workOrder = new WorkOrder(
            "WS-1234",
            1,
            2,
            3,
            "Diagnosis",
            "2026-10-10",
            300m);

        // Assert
        Assert.Matches(
            "^WO-[A-F0-9]{6}$",
            workOrder.TrackingCode);
    }

    [Fact]
    public void WorkOrder_Update_ShouldChangeDescriptionDateAndPrice()
    {
        // Arrange
        var workOrder = new WorkOrder(
            "WS-1234",
            1,
            2,
            3,
            "Initial diagnosis",
            "2026-10-10",
            300m);

        // Act
        workOrder.Update(
            "Complete engine repair",
            "2026-10-15",
            750m);

        // Assert
        Assert.Equal("Complete engine repair", workOrder.Description);
        Assert.Equal("2026-10-15", workOrder.EstimatedDate);
        Assert.Equal(750m, workOrder.Price);
    }

    [Fact]
    public void WorkOrder_UpdateChecklist_ShouldUpdateAllChecklistFlags()
    {
        // Arrange
        var workOrder = new WorkOrder(
            "WS-1234",
            1,
            2,
            3,
            "Diagnosis",
            "2026-10-10",
            300m);

        // Act
        workOrder.UpdateChecklist(
            true,
            true,
            true,
            false,
            true);

        // Assert
        Assert.True(workOrder.TasksCompleted);
        Assert.True(workOrder.SparePartsChecked);
        Assert.True(workOrder.DiagnosisValidated);
        Assert.False(workOrder.CleaningDone);
        Assert.True(workOrder.FinalTestDone);
    }

    [Fact]
    public void WorkOrder_UpdateStatus_WithValidStatus_ShouldUpdateStatus()
    {
        // Arrange
        var workOrder = new WorkOrder(
            "WS-1234",
            1,
            2,
            3,
            "Diagnosis",
            "2026-10-10",
            300m);

        // Act
        workOrder.UpdateStatus("IN_PROGRESS");

        // Assert
        Assert.Equal("IN_PROGRESS", workOrder.Status);
    }

    [Fact]
    public void WorkOrder_UpdateStatus_WithEmptyStatus_ShouldKeepPreviousStatus()
    {
        // Arrange
        var workOrder = new WorkOrder(
            "WS-1234",
            1,
            2,
            3,
            "Diagnosis",
            "2026-10-10",
            300m);

        // Act
        workOrder.UpdateStatus(string.Empty);

        // Assert
        Assert.Equal("PENDING", workOrder.Status);
    }
}
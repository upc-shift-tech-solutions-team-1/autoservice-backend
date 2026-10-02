using System.Text.RegularExpressions;
using AutoServiceAW.API.WorkshopOperations.Domain.Model.Aggregates;

namespace AutoServiceAW.API.Tests.WorkshopOperations.Domain.Model.Aggregates;

[TestClass]
public class WorkOrderTests
{
    /// <summary>
    /// Verifies that the work order initializes its data, generates a valid
    /// tracking code, and starts with today's date and an incomplete checklist.
    /// </summary>
    [TestMethod]
    public void Constructor_ShouldInitializeWorkOrderAndGenerateTrackingInformation()
    {
        // Arrange
        var beforeConstruction = DateTime.UtcNow.Date;

        // Act
        var workOrder = new WorkOrder("workshop-1", 10, 20, 30, "Brake service", "2026-10-10", 500m);

        // Assert
        Assert.AreEqual("workshop-1", workOrder.WorkshopId);
        Assert.AreEqual(10, workOrder.VehicleId);
        Assert.AreEqual(20, workOrder.CustomerId);
        Assert.AreEqual(30, workOrder.MechanicId);
        Assert.AreEqual("Brake service", workOrder.Description);
        Assert.AreEqual("PENDING", workOrder.Status);
        Assert.AreEqual(500m, workOrder.Price);
        Assert.AreEqual("2026-10-10", workOrder.EstimatedDate);
        Assert.IsTrue(Regex.IsMatch(workOrder.TrackingCode, "^WO-[A-F0-9]{6}$"));
        Assert.IsTrue(DateTime.TryParse(workOrder.StartDate, out var startDate));
        Assert.AreEqual(beforeConstruction, startDate.Date);
        Assert.IsFalse(workOrder.TasksCompleted);
        Assert.IsFalse(workOrder.SparePartsChecked);
        Assert.IsFalse(workOrder.DiagnosisValidated);
        Assert.IsFalse(workOrder.CleaningDone);
        Assert.IsFalse(workOrder.FinalTestDone);
    }

    /// <summary>
    /// Verifies that Update changes the description, estimated date, and price
    /// without changing the tracking code or start date.
    /// </summary>
    [TestMethod]
    public void Update_ShouldChangeDescriptionEstimateAndPrice()
    {
        // Arrange
        var workOrder = new WorkOrder("workshop-1", 10, 20, 30, "Initial", "2026-10-10", 500m);
        var trackingCode = workOrder.TrackingCode;
        var startDate = workOrder.StartDate;

        // Act
        workOrder.Update("Updated service", "2026-10-15", 750m);

        // Assert
        Assert.AreEqual("Updated service", workOrder.Description);
        Assert.AreEqual("2026-10-15", workOrder.EstimatedDate);
        Assert.AreEqual(750m, workOrder.Price);
        Assert.AreEqual(trackingCode, workOrder.TrackingCode);
        Assert.AreEqual(startDate, workOrder.StartDate);
    }

    /// <summary>
    /// Verifies that UpdateChecklist updates each work order checklist flag.
    /// </summary>
    [TestMethod]
    public void UpdateChecklist_ShouldReplaceAllChecklistFlags()
    {
        // Arrange
        var workOrder = new WorkOrder("workshop-1", 10, 20, 30, "Brake service", "2026-10-10", 500m);

        // Act
        workOrder.UpdateChecklist(true, false, true, false, true);

        // Assert
        Assert.IsTrue(workOrder.TasksCompleted);
        Assert.IsFalse(workOrder.SparePartsChecked);
        Assert.IsTrue(workOrder.DiagnosisValidated);
        Assert.IsFalse(workOrder.CleaningDone);
        Assert.IsTrue(workOrder.FinalTestDone);
    }

    /// <summary>
    /// Verifies that null and empty statuses are ignored and do not replace
    /// the work order's last valid status.
    /// </summary>
    [TestMethod]
    public void UpdateStatus_WithNullOrEmptyStatus_ShouldPreserveCurrentStatus()
    {
        // Arrange
        var workOrder = new WorkOrder("workshop-1", 10, 20, 30, "Brake service", "2026-10-10", 500m);

        // Act
        workOrder.UpdateStatus("IN_PROGRESS");
        workOrder.UpdateStatus(string.Empty);
        workOrder.UpdateStatus(null!);

        // Assert
        Assert.AreEqual("IN_PROGRESS", workOrder.Status);
    }

    /// <summary>
    /// Verifies that a non-empty status replaces the work order's current status.
    /// </summary>
    [TestMethod]
    public void UpdateStatus_WithNonEmptyStatus_ShouldReplaceCurrentStatus()
    {
        // Arrange
        var workOrder = new WorkOrder("workshop-1", 10, 20, 30, "Brake service", "2026-10-10", 500m);

        // Act
        workOrder.UpdateStatus("COMPLETED");

        // Assert
        Assert.AreEqual("COMPLETED", workOrder.Status);
    }
}

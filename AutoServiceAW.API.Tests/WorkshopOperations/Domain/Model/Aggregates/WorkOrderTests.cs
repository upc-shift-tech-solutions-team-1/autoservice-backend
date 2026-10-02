using System.Text.RegularExpressions;
using AutoServiceAW.API.WorkshopOperations.Domain.Model.Aggregates;

namespace AutoServiceAW.API.Tests.WorkshopOperations.Domain.Model.Aggregates;

[TestClass]
public class WorkOrderTests
{
    [TestMethod]
    public void Constructor_ShouldInitializeWorkOrderAndGenerateTrackingInformation()
    {
        var beforeConstruction = DateTime.UtcNow.Date;

        var workOrder = new WorkOrder("workshop-1", 10, 20, 30, "Brake service", "2026-10-10", 500m);

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

    [TestMethod]
    public void Update_ShouldChangeDescriptionEstimateAndPrice()
    {
        var workOrder = new WorkOrder("workshop-1", 10, 20, 30, "Initial", "2026-10-10", 500m);
        var trackingCode = workOrder.TrackingCode;
        var startDate = workOrder.StartDate;

        workOrder.Update("Updated service", "2026-10-15", 750m);

        Assert.AreEqual("Updated service", workOrder.Description);
        Assert.AreEqual("2026-10-15", workOrder.EstimatedDate);
        Assert.AreEqual(750m, workOrder.Price);
        Assert.AreEqual(trackingCode, workOrder.TrackingCode);
        Assert.AreEqual(startDate, workOrder.StartDate);
    }

    [TestMethod]
    public void UpdateChecklist_ShouldReplaceAllChecklistFlags()
    {
        var workOrder = new WorkOrder("workshop-1", 10, 20, 30, "Brake service", "2026-10-10", 500m);

        workOrder.UpdateChecklist(true, false, true, false, true);

        Assert.IsTrue(workOrder.TasksCompleted);
        Assert.IsFalse(workOrder.SparePartsChecked);
        Assert.IsTrue(workOrder.DiagnosisValidated);
        Assert.IsFalse(workOrder.CleaningDone);
        Assert.IsTrue(workOrder.FinalTestDone);
    }

    [TestMethod]
    public void UpdateStatus_WithNullOrEmptyStatus_ShouldPreserveCurrentStatus()
    {
        var workOrder = new WorkOrder("workshop-1", 10, 20, 30, "Brake service", "2026-10-10", 500m);

        workOrder.UpdateStatus("IN_PROGRESS");
        workOrder.UpdateStatus(string.Empty);
        workOrder.UpdateStatus(null!);

        Assert.AreEqual("IN_PROGRESS", workOrder.Status);
    }

    [TestMethod]
    public void UpdateStatus_WithNonEmptyStatus_ShouldReplaceCurrentStatus()
    {
        var workOrder = new WorkOrder("workshop-1", 10, 20, 30, "Brake service", "2026-10-10", 500m);

        workOrder.UpdateStatus("COMPLETED");

        Assert.AreEqual("COMPLETED", workOrder.Status);
    }
}

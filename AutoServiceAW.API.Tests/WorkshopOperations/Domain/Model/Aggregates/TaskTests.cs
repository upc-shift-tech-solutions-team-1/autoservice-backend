using WorkshopTask =
    AutoServiceAW.API.WorkshopOperations.Domain.Model.Aggregates.Task;
using AutoServiceAW.API.WorkshopOperations.Domain.Model.Aggregates;

namespace AutoServiceAW.API.Tests.WorkshopOperations.Domain.Model.Aggregates;

[TestClass]
public class TaskTests
{
    /// <summary>
    /// Verifies that a task is initialized correctly and that labor cost defaults
    /// to 50% of the labor price when no explicit cost is provided.
    /// </summary>
    [TestMethod]
    public void Constructor_WithLaborPriceAndNoLaborCost_ShouldInitializePropertiesAndCalculateLaborCost()
    {
        // Arrange
        // Omit labor cost to verify its automatic calculation.

        // Act
        var task = new WorkshopTask(
            12,
            34,
            "Replace brake pads",
            "PENDING",
            "HIGH",
            90,
            250m
        );

        // Assert
        Assert.AreEqual(12, task.WorkOrderId);
        Assert.AreEqual(34, task.MechanicId);
        Assert.AreEqual("Replace brake pads", task.Description);
        Assert.AreEqual("PENDING", task.Status);
        Assert.AreEqual("HIGH", task.Priority);
        Assert.AreEqual(90, task.EstimatedTime);
        Assert.AreEqual(250m, task.LaborPrice);
        Assert.AreEqual(125m, task.LaborCost);
        Assert.AreEqual(250m, task.TotalCost);
        Assert.AreEqual(125m, task.TotalInternalCost);
        Assert.AreEqual(125m, task.GrossProfit);
        Assert.AreEqual(50m, task.MarginPercentage);
        Assert.IsEmpty(task.Parts);
    }

    /// <summary>
    /// Verifies that an explicitly provided labor cost is used and that
    /// a task can be created without an assigned mechanic.
    /// </summary>
    [TestMethod]
    public void Constructor_WithExplicitLaborCost_ShouldUseProvidedCost()
    {
        // Arrange

        // Act
        var task = new WorkshopTask(12, null, "Inspection", "PENDING", "LOW", 30, 100m, 65m);

        // Assert
        Assert.IsNull(task.MechanicId);
        Assert.AreEqual(65m, task.LaborCost);
    }

    /// <summary>
    /// Verifies that Update replaces the task details and recalculates
    /// labor cost when no explicit cost is provided.
    /// </summary>
    [TestMethod]
    public void Update_ShouldReplaceTaskDetailsAndResolveLaborCost()
    {
        // Arrange
        var task = new WorkshopTask(12, 34, "Old description", "PENDING", "LOW", 30, 100m, 80m);

        // Act
        task.Update("New description", "IN_PROGRESS", "HIGH", 60, 200m, null);

        // Assert
        Assert.AreEqual("New description", task.Description);
        Assert.AreEqual("IN_PROGRESS", task.Status);
        Assert.AreEqual("HIGH", task.Priority);
        Assert.AreEqual(60, task.EstimatedTime);
        Assert.AreEqual(200m, task.LaborPrice);
        Assert.AreEqual(100m, task.LaborCost);
        Assert.IsNull(task.MechanicId);
    }

    /// <summary>
    /// Verifies that an empty status preserves the current status while
    /// the technical fields are updated with the supplied information.
    /// </summary>
    [TestMethod]
    public void PatchTechnicalData_WithEmptyStatus_ShouldPreserveStatusAndReplaceTechnicalData()
    {
        // Arrange
        var task = new WorkshopTask(12, null, "Inspection", "IN_PROGRESS", "LOW", 30, 100m);

        // Act
        task.PatchTechnicalData(
            string.Empty,
            "Engine noise diagnosed",
            "The engine requires inspection",
            "Do not start engine",
            "evidence-123",
            "PENDING"
        );

        // Assert
        Assert.AreEqual("IN_PROGRESS", task.Status);
        Assert.AreEqual("Engine noise diagnosed", task.TechnicalDiagnosis);
        Assert.AreEqual("The engine requires inspection", task.CustomerExplanation);
        Assert.AreEqual("Do not start engine", task.InternalObservation);
        Assert.AreEqual("evidence-123", task.EvidenceRegistered);
        Assert.AreEqual("PENDING", task.AdminReviewStatus);
    }

    /// <summary>
    /// Verifies that material costs and an added part are included in the task
    /// totals, gross profit, and margin.
    /// </summary>
    [TestMethod]
    public void UpdateMaterialsCostAndAddPart_ShouldUpdateTotalsAndParts()
    {
        // Arrange
        var task = new WorkshopTask(12, null, "Brake service", "PENDING", "HIGH", 90, 250m, 125m);
        var part = new TaskPart(0, 7, "Brake pad", 2, 80m, 50m);

        // Act
        task.UpdateMaterialsCost(160m, 100m);
        task.AddPart(part);

        // Assert
        Assert.AreSame(part, task.Parts.Single());
        Assert.AreEqual(410m, task.TotalCost);
        Assert.AreEqual(225m, task.TotalInternalCost);
        Assert.AreEqual(185m, task.GrossProfit);
        Assert.AreEqual(45.12m, task.MarginPercentage);
    }
}

using AutoServiceAW.API.PublicTracking.Application.Internal;
using AutoServiceAW.API.WorkshopOperations.Domain.Model.Aggregates;
using WorkshopTask = AutoServiceAW.API.WorkshopOperations.Domain.Model.Aggregates.Task;

namespace AutoServiceAW.API.Tests.PublicTracking.Application.Internal;

[TestClass]
public class TrackingSummaryFactoryTests
{
    [TestMethod]
    [DataRow(0, 0, 0)]
    [DataRow(0, 3, 0)]
    [DataRow(1, 4, 25)]
    [DataRow(3, 4, 75)]
    [DataRow(4, 4, 100)]
    public void CalculateProgress_ShouldUseCompletedTasksAndHandleEmptyOrders(
        int completedCount,
        int totalCount,
        int expectedProgress
    )
    {
        // ARRANGE
        var tasks = Enumerable.Range(0, totalCount)
            .Select(index => CreateTask(
                index < completedCount ? "COMPLETED" : "PENDING"
            ))
            .ToList();

        // ACT
        var progress = TrackingSummaryFactory.CalculateProgress(tasks);

        // ASSERT
        Assert.AreEqual((decimal)expectedProgress, progress);
    }

    [TestMethod]
    public void Create_ShouldCalculateCustomerCostsWithoutInternalCosts()
    {
        // ARRANGE
        var workOrder = CreateWorkOrder(price: 999m);
        var firstTask = CreateTask("COMPLETED", laborPrice: 100m);
        firstTask.UpdateMaterialsCost(materialsCost: 25m, materialsPurchaseCost: 9m);
        var secondTask = CreateTask("PENDING", laborPrice: 80m);
        secondTask.UpdateMaterialsCost(materialsCost: 50m, materialsPurchaseCost: 16m);

        // ACT
        var summary = TrackingSummaryFactory.Create(
            workOrder,
            new[] { firstTask, secondTask }
        );

        // ASSERT
        Assert.AreEqual(180m, summary.Costs.LaborSubtotal);
        Assert.AreEqual(75m, summary.Costs.MaterialsSubtotal);
        Assert.AreEqual(255m, summary.Costs.Total);
        Assert.AreEqual(999m, workOrder.Price);
    }

    [TestMethod]
    public void UpdateStatus_ShouldRecordOnlyActualStatusChangesWithUtcTimestamps()
    {
        // ARRANGE
        var workOrder = CreateWorkOrder();
        var beforeChange = DateTime.UtcNow;

        // ACT
        workOrder.UpdateStatus("IN_PROGRESS");
        workOrder.UpdateStatus("IN_PROGRESS");

        // ASSERT
        Assert.HasCount(2, workOrder.StatusHistory);
        var recordedChange = workOrder.StatusHistory.Last();
        Assert.AreEqual("IN_PROGRESS", recordedChange.Status);
        Assert.AreEqual(DateTimeKind.Utc, recordedChange.ChangedAtUtc.Kind);
        Assert.IsTrue(recordedChange.ChangedAtUtc >= beforeChange);
    }

    [TestMethod]
    public void Create_ShouldExposeCustomerTaskDetailsAndOmitInternalFields()
    {
        // ARRANGE
        var task = CreateTask("COMPLETED", laborPrice: 120m);
        task.UpdateMaterialsCost(materialsCost: 180m, materialsPurchaseCost: 95m);
        task.PatchTechnicalData(
            "COMPLETED",
            "Batería desgastada",
            "Se reemplazó la batería",
            "Nota interna del taller",
            "https://example.test/evidence",
            "APPROVED"
        );
        task.AddPart(new TaskPart(
            taskId: 12,
            inventoryItemId: 91,
            name: "Batería 12V",
            quantity: 1,
            unitPrice: 180m,
            purchasePrice: 95m
        ));

        // ACT
        var summary = TrackingSummaryFactory.Create(CreateWorkOrder(), new[] { task });
        var publicTask = summary.Tasks.Single();

        // ASSERT
        Assert.AreEqual("Batería desgastada", publicTask.TechnicalDiagnosis);
        Assert.AreEqual("Se reemplazó la batería", publicTask.CustomerExplanation);
        Assert.AreEqual("https://example.test/evidence", publicTask.EvidenceRegistered);
        Assert.AreEqual("Batería 12V", publicTask.Parts.Single().Name);
        Assert.AreEqual(1, publicTask.Parts.Single().Quantity);
        Assert.AreEqual(180m, publicTask.Parts.Single().UnitPrice);
        Assert.IsNull(publicTask.Parts.Single().GetType().GetProperty("PurchasePrice"));
        Assert.IsNull(publicTask.Parts.Single().GetType().GetProperty("InventoryItemId"));
        Assert.IsNull(publicTask.GetType().GetProperty("InternalObservation"));
        Assert.IsNull(publicTask.GetType().GetProperty("LaborCost"));
        Assert.IsNull(publicTask.GetType().GetProperty("MaterialsPurchaseCost"));
        Assert.IsNull(publicTask.GetType().GetProperty("AdminReviewStatus"));
    }

    private static WorkOrder CreateWorkOrder(decimal price = 150m) => new(
        workshopId: "workshop-test",
        vehicleId: 1,
        customerId: 1,
        mechanicId: 1,
        description: "Servicio de prueba",
        estimatedDate: "2026-10-15",
        price: price
    );

    private static WorkshopTask CreateTask(string status, decimal laborPrice = 0m) => new(
        workOrderId: 1,
        mechanicId: 2,
        description: "Revisión de batería",
        status: status,
        priority: "NORMAL",
        estimatedTime: 30,
        laborPrice: laborPrice
    );
}

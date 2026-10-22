using System.Net;
using System.Text.Json;
using AutoServiceAW.API.CustomerManagement.Domain.Services;
using AutoServiceAW.API.FleetManagement.Domain.Services;
using AutoServiceAW.API.PublicTracking.Interfaces.REST;
using AutoServiceAW.API.TenantManagement.Domain.Repositories;
using AutoServiceAW.API.WorkshopOperations.Domain.Model.Aggregates;
using AutoServiceAW.API.WorkshopOperations.Domain.Services;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Task = System.Threading.Tasks.Task;

namespace AutoServiceAW.API.Tests.PublicTracking.Interfaces.REST;

[TestClass]
public class TrackingIntegrationTests
{
    private Mock<IWorkOrderService> _workOrderServiceMock = null!;
    private Mock<ITaskService> _taskServiceMock = null!;
    private WorkOrder _requestedOrder = null!;
    private WebApplication _app = null!;
    private HttpClient _client = null!;

    [TestInitialize]
    public async Task SetUp()
    {
        // ARRANGE
        _requestedOrder = CreateWorkOrder("Requested service");
        _workOrderServiceMock = new Mock<IWorkOrderService>();
        _taskServiceMock = new Mock<ITaskService>();

        _workOrderServiceMock
            .Setup(service => service.GetByTrackingCodeAsync(_requestedOrder.TrackingCode))
            .ReturnsAsync(_requestedOrder);
        _workOrderServiceMock
            .Setup(service => service.GetByTrackingCodeAsync("UNKNOWN"))
            .ReturnsAsync((WorkOrder?)null);

        _taskServiceMock
            .Setup(service => service.ListByWorkOrderIdAsync(_requestedOrder.Id))
            .ReturnsAsync(CreateRequestedTasks(_requestedOrder.Id));

        var builder = WebApplication.CreateBuilder();
        builder.WebHost.UseTestServer();
        builder.Services
            .AddControllers()
            .AddApplicationPart(typeof(TrackingController).Assembly);
        builder.Services.AddSingleton(_workOrderServiceMock.Object);
        builder.Services.AddSingleton(Mock.Of<IVehicleService>());
        builder.Services.AddSingleton(_taskServiceMock.Object);
        builder.Services.AddSingleton(Mock.Of<ICustomerService>());
        builder.Services.AddSingleton(Mock.Of<IWorkshopRepository>());

        _app = builder.Build();
        _app.MapControllers();
        await _app.StartAsync();
        _client = _app.GetTestClient();
    }

    [TestCleanup]
    public async Task TearDown()
    {
        _client.Dispose();
        await _app.DisposeAsync();
    }

    [TestMethod]
    public async Task GetSummaryByTrackingCodeShouldReturnSafeOrderDataAndRejectIdOnlyTaskAccess()
    {
        // ACT
        var orderResponse = await _client.GetAsync(
            $"/api/v1/tracking/workorders?trackingCode={_requestedOrder.TrackingCode}"
        );
        var response = await _client.GetAsync(
            $"/api/v1/tracking/summary?trackingCode={_requestedOrder.TrackingCode}"
        );
        var unknownOrderResponse = await _client.GetAsync(
            "/api/v1/tracking/workorders?trackingCode=UNKNOWN"
        );
        var unknownCodeResponse = await _client.GetAsync(
            "/api/v1/tracking/summary?trackingCode=UNKNOWN"
        );
        var idOnlyTasksResponse = await _client.GetAsync(
            $"/api/v1/tracking/tasks?workOrderId={_requestedOrder.Id}"
        );

        // ASSERT
        Assert.AreEqual(HttpStatusCode.OK, orderResponse.StatusCode);
        Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
        Assert.AreEqual(HttpStatusCode.NotFound, unknownOrderResponse.StatusCode);
        Assert.AreEqual(HttpStatusCode.NotFound, unknownCodeResponse.StatusCode);
        Assert.AreEqual(HttpStatusCode.NotFound, idOnlyTasksResponse.StatusCode);

        var orderContent = await orderResponse.Content.ReadAsStringAsync();
        using var orderDocument = JsonDocument.Parse(orderContent);
        var publicOrderList = orderDocument.RootElement;
        Assert.AreEqual(JsonValueKind.Array, publicOrderList.ValueKind);
        Assert.AreEqual(1, publicOrderList.GetArrayLength());

        var publicOrder = publicOrderList[0];
        Assert.AreEqual(_requestedOrder.TrackingCode, publicOrder.GetProperty("trackingCode").GetString());
        Assert.AreEqual(_requestedOrder.Status, publicOrder.GetProperty("status").GetString());
        Assert.AreEqual(_requestedOrder.EstimatedDate, publicOrder.GetProperty("estimatedDate").GetString());
        Assert.AreEqual(_requestedOrder.VehicleId, publicOrder.GetProperty("vehicleId").GetInt32());
        Assert.AreEqual(_requestedOrder.CustomerId, publicOrder.GetProperty("customerId").GetInt32());
        Assert.AreEqual(_requestedOrder.WorkshopId, publicOrder.GetProperty("workshopId").GetString());
        Assert.AreEqual(6, publicOrder.EnumerateObject().Count());
        Assert.IsFalse(publicOrder.TryGetProperty("id", out _));
        Assert.IsFalse(publicOrder.TryGetProperty("price", out _));
        Assert.IsFalse(publicOrder.TryGetProperty("mechanicId", out _));
        Assert.IsFalse(publicOrder.TryGetProperty("tasksCompleted", out _));

        var responseContent = await response.Content.ReadAsStringAsync();
        using var responseDocument = JsonDocument.Parse(responseContent);
        var summary = responseDocument.RootElement;

        Assert.AreEqual(JsonValueKind.Object, summary.ValueKind);
        Assert.AreEqual(
            _requestedOrder.TrackingCode,
            summary.GetProperty("trackingCode").GetString()
        );
        Assert.AreEqual(50m, summary.GetProperty("progressPercentage").GetDecimal());
        Assert.AreEqual(120m, summary.GetProperty("costs").GetProperty("total").GetDecimal());
        Assert.AreEqual(2, summary.GetProperty("tasks").GetArrayLength());
        Assert.AreEqual(1, summary.GetProperty("history").GetArrayLength());

        var publicTask = summary.GetProperty("tasks")[0];
        Assert.AreEqual("Diagnóstico de prueba", publicTask.GetProperty("technicalDiagnosis").GetString());
        var publicPart = publicTask.GetProperty("parts")[0];
        Assert.AreEqual("Filtro de aceite", publicPart.GetProperty("name").GetString());
        Assert.AreEqual(1, publicPart.GetProperty("quantity").GetInt32());
        Assert.AreEqual(20m, publicPart.GetProperty("unitPrice").GetDecimal());
        Assert.IsFalse(publicPart.TryGetProperty("purchasePrice", out _));
        Assert.IsFalse(publicPart.TryGetProperty("inventoryItemId", out _));
        Assert.IsFalse(publicTask.TryGetProperty("internalObservation", out _));
        Assert.IsFalse(publicTask.TryGetProperty("laborCost", out _));
        Assert.IsFalse(publicTask.TryGetProperty("materialsPurchaseCost", out _));
        Assert.IsFalse(summary.TryGetProperty("workOrderId", out _));

        _workOrderServiceMock.Verify(
            service => service.GetByTrackingCodeAsync(_requestedOrder.TrackingCode),
            Times.Exactly(2)
        );
        _workOrderServiceMock.Verify(service => service.ListAsync(), Times.Never);
        _taskServiceMock.Verify(
            service => service.ListByWorkOrderIdAsync(_requestedOrder.Id),
            Times.Once
        );
    }

    private static WorkOrder CreateWorkOrder(string description) => new(
        workshopId: "workshop-test",
        vehicleId: 1,
        customerId: 1,
        mechanicId: 1,
        description: description,
        estimatedDate: "2026-10-15",
        price: 150m
    );

    private static AutoServiceAW.API.WorkshopOperations.Domain.Model.Aggregates.Task CreateTask(
        int workOrderId,
        string status,
        decimal laborPrice = 100m,
        decimal materialsCost = 20m
    )
    {
        var task = new AutoServiceAW.API.WorkshopOperations.Domain.Model.Aggregates.Task(
            workOrderId,
            mechanicId: 2,
            description: "Diagnóstico de prueba",
            status: status,
            priority: "NORMAL",
            estimatedTime: 30,
            laborPrice: laborPrice,
            laborCost: 40m
        );
        task.UpdateMaterialsCost(materialsCost: materialsCost, materialsPurchaseCost: 8m);
        task.PatchTechnicalData(
            status,
            "Diagnóstico de prueba",
            "Explicación para cliente",
            "Observación privada",
            "https://example.test/evidence",
            "APPROVED"
        );
        return task;
    }

    private static IReadOnlyList<AutoServiceAW.API.WorkshopOperations.Domain.Model.Aggregates.Task>
        CreateRequestedTasks(int workOrderId)
    {
        var completedTask = CreateTask(workOrderId, "COMPLETED");
        completedTask.AddPart(new TaskPart(
            taskId: 15,
            inventoryItemId: 44,
            name: "Filtro de aceite",
            quantity: 1,
            unitPrice: 20m,
            purchasePrice: 18m
        ));

        return new[]
        {
            completedTask,
            CreateTask(workOrderId, "PENDING", laborPrice: 0m, materialsCost: 0m)
        };
    }
}

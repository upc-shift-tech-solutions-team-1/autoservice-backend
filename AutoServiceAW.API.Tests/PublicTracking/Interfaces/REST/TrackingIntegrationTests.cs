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
    private WorkOrder _requestedOrder = null!;
    private WorkOrder _unrelatedOrder = null!;
    private WebApplication _app = null!;
    private HttpClient _client = null!;

    [TestInitialize]
    public async Task SetUp()
    {
        // ARRANGE
        _requestedOrder = CreateWorkOrder("Requested service");
        _unrelatedOrder = CreateWorkOrder("Unrelated service");
        _workOrderServiceMock = new Mock<IWorkOrderService>();

        _workOrderServiceMock
            .Setup(service => service.ListAsync())
            .ReturnsAsync(new[] { _unrelatedOrder, _requestedOrder });

        var builder = WebApplication.CreateBuilder();
        builder.WebHost.UseTestServer();
        builder.Services
            .AddControllers()
            .AddApplicationPart(typeof(TrackingController).Assembly);
        builder.Services.AddSingleton(_workOrderServiceMock.Object);
        builder.Services.AddSingleton(Mock.Of<IVehicleService>());
        builder.Services.AddSingleton(Mock.Of<ITaskService>());
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
    public async Task GetOrderByCodeWithValidCodeShouldReturnOnlyAssociatedOrder()
    {
        // ACT
        var response = await _client.GetAsync(
            $"/api/v1/tracking/workorders?trackingCode={_requestedOrder.TrackingCode}"
        );

        // ASSERT
        Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);

        var responseContent = await response.Content.ReadAsStringAsync();
        using var responseDocument = JsonDocument.Parse(responseContent);
        var orders = responseDocument.RootElement;

        Assert.AreEqual(JsonValueKind.Array, orders.ValueKind);
        Assert.AreEqual(1, orders.GetArrayLength());
        Assert.AreEqual(
            _requestedOrder.TrackingCode,
            orders[0].GetProperty("trackingCode").GetString()
        );
        Assert.AreNotEqual(
            _unrelatedOrder.TrackingCode,
            orders[0].GetProperty("trackingCode").GetString()
        );

        _workOrderServiceMock.Verify(service => service.ListAsync(), Times.Once);
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
}

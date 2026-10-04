using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using AutoServiceAW.API.WorkshopOperations.Domain.Model.Aggregates;
using AutoServiceAW.API.WorkshopOperations.Domain.Services;
using AutoServiceAW.API.WorkshopOperations.Interfaces.REST;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Moq;

namespace AutoServiceAW.API.Tests.WorkshopOperations.Interfaces.REST;

[TestClass]
public class WorkOrdersIntegrationTests
{
    [TestMethod]
    public async System.Threading.Tasks.Task CreateThenListWorkOrder_ShouldKeepAuthenticatedWorkshopBoundary()
    {
        // Arrange
        WorkOrder? storedOrder = null;
        var service = new Mock<IWorkOrderService>();

        service
            .Setup(value => value.CreateAsync(It.IsAny<WorkOrder>()))
            .Callback<WorkOrder>(order => storedOrder = order)
            .ReturnsAsync((WorkOrder order) => order);

        service
            .Setup(value => value.ListByTenantIdAsync("WS-01"))
            .ReturnsAsync(() =>
                storedOrder == null
                    ? Array.Empty<WorkOrder>()
                    : new[] { storedOrder }
            );

        var builder = WebApplication.CreateBuilder();
        builder.WebHost.UseTestServer();

        builder.Services
            .AddAuthentication("Test")
            .AddScheme<
                AuthenticationSchemeOptions,
                TestAuthenticationHandler
            >("Test", _ => { });
        builder.Services.AddAuthorization();
        builder.Services
            .AddControllers()
            .AddApplicationPart(typeof(WorkOrdersController).Assembly);
        builder.Services.AddSingleton(service.Object);

        await using var app = builder.Build();
        app.UseAuthentication();
        app.UseAuthorization();
        app.MapControllers();
        await app.StartAsync();

        var client = app.GetTestClient();

        // Act: create the work order through the real HTTP controller.
        var createResponse = await client.PostAsJsonAsync(
            "/api/v1/workorders",
            new CreateWorkOrderResource(
                15,
                23,
                8,
                "Brake noise reported by the customer",
                "2026-10-05"
            )
        );

        var listResponse = await client.GetAsync(
            "/api/v1/workorders"
        );

        // Assert
        Assert.AreEqual(
            HttpStatusCode.Created,
            createResponse.StatusCode
        );
        Assert.AreEqual(HttpStatusCode.OK, listResponse.StatusCode);
        Assert.IsNotNull(storedOrder);
        Assert.AreEqual("WS-01", storedOrder.WorkshopId);

        var json = await listResponse.Content
            .ReadFromJsonAsync<JsonElement>();

        Assert.AreEqual(JsonValueKind.Array, json.ValueKind);
        Assert.AreEqual(1, json.GetArrayLength());
        Assert.AreEqual(
            "Brake noise reported by the customer",
            json[0].GetProperty("description").GetString()
        );

        service.Verify(
            value => value.ListByTenantIdAsync("WS-01"),
            Times.Once
        );
    }
}

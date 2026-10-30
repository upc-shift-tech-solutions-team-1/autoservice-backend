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

        // Defines the authenticated workshop for this integration test.
        app.Use(async (context, next) =>
        {
            context.User = new System.Security.Claims.ClaimsPrincipal(
                new System.Security.Claims.ClaimsIdentity(
                    new[]
                    {
                        new System.Security.Claims.Claim(
                            "WorkshopId",
                            "WS-01"
                        )
                    },
                    "Test"
                )
            );

            await next();
        });

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

        Assert.AreEqual(
            HttpStatusCode.OK,
            listResponse.StatusCode
        );

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

    [TestMethod]
    public async System.Threading.Tasks.Task
        GetWorkOrder_FromAnotherWorkshop_ShouldReturnNotFoundOverHttp()
    {
        // Arrange
        var foreignOrder = new WorkOrder(
            "WS-02",
            15,
            23,
            8,
            "Brake repair from another workshop",
            "2026-10-25",
            500m
        );

        var service = new Mock<IWorkOrderService>();

        service
            .Setup(value => value.GetByIdAsync(41))
            .ReturnsAsync(foreignOrder);

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

        // Act
        var response = await client.GetAsync(
            "/api/v1/workorders/41"
        );

        // Assert
        Assert.AreEqual(
            HttpStatusCode.NotFound,
            response.StatusCode
        );

        service.Verify(
            value => value.GetByIdAsync(41),
            Times.Once
        );
    }

    [TestMethod]
    public async System.Threading.Tasks.Task
        UpdateWorkOrder_FromAnotherWorkshop_ShouldReturnNotFoundOverHttp()
    {
        // Arrange
        var foreignOrder = new WorkOrder(
            "WS-02",
            15,
            23,
            8,
            "Original brake diagnosis",
            "2026-10-25",
            500m
        );

        var service = new Mock<IWorkOrderService>();

        service
            .Setup(value => value.GetByIdAsync(41))
            .ReturnsAsync(foreignOrder);

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

        var request = new UpdateWorkOrderResource(
            "Unauthorized brake modification",
            "2026-11-05",
            900m,
            "COMPLETED",
            true,
            true,
            true,
            true,
            true
        );

        // Act
        var response = await client.PutAsJsonAsync(
            "/api/v1/workorders/41",
            request
        );

        // Assert
        Assert.AreEqual(
            HttpStatusCode.NotFound,
            response.StatusCode
        );

        service.Verify(
            value => value.GetByIdAsync(41),
            Times.Once
        );

        service.Verify(
            value => value.UpdateAsync(
                It.IsAny<int>(),
                It.IsAny<string>(),
                It.IsAny<WorkOrder>()),
            Times.Never
        );

        Assert.AreEqual(
            "Original brake diagnosis",
            foreignOrder.Description
        );

        Assert.AreEqual(500m, foreignOrder.Price);
    }


    [TestMethod]
    public async System.Threading.Tasks.Task
        DeleteWorkOrder_FromAnotherWorkshop_ShouldReturnNotFoundOverHttp()
    {
        // Arrange
        var foreignOrder = new WorkOrder(
            "WS-02",
            15,
            23,
            8,
            "Brake repair from another workshop",
            "2026-10-25",
            500m
        );

        var service = new Mock<IWorkOrderService>();

        service
            .Setup(value => value.GetByIdAsync(41))
            .ReturnsAsync(foreignOrder);

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

        // Act
        var response = await client.DeleteAsync(
            "/api/v1/workorders/41"
        );

        // Assert
        Assert.AreEqual(
            HttpStatusCode.NotFound,
            response.StatusCode
        );

        service.Verify(
            value => value.GetByIdAsync(41),
            Times.Once
        );

        service.Verify(
            value => value.DeleteAsync(
                It.IsAny<int>(),
                It.IsAny<string>()),
            Times.Never
        );
    }

    [TestMethod]
    public async System.Threading.Tasks.Task
        GetWorkOrder_FromOwnWorkshop_ShouldReturnOkOverHttp()
    {
        // Arrange
        var ownOrder = new WorkOrder(
            "WS-01",
            15,
            23,
            8,
            "Brake repair for own workshop",
            "2026-10-25",
            500m
        );

        var service = new Mock<IWorkOrderService>();

        service
            .Setup(value => value.GetByIdAsync(41))
            .ReturnsAsync(ownOrder);

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

        // Act
        var response = await client.GetAsync(
            "/api/v1/workorders/41"
        );

        // Assert
        Assert.AreEqual(
            HttpStatusCode.OK,
            response.StatusCode
        );

        var json = await response.Content
            .ReadFromJsonAsync<JsonElement>();

        Assert.AreEqual(
            "WS-01",
            json.GetProperty("workshopId").GetString()
        );

        Assert.AreEqual(
            "Brake repair for own workshop",
            json.GetProperty("description").GetString()
        );

        service.Verify(
            value => value.GetByIdAsync(41),
            Times.Once
        );
    }

[TestMethod]
public async System.Threading.Tasks.Task
    UpdateWorkOrder_FromOwnWorkshop_ShouldReturnOkOverHttp()
{
    // Arrange
    var existingOrder = new WorkOrder(
        "WS-01",
        15,
        23,
        8,
        "Original brake diagnosis",
        "2026-10-25",
        500m
    );

    var service = new Mock<IWorkOrderService>();

    service
        .Setup(value => value.GetByIdAsync(41))
        .ReturnsAsync(existingOrder);

    service
        .Setup(value => value.UpdateAsync(
            41,
            "WS-01",
            It.IsAny<WorkOrder>(),
            It.IsAny<string?>()))
        .ReturnsAsync((int _, string _, WorkOrder updated, string? _) => updated);

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

    var request = new UpdateWorkOrderResource(
        "Brake repair completed",
        "2026-11-05",
        750m,
        "COMPLETED",
        true,
        true,
        true,
        true,
        true,
        "El proveedor retrasó la entrega del material"
    );

    // Act
    var response = await client.PutAsJsonAsync(
        "/api/v1/workorders/41",
        request
    );

    // Assert
    Assert.AreEqual(
        HttpStatusCode.OK,
        response.StatusCode
    );

    service.Verify(
        value => value.UpdateAsync(
            41,
            "WS-01",
            It.Is<WorkOrder>(order =>
                order.WorkshopId == "WS-01" &&
                order.Description == "Brake repair completed" &&
                order.Price == 750m),
            "El proveedor retrasó la entrega del material"),
        Times.Once
    );
}

[TestMethod]
public async System.Threading.Tasks.Task
    DeleteWorkOrder_FromOwnWorkshop_ShouldReturnNoContentOverHttp()
{
    // Arrange
    var ownOrder = new WorkOrder(
        "WS-01",
        15,
        23,
        8,
        "Brake repair",
        "2026-10-25",
        500m
    );

    var service = new Mock<IWorkOrderService>();

    service
        .Setup(value => value.GetByIdAsync(41))
        .ReturnsAsync(ownOrder);

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

    // Act
    var response = await client.DeleteAsync(
        "/api/v1/workorders/41"
    );

    // Assert
    Assert.AreEqual(
        HttpStatusCode.NoContent,
        response.StatusCode
    );

    service.Verify(
        value => value.GetByIdAsync(41),
        Times.Once
    );

    service.Verify(
        value => value.DeleteAsync(41, "WS-01"),
        Times.Once
    );
}


}

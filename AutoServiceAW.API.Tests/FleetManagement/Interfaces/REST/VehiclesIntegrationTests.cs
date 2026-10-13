using System.Net;
using System.Net.Http.Json;
using System.Security.Claims;
using System.Text.Encodings.Web;
using System.Text.Json;
using AutoServiceAW.API.CustomerManagement.Domain.Services;
using AutoServiceAW.API.FleetManagement.Application.Internal;
using AutoServiceAW.API.FleetManagement.Domain.Model.Aggregates;
using AutoServiceAW.API.FleetManagement.Domain.Repositories;
using AutoServiceAW.API.FleetManagement.Domain.Services;
using AutoServiceAW.API.FleetManagement.Interfaces.REST;
using AutoServiceAW.API.Shared.Domain.Repositories;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;

namespace AutoServiceAW.API.Tests.FleetManagement.Interfaces.REST;

[TestClass]
public class VehiclesIntegrationTests
{
    /// <summary>
    /// Verifies that POST /api/v1/vehicles with a valid payload creates a vehicle,
    /// persists it via the repository, completes the unit of work, and returns HTTP 201 Created.
    /// </summary>
    [TestMethod]
    public async Task CreateVehicle_WithValidResource_ShouldReturnCreatedAndPersistVehicle()
    {
        // ARRANGE
        Vehicle? persistedVehicle = null;
        var vehicleRepositoryMock = new Mock<IVehicleRepository>();
        var unitOfWorkMock = new Mock<IUnitOfWork>();
        var customerServiceMock = new Mock<ICustomerService>();

        vehicleRepositoryMock
            .Setup(repository => repository.AddAsync(
                It.IsAny<Vehicle>(),
                It.IsAny<CancellationToken>()
            ))
            .Callback<Vehicle, CancellationToken>(
                (vehicle, _) => persistedVehicle = vehicle
            )
            .Returns(Task.CompletedTask);

        unitOfWorkMock
            .Setup(unitOfWork => unitOfWork.CompleteAsync())
            .Returns(Task.CompletedTask);

        var builder = WebApplication.CreateBuilder();
        builder.WebHost.UseTestServer();

        builder.Services
            .AddControllers()
            .AddApplicationPart(typeof(VehiclesController).Assembly);

        builder.Services.AddSingleton<IVehicleRepository>(vehicleRepositoryMock.Object);
        builder.Services.AddSingleton<IUnitOfWork>(unitOfWorkMock.Object);
        builder.Services.AddSingleton<ICustomerService>(customerServiceMock.Object);
        builder.Services.AddScoped<IVehicleService, VehicleService>();

        builder.Services
            .AddAuthentication("Test")
            .AddScheme<AuthenticationSchemeOptions, VehicleTestAuthHandler>(
                "Test",
                _ => { }
            );
        builder.Services.AddAuthorization();

        await using var app = builder.Build();
        app.UseAuthentication();
        app.UseAuthorization();
        app.MapControllers();
        await app.StartAsync();

        var client = app.GetTestClient();
        var request = new CreateVehicleResource(
            "ABC-123",
            "Toyota",
            "Corolla",
            "2024",
            "Black",
            "ACTIVE",
            "https://example.test/images/corolla.jpg",
            1
        );

        // ACT
        var response = await client.PostAsJsonAsync(
            "/api/v1/vehicles",
            request
        );

        // ASSERT
        Assert.AreEqual(HttpStatusCode.Created, response.StatusCode);
        Assert.IsNotNull(persistedVehicle);
        Assert.AreEqual("ABC-123", persistedVehicle.Plate);
        Assert.AreEqual("Toyota", persistedVehicle.Brand);
        Assert.AreEqual("Corolla", persistedVehicle.Model);
        Assert.AreEqual("2024", persistedVehicle.Year);
        Assert.AreEqual("Black", persistedVehicle.Color);
        Assert.AreEqual("ACTIVE", persistedVehicle.Status);
        Assert.AreEqual(1, persistedVehicle.CustomerId);

        var responseJson = await response.Content.ReadFromJsonAsync<JsonElement>();
        Assert.AreEqual("ABC-123", responseJson.GetProperty("plate").GetString());
        Assert.AreEqual("Toyota", responseJson.GetProperty("brand").GetString());
        Assert.AreEqual("Corolla", responseJson.GetProperty("model").GetString());
        Assert.AreEqual("ACTIVE", responseJson.GetProperty("status").GetString());

        vehicleRepositoryMock.Verify(
            repository => repository.AddAsync(
                It.IsAny<Vehicle>(),
                It.IsAny<CancellationToken>()
            ),
            Times.Once
        );

        unitOfWorkMock.Verify(
            unitOfWork => unitOfWork.CompleteAsync(),
            Times.Once
        );
    }

    /// <summary>
    /// Verifies that GET /api/v1/vehicles/{id} returns HTTP 200 OK with the vehicle data when it exists.
    /// </summary>
    [TestMethod]
    public async Task GetVehicleById_WhenVehicleExists_ShouldReturnOkWithVehicleData()
    {
        // ARRANGE
        const int vehicleId = 10;
        var existingVehicle = new Vehicle(
            "ABC-123",
            "Toyota",
            "Corolla",
            "2024",
            "Black",
            "ACTIVE",
            "corolla.jpg",
            1
        );

        var vehicleRepositoryMock = new Mock<IVehicleRepository>();
        var unitOfWorkMock = new Mock<IUnitOfWork>();
        var customerServiceMock = new Mock<ICustomerService>();

        vehicleRepositoryMock
            .Setup(repository => repository.FindByIdAsync(
                vehicleId,
                It.IsAny<CancellationToken>()
            ))
            .ReturnsAsync(existingVehicle);

        var builder = WebApplication.CreateBuilder();
        builder.WebHost.UseTestServer();

        builder.Services
            .AddControllers()
            .AddApplicationPart(typeof(VehiclesController).Assembly);

        builder.Services.AddSingleton<IVehicleRepository>(vehicleRepositoryMock.Object);
        builder.Services.AddSingleton<IUnitOfWork>(unitOfWorkMock.Object);
        builder.Services.AddSingleton<ICustomerService>(customerServiceMock.Object);
        builder.Services.AddScoped<IVehicleService, VehicleService>();

        builder.Services
            .AddAuthentication("Test")
            .AddScheme<AuthenticationSchemeOptions, VehicleTestAuthHandler>(
                "Test",
                _ => { }
            );
        builder.Services.AddAuthorization();

        await using var app = builder.Build();
        app.UseAuthentication();
        app.UseAuthorization();
        app.MapControllers();
        await app.StartAsync();

        var client = app.GetTestClient();

        // ACT
        var response = await client.GetAsync($"/api/v1/vehicles/{vehicleId}");

        // ASSERT
        Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);

        var responseJson = await response.Content.ReadFromJsonAsync<JsonElement>();
        Assert.AreEqual("ABC-123", responseJson.GetProperty("plate").GetString());
        Assert.AreEqual("Toyota", responseJson.GetProperty("brand").GetString());
        Assert.AreEqual("Corolla", responseJson.GetProperty("model").GetString());
        Assert.AreEqual("2024", responseJson.GetProperty("year").GetString());
        Assert.AreEqual("Black", responseJson.GetProperty("color").GetString());
        Assert.AreEqual("ACTIVE", responseJson.GetProperty("status").GetString());
        Assert.AreEqual(1, responseJson.GetProperty("customerId").GetInt32());

        vehicleRepositoryMock.Verify(
            repository => repository.FindByIdAsync(
                vehicleId,
                It.IsAny<CancellationToken>()
            ),
            Times.Once
        );
    }
}

internal sealed class VehicleTestAuthHandler(
    IOptionsMonitor<AuthenticationSchemeOptions> options,
    ILoggerFactory logger,
    UrlEncoder encoder
) : AuthenticationHandler<AuthenticationSchemeOptions>(
    options,
    logger,
    encoder
)
{
    protected override Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        var identity = new ClaimsIdentity(
            [
                new Claim(ClaimTypes.Name, "test-user"),
                new Claim("WorkshopId", "WS-101")
            ],
            Scheme.Name
        );
        var principal = new ClaimsPrincipal(identity);
        var ticket = new AuthenticationTicket(principal, Scheme.Name);

        return Task.FromResult(AuthenticateResult.Success(ticket));
    }
}

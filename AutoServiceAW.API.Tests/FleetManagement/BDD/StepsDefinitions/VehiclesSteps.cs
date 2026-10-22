
using System.Net;
using System.Net.Http.Json;
using AutoServiceAW.API.CustomerManagement.Domain.Services;
using AutoServiceAW.API.FleetManagement.Application.Internal;
using AutoServiceAW.API.FleetManagement.Domain.Model.Aggregates;
using AutoServiceAW.API.FleetManagement.Domain.Repositories;
using AutoServiceAW.API.FleetManagement.Domain.Services;
using AutoServiceAW.API.FleetManagement.Interfaces.REST;
using AutoServiceAW.API.Shared.Domain.Repositories;
using AutoServiceAW.API.Tests.WorkshopOperations;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Reqnroll;

namespace AutoServiceAW.API.Tests.FleetManagement.BDD.StepsDefinitions;

[Binding]
public sealed class VehiclesSteps
{
    private readonly Mock<IVehicleRepository> _vehicleRepositoryMock = new();
    private readonly Mock<IUnitOfWork> _unitOfWorkMock = new();
    private readonly Mock<ICustomerService> _customerServiceMock = new();

    private Vehicle? _vehicle;
    private HttpResponseMessage? _response;
    private WebApplication? _app;
    private int _customerId;

    private async System.Threading.Tasks.Task<HttpClient>
        CreateAdminClientAsync()
    {
        var builder = WebApplication.CreateBuilder();

        builder.WebHost.UseTestServer();

        builder.Services
            .AddAuthentication("Test")
            .AddScheme<
                AuthenticationSchemeOptions,
                TestAuthenticationHandler
            >("Test", _ => { });

        builder.Services.AddAuthorization();

        builder.Services.AddControllers()
            .AddApplicationPart(typeof(VehiclesController).Assembly);

        builder.Services.AddSingleton<IVehicleRepository>(
            _vehicleRepositoryMock.Object
        );

        builder.Services.AddSingleton<IUnitOfWork>(
            _unitOfWorkMock.Object
        );

        builder.Services.AddSingleton<ICustomerService>(
            _customerServiceMock.Object
        );

        builder.Services.AddScoped<IVehicleService, VehicleService>();

        _app = builder.Build();

        _app.UseAuthentication();
        _app.UseAuthorization();
        _app.MapControllers();

        await _app.StartAsync();

        return _app.GetTestClient();
    }



    [Given("a customer with ID {int} is registered in the workshop")]
    public void GivenACustomerIsRegistered(int customerId)
    {
        Assert.IsGreaterThan(
            0,
            customerId,
            "The customer ID must be positive."
        );

        // Customer context for this scenario.
        // The current POST endpoint does not query
        // ICustomerService to verify customer existence.
        _customerId = customerId;

        _vehicleRepositoryMock
            .Setup(repository => repository.AddAsync(
                It.IsAny<Vehicle>()
            ))
            .Callback<Vehicle, CancellationToken>((vehicle, cancellationToken) =>
            {
                _vehicle = vehicle;
            })
            .Returns(System.Threading.Tasks.Task.CompletedTask);

        _unitOfWorkMock
            .Setup(unit => unit.CompleteAsync())
            .Returns(System.Threading.Tasks.Task.CompletedTask);
    }

    [When(
        "an administrator registers a vehicle with plate {string}, brand {string}, model {string}, year {string}, color {string}, and status {string} for customer {int}"
    )]
    public async System.Threading.Tasks.Task
        WhenAnAdministratorRegistersAVehicle(
            string plate,
            string brand,
            string model,
            string year,
            string color,
            string status,
            int customerId
        )
    {
        Assert.AreEqual(
            _customerId,
            customerId,
            "The vehicle must use the customer from the Given step."
        );

        var client = await CreateAdminClientAsync();

        var request = new CreateVehicleResource(
            Plate: plate,
            Brand: brand,
            Model: model,
            Year: year,
            Color: color,
            Status: status,
            Image: "",
            CustomerId: customerId
        );

        _response = await client.PostAsJsonAsync(
            "/api/v1/Vehicles",
            request
        );
    }

    [Then("the vehicle is successfully registered in the fleet")]
    public void ThenTheVehicleIsSuccessfullyRegistered()
    {
        Assert.IsNotNull(
            _response,
            "The HTTP response should exist."
        );

        Assert.AreEqual(
            HttpStatusCode.Created,
            _response.StatusCode,
            "A successful vehicle registration must return 201 Created."
        );

        Assert.IsNotNull(
            _vehicle,
            "The vehicle should have been sent to the repository."
        );

        _vehicleRepositoryMock.Verify(
            repository => repository.AddAsync(
                It.Is<Vehicle>(vehicle =>
                    vehicle.Plate == "ABC-123" &&
                    vehicle.Brand == "Toyota" &&
                    vehicle.Model == "Corolla" &&
                    vehicle.Year == "2024" &&
                    vehicle.Color == "Black"
                )
            ),
            Times.Once
        );

        _unitOfWorkMock.Verify(
            unit => unit.CompleteAsync(),
            Times.Once
        );
    }

    [Then("the vehicle has status {string}")]
    public void ThenTheVehicleHasStatus(string expectedStatus)
    {
        Assert.IsNotNull(
            _vehicle,
            "The vehicle should have been created."
        );

        Assert.AreEqual(
            expectedStatus,
            _vehicle.Status
        );
    }

    [Then("the vehicle is associated with customer ID {int}")]
    public void ThenTheVehicleIsAssociatedWithCustomer(int expectedCustomerId)
    {
        Assert.IsNotNull(
            _vehicle,
            "The vehicle should have been created."
        );

        Assert.AreEqual(
            expectedCustomerId,
            _vehicle.CustomerId
        );

        _vehicleRepositoryMock.Verify(
            repository => repository.AddAsync(
                It.Is<Vehicle>(vehicle =>
                    vehicle.CustomerId == expectedCustomerId
                )
            ),
            Times.Once
        );
    }

    // ==============================================
    // SCENARIO 2: UPDATE EXISTING VEHICLE
    // ==============================================

    [Given("a vehicle with ID {int} and plate {string} exists in the fleet")]
    public void GivenAVehicleExistsInTheFleet(
        int vehicleId,
        string plate
    )
    {
        _vehicle = new Vehicle(
            plate,
            "Toyota",
            "Corolla",
            "2024",
            "Black",
            "ACTIVE",
            "",
            1
        );

        _vehicleRepositoryMock
            .Setup(repository => repository.FindByIdAsync(vehicleId))
            .ReturnsAsync(_vehicle);

        _unitOfWorkMock
            .Setup(unit => unit.CompleteAsync())
            .Returns(System.Threading.Tasks.Task.CompletedTask);
    }

    [When("an administrator updates the vehicle status to {string} and color to {string}")]
    public async System.Threading.Tasks.Task
        WhenAnAdministratorUpdatesTheVehicle(
            string newStatus,
            string newColor
        )
    {
        var client = await CreateAdminClientAsync();

        var request = new CreateVehicleResource(
            Plate: "ABC-123",
            Brand: "Toyota",
            Model: "Corolla",
            Year: "2024",
            Color: newColor,
            Status: newStatus,
            Image: "",
            CustomerId: 1
        );

        _response = await client.PutAsJsonAsync(
            "/api/v1/Vehicles/1",
            request
        );
    }

    [Then("the vehicle reflects the updated status {string}")]
    public void ThenTheVehicleReflectsTheUpdatedStatus(
        string expectedStatus
    )
    {
        Assert.IsNotNull(_response);
        Assert.AreEqual(
            HttpStatusCode.OK,
            _response.StatusCode
        );

        Assert.IsNotNull(_vehicle);
        Assert.AreEqual(
            expectedStatus,
            _vehicle.Status
        );

        _vehicleRepositoryMock.Verify(
            repository => repository.Update(
                It.Is<Vehicle>(vehicle =>
                    vehicle.Status == expectedStatus
                )
            ),
            Times.Once
        );
    }

    [Then("the vehicle exterior color is updated to {string}")]
    public void ThenTheVehicleExteriorColorIsUpdated(
        string expectedColor
    )
    {
        Assert.IsNotNull(_vehicle);

        Assert.AreEqual(
            expectedColor,
            _vehicle.Color
        );
    }

    [Then("the vehicle remains associated with customer ID {int}")]
    public void ThenTheVehicleRemainsAssociatedWithCustomer(
        int expectedCustomerId
    )
    {
        Assert.IsNotNull(_vehicle);

        Assert.AreEqual(
            expectedCustomerId,
            _vehicle.CustomerId
        );

        _vehicleRepositoryMock.Verify(
            repository => repository.Update(
                It.Is<Vehicle>(vehicle =>
                    vehicle.CustomerId == expectedCustomerId &&
                    vehicle.Status == "IN_REPAIR" &&
                    vehicle.Color == "White"
                )
            ),
            Times.Once
        );

        _unitOfWorkMock.Verify(
            unit => unit.CompleteAsync(),
            Times.Once
        );
    }

    // ==============================================
    // SCENARIO 3: QUERY VEHICLE BY ID
    // ==============================================

    [Given("a vehicle with ID {int} and plate {string} is registered in the fleet")]
    public void GivenAVehicleIsRegisteredInTheFleet(
        int vehicleId,
        string plate
    )
    {
        _vehicle = new Vehicle(
            plate,
            "Toyota",
            "Corolla",
            "2024",
            "Black",
            "ACTIVE",
            "",
            1
        );

        _vehicleRepositoryMock
            .Setup(repository => repository.FindByIdAsync(vehicleId))
            .ReturnsAsync(_vehicle);
    }

    [When("an authorized user requests the vehicle details for ID {int}")]
    public async System.Threading.Tasks.Task
        WhenAnAuthorizedUserRequestsVehicleDetails(int vehicleId)
    {
        var client = await CreateAdminClientAsync();

        _response = await client.GetAsync(
            $"/api/v1/Vehicles/{vehicleId}"
        );
    }

    [Then("the vehicle details should be returned successfully")]
    public void ThenTheVehicleDetailsShouldBeReturnedSuccessfully()
    {
        Assert.IsNotNull(
            _response,
            "The HTTP response should exist."
        );

        Assert.AreEqual(
            HttpStatusCode.OK,
            _response.StatusCode,
            "The vehicle query should return 200 OK."
        );

        _vehicleRepositoryMock.Verify(
            repository => repository.FindByIdAsync(1),
            Times.Once
        );
    }

    [Then("the response should contain the license plate {string}")]
    public async System.Threading.Tasks.Task
        ThenTheResponseShouldContainTheLicensePlate(string expectedPlate)
    {
        Assert.IsNotNull(_response);

        using var json = System.Text.Json.JsonDocument.Parse(
            await _response.Content.ReadAsStringAsync()
        );

        var actualPlate = json.RootElement
            .GetProperty("plate")
            .GetString();

        Assert.AreEqual(
            expectedPlate,
            actualPlate
        );
    }

    [Then("the response should contain the brand {string}")]
    public async System.Threading.Tasks.Task
        ThenTheResponseShouldContainTheBrand(string expectedBrand)
    {
        Assert.IsNotNull(_response);

        using var json = System.Text.Json.JsonDocument.Parse(
            await _response.Content.ReadAsStringAsync()
        );

        var actualBrand = json.RootElement
            .GetProperty("brand")
            .GetString();

        Assert.AreEqual(
            expectedBrand,
            actualBrand
        );
    }

    // ==============================================
    // CLEANUP
    // ==============================================

    [AfterScenario]
    public async System.Threading.Tasks.Task CleanupAsync()
    {
        _response?.Dispose();

        if (_app != null)
        {
            await _app.DisposeAsync();
        }
    }
}

using System.IdentityModel.Tokens.Jwt;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Security.Claims;
using System.Text;
using System.Text.Json;
using AutoServiceAW.API.CustomerManagement.Domain.Model.Aggregates;
using AutoServiceAW.API.CustomerManagement.Domain.Services;
using AutoServiceAW.API.FleetManagement.Application.Internal;
using AutoServiceAW.API.FleetManagement.Domain.Model.Aggregates;
using AutoServiceAW.API.FleetManagement.Domain.Repositories;
using AutoServiceAW.API.FleetManagement.Domain.Services;
using AutoServiceAW.API.FleetManagement.Interfaces.REST;
using AutoServiceAW.API.Shared.Domain.Repositories;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using Moq;

namespace AutoServiceAW.API.Tests.FleetManagement.Acceptance;

[TestClass]
public class VehicleAcceptanceTests
{
    private const string JwtSecret = "AutoServiceTestSecretKey12345678901234567890";
    private const string JwtIssuer = "AutoServiceTest";
    private const string JwtAudience = "AutoServiceTestUsers";

    /// <summary>
    /// System test verifying a complete end-to-end flow through the REST API interface:
    /// 1. Create a new vehicle via POST /api/v1/vehicles
    /// 2. Retrieve the created vehicle by its ID via GET /api/v1/vehicles/{id}
    /// 3. Update the vehicle details and maintenance status via PUT /api/v1/vehicles/{id}
    /// 4. List all workshop vehicles via GET /api/v1/vehicles and verify filtering
    /// 5. Remove the vehicle via DELETE /api/v1/vehicles/{id}
    /// 6. Confirm the vehicle is no longer accessible via GET /api/v1/vehicles/{id} (404 Not Found)
    /// </summary>
    [TestMethod]
    public async Task VehicleLifecycle_FromCreationToRetrievalUpdateAndDeletion_ShouldSucceed()
    {
        // ARRANGE
        const string workshopId = "WS-01";
        const int customerId = 1;

        var vehicleStore = new Dictionary<int, Vehicle>();
        var idSequence = 1;

        var vehicleRepositoryMock = new Mock<IVehicleRepository>();
        var unitOfWorkMock = new Mock<IUnitOfWork>();
        var customerServiceMock = new Mock<ICustomerService>();

        vehicleRepositoryMock
            .Setup(r => r.AddAsync(It.IsAny<Vehicle>(), It.IsAny<CancellationToken>()))
            .Callback<Vehicle, CancellationToken>((v, _) =>
            {
                typeof(Vehicle).GetProperty("Id")?.SetValue(v, idSequence);
                vehicleStore[idSequence] = v;
                idSequence++;
            })
            .Returns(Task.CompletedTask);

        vehicleRepositoryMock
            .Setup(r => r.FindByIdAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((int id, CancellationToken _) => vehicleStore.TryGetValue(id, out var v) ? v : null);

        vehicleRepositoryMock
            .Setup(r => r.ListAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(() => vehicleStore.Values.ToList());

        vehicleRepositoryMock
            .Setup(r => r.Update(It.IsAny<Vehicle>()))
            .Callback<Vehicle>(v => vehicleStore[v.Id] = v);

        vehicleRepositoryMock
            .Setup(r => r.Remove(It.IsAny<Vehicle>()))
            .Callback<Vehicle>(v => vehicleStore.Remove(v.Id));

        unitOfWorkMock
            .Setup(u => u.CompleteAsync())
            .Returns(Task.CompletedTask);

        var registeredCustomer = new Customer(
            workshopId,
            "Carlos Perez",
            "12345678",
            "carlos@example.com",
            "987654321"
        );
        typeof(Customer).GetProperty("Id")?.SetValue(registeredCustomer, customerId);

        customerServiceMock
            .Setup(c => c.ListAsync())
            .ReturnsAsync(new List<Customer> { registeredCustomer });

        var builder = WebApplication.CreateBuilder();
        builder.WebHost.UseTestServer();

        builder.Configuration["Jwt:Secret"] = JwtSecret;
        builder.Configuration["Jwt:Issuer"] = JwtIssuer;
        builder.Configuration["Jwt:Audience"] = JwtAudience;

        builder.Services
            .AddControllers()
            .AddApplicationPart(typeof(VehiclesController).Assembly);

        builder.Services.AddSingleton<IVehicleRepository>(vehicleRepositoryMock.Object);
        builder.Services.AddSingleton<IUnitOfWork>(unitOfWorkMock.Object);
        builder.Services.AddSingleton<ICustomerService>(customerServiceMock.Object);
        builder.Services.AddScoped<IVehicleService, VehicleService>();

        var jwtKey = Encoding.ASCII.GetBytes(JwtSecret);
        builder.Services
            .AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer(options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    ValidIssuer = JwtIssuer,
                    ValidAudience = JwtAudience,
                    IssuerSigningKey = new SymmetricSecurityKey(jwtKey)
                };
            });

        builder.Services.AddAuthorization();

        await using var app = builder.Build();
        app.UseAuthentication();
        app.UseAuthorization();
        app.MapControllers();
        await app.StartAsync();

        var client = app.GetTestClient();

        var token = GenerateJwtToken("admin@autoservice.com", "admin", workshopId);
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        // ACT & ASSERT - STEP 1: CREATE VEHICLE THROUGH REST INTERFACE
        var createResource = new CreateVehicleResource(
            "ABC-123",
            "Toyota",
            "Corolla",
            "2024",
            "Black",
            "ACTIVE",
            "https://example.test/corolla.png",
            customerId
        );

        var createResponse = await client.PostAsJsonAsync("/api/v1/vehicles", createResource);
        Assert.AreEqual(HttpStatusCode.Created, createResponse.StatusCode);

        var createdJson = await createResponse.Content.ReadFromJsonAsync<JsonElement>();
        var createdVehicleId = createdJson.GetProperty("id").GetInt32();
        Assert.IsGreaterThan(0, createdVehicleId);
        Assert.AreEqual(1, createdVehicleId);
        Assert.AreEqual("ACTIVE", createdJson.GetProperty("status").GetString());
        Assert.IsTrue(vehicleStore.ContainsKey(createdVehicleId));

        // ACT & ASSERT - STEP 2: GET VEHICLE BY ID THROUGH REST INTERFACE
        var getByIdResponse = await client.GetAsync($"/api/v1/vehicles/{createdVehicleId}");
        Assert.AreEqual(HttpStatusCode.OK, getByIdResponse.StatusCode);

        var retrievedJson = await getByIdResponse.Content.ReadFromJsonAsync<JsonElement>();
        Assert.AreEqual(createdVehicleId, retrievedJson.GetProperty("id").GetInt32());
        Assert.AreEqual("ABC-123", retrievedJson.GetProperty("plate").GetString());
        Assert.AreEqual("Toyota", retrievedJson.GetProperty("brand").GetString());
        Assert.AreEqual("Black", retrievedJson.GetProperty("color").GetString());

        // ACT & ASSERT - STEP 3: UPDATE VEHICLE DETAILS & STATUS THROUGH REST INTERFACE
        var updateResource = new CreateVehicleResource(
            "ABC-123",
            "Toyota",
            "Corolla",
            "2024",
            "White",
            "IN_REPAIR",
            "https://example.test/corolla-white.png",
            customerId
        );

        var updateResponse = await client.PutAsJsonAsync($"/api/v1/vehicles/{createdVehicleId}", updateResource);
        Assert.AreEqual(HttpStatusCode.OK, updateResponse.StatusCode);

        var updatedJson = await updateResponse.Content.ReadFromJsonAsync<JsonElement>();
        Assert.AreEqual("IN_REPAIR", updatedJson.GetProperty("status").GetString());
        Assert.AreEqual("White", updatedJson.GetProperty("color").GetString());
        Assert.AreEqual("IN_REPAIR", vehicleStore[createdVehicleId].Status);
        Assert.AreEqual("White", vehicleStore[createdVehicleId].Color);

        // ACT & ASSERT - STEP 4: GET ALL WORKSHOP VEHICLES THROUGH REST INTERFACE
        var getAllResponse = await client.GetAsync("/api/v1/vehicles");
        Assert.AreEqual(HttpStatusCode.OK, getAllResponse.StatusCode);

        var fleetList = await getAllResponse.Content.ReadFromJsonAsync<List<JsonElement>>();
        Assert.IsNotNull(fleetList);
        Assert.HasCount(1, fleetList);
        Assert.AreEqual("ABC-123", fleetList[0].GetProperty("plate").GetString());
        Assert.AreEqual("IN_REPAIR", fleetList[0].GetProperty("status").GetString());

        // ACT & ASSERT - STEP 5: DELETE VEHICLE THROUGH REST INTERFACE
        var deleteResponse = await client.DeleteAsync($"/api/v1/vehicles/{createdVehicleId}");
        Assert.AreEqual(HttpStatusCode.NoContent, deleteResponse.StatusCode);
        Assert.IsFalse(vehicleStore.ContainsKey(createdVehicleId));

        // ACT & ASSERT - STEP 6: VERIFY VEHICLE DELETION (404 NOT FOUND)
        var verifyDeletedResponse = await client.GetAsync($"/api/v1/vehicles/{createdVehicleId}");
        Assert.AreEqual(HttpStatusCode.NotFound, verifyDeletedResponse.StatusCode);
    }

    /// <summary>
    /// Verifies that querying the vehicles list without a WorkshopId claim in the token returns 401 Unauthorized.
    /// </summary>
    [TestMethod]
    public async Task GetAllVehicles_WithoutWorkshopIdClaim_ShouldReturnUnauthorized()
    {
        // ARRANGE
        var builder = WebApplication.CreateBuilder();
        builder.WebHost.UseTestServer();

        builder.Configuration["Jwt:Secret"] = JwtSecret;
        builder.Configuration["Jwt:Issuer"] = JwtIssuer;
        builder.Configuration["Jwt:Audience"] = JwtAudience;

        builder.Services
            .AddControllers()
            .AddApplicationPart(typeof(VehiclesController).Assembly);

        builder.Services.AddSingleton(Mock.Of<IVehicleRepository>());
        builder.Services.AddSingleton(Mock.Of<IUnitOfWork>());
        builder.Services.AddSingleton(Mock.Of<ICustomerService>());
        builder.Services.AddScoped<IVehicleService, VehicleService>();

        var jwtKey = Encoding.ASCII.GetBytes(JwtSecret);
        builder.Services
            .AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer(options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    ValidIssuer = JwtIssuer,
                    ValidAudience = JwtAudience,
                    IssuerSigningKey = new SymmetricSecurityKey(jwtKey)
                };
            });

        builder.Services.AddAuthorization();

        await using var app = builder.Build();
        app.UseAuthentication();
        app.UseAuthorization();
        app.MapControllers();
        await app.StartAsync();

        var client = app.GetTestClient();

        // Token generated WITHOUT WorkshopId claim
        var tokenWithoutWorkshop = GenerateJwtToken("mechanic@autoservice.com", "mechanic", null);
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", tokenWithoutWorkshop);

        // ACT
        var response = await client.GetAsync("/api/v1/vehicles");

        // ASSERT
        Assert.AreEqual(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    private static string GenerateJwtToken(string email, string role, string? workshopId)
    {
        var tokenHandler = new JwtSecurityTokenHandler();
        var key = Encoding.ASCII.GetBytes(JwtSecret);

        var claims = new List<Claim>
        {
            new(ClaimTypes.Name, email),
            new("email", email),
            new("role", role)
        };

        if (!string.IsNullOrEmpty(workshopId))
        {
            claims.Add(new Claim("WorkshopId", workshopId));
        }

        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(claims),
            Expires = DateTime.UtcNow.AddHours(1),
            Issuer = JwtIssuer,
            Audience = JwtAudience,
            SigningCredentials = new SigningCredentials(
                new SymmetricSecurityKey(key),
                SecurityAlgorithms.HmacSha256Signature
            )
        };

        var token = tokenHandler.CreateToken(tokenDescriptor);
        return tokenHandler.WriteToken(token);
    }
}

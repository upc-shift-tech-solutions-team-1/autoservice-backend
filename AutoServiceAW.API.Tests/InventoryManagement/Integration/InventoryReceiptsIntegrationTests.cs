
using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using AutoServiceAW.API.InventoryManagement.Application.Internal;
using AutoServiceAW.API.InventoryManagement.Domain.Model.Aggregates;
using AutoServiceAW.API.InventoryManagement.Domain.Repositories;
using AutoServiceAW.API.InventoryManagement.Domain.Services;
using AutoServiceAW.API.InventoryManagement.Interfaces.REST;
using AutoServiceAW.API.Shared.Domain.Repositories;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.IdentityModel.Tokens;
using Moq;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace AutoServiceAW.API.Tests.InventoryManagement.Integration;

[TestClass]
public class InventoryReceiptsIntegrationTests
{
    private const string JwtSecret =
        "AutoServiceInventoryTestsSecretKey2026ForHmacSha256";

    private static InventoryItem CreateInventoryItem()
    {
        return new InventoryItem(
            "Brake Pad",
            "Brakes",
            "Bosch",
            100m,
            10,
            2,
            "brake-pad.jpg"
        );
    }

    private static async System.Threading.Tasks.Task<WebApplication>
        CreateTestAppAsync(
            Mock<IInventoryItemRepository> repositoryMock,
            Mock<IUnitOfWork> unitOfWorkMock
        )
    {
        var builder = WebApplication.CreateBuilder();

        builder.WebHost.UseTestServer();

        builder.Services
            .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(options =>
            {
                options.TokenValidationParameters =
                    new TokenValidationParameters
                    {
                        ValidateIssuer = true,
                        ValidIssuer = "AutoServiceInventoryTests",

                        ValidateAudience = true,
                        ValidAudience = "AutoServiceInventoryClients",

                        ValidateLifetime = true,

                        ValidateIssuerSigningKey = true,
                        IssuerSigningKey = new SymmetricSecurityKey(
                            Encoding.UTF8.GetBytes(JwtSecret)
                        )
                    };
            });

        builder.Services.AddAuthorization();

        builder.Services.AddControllers()
            .AddApplicationPart(
                typeof(InventoryItemsController).Assembly
            );

        builder.Services.AddSingleton<IInventoryItemRepository>(
            repositoryMock.Object
        );

        builder.Services.AddSingleton<IUnitOfWork>(
            unitOfWorkMock.Object
        );

        builder.Services.AddScoped<
            IInventoryItemService,
            InventoryItemService
        >();

        var app = builder.Build();

        app.UseAuthentication();
        app.UseAuthorization();
        app.MapControllers();

        await app.StartAsync();

        return app;
    }

    private static string CreateJwt(string role)
    {
        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, "test-user"),
            new Claim(ClaimTypes.Role, role),
            new Claim("WorkshopId", "WS-01")
        };

        var token = new JwtSecurityToken(
            issuer: "AutoServiceInventoryTests",
            audience: "AutoServiceInventoryClients",
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(30),
            signingCredentials: new SigningCredentials(
                new SymmetricSecurityKey(
                    Encoding.UTF8.GetBytes(JwtSecret)
                ),
                SecurityAlgorithms.HmacSha256
            )
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    [TestMethod]
    public async System.Threading.Tasks.Task
        ReceiveStock_AsAdmin_ShouldIncreaseStockAndReturnOk()
    {
        // Arrange
        var item = CreateInventoryItem();

        var repositoryMock =
            new Mock<IInventoryItemRepository>();

        var unitOfWorkMock =
            new Mock<IUnitOfWork>();

        repositoryMock
            .Setup(repository => repository.FindByIdAsync(1))
            .ReturnsAsync(item);

        unitOfWorkMock
            .Setup(unit => unit.CompleteAsync())
            .Returns(System.Threading.Tasks.Task.CompletedTask);

        await using var app = await CreateTestAppAsync(
            repositoryMock,
            unitOfWorkMock
        );

        using var client = app.GetTestClient();

        client.DefaultRequestHeaders.Authorization =
            new System.Net.Http.Headers.AuthenticationHeaderValue(
                "Bearer",
                CreateJwt("admin")
            );

        var request = new ReceiveInventoryStockResource(
            Quantity: 5,
            ProviderName: "Bosch",
            DocumentNumber: "FAC-001",
            Notes: "Material received"
        );

        // Act
        using var response = await client.PostAsJsonAsync(
            "/api/v1/InventoryItems/1/receipts",
            request
        );

        // Assert
        Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
        Assert.AreEqual(15, item.Stock);

        using var json = JsonDocument.Parse(
            await response.Content.ReadAsStringAsync()
        );

        Assert.AreEqual(
            10,
            json.RootElement.GetProperty("previousStock").GetInt32()
        );

        Assert.AreEqual(
            15,
            json.RootElement.GetProperty("currentStock").GetInt32()
        );

        Assert.AreEqual(
            5,
            json.RootElement.GetProperty("quantityReceived").GetInt32()
        );

        repositoryMock.Verify(
            repository => repository.Update(
                It.Is<InventoryItem>(inventory => inventory.Stock == 15)
            ),
            Times.Once
        );

        unitOfWorkMock.Verify(
            unit => unit.CompleteAsync(),
            Times.Once
        );
    }

    [TestMethod]
    public async System.Threading.Tasks.Task
        ReceiveStock_AsMechanic_ShouldReturnForbiddenWithoutChangingStock()
    {
        // Arrange
        var item = CreateInventoryItem();

        var repositoryMock =
            new Mock<IInventoryItemRepository>();

        var unitOfWorkMock =
            new Mock<IUnitOfWork>();

        await using var app = await CreateTestAppAsync(
            repositoryMock,
            unitOfWorkMock
        );

        using var client = app.GetTestClient();

        client.DefaultRequestHeaders.Authorization =
            new System.Net.Http.Headers.AuthenticationHeaderValue(
                "Bearer",
                CreateJwt("mechanic")
            );

        var request = new ReceiveInventoryStockResource(
            Quantity: 5,
            ProviderName: "Bosch",
            DocumentNumber: "FAC-002",
            Notes: "Unauthorized attempt"
        );

        // Act
        using var response = await client.PostAsJsonAsync(
            "/api/v1/InventoryItems/1/receipts",
            request
        );

        // Assert
        Assert.AreEqual(
            HttpStatusCode.Forbidden,
            response.StatusCode
        );

        Assert.AreEqual(10, item.Stock);

        repositoryMock.VerifyNoOtherCalls();
        unitOfWorkMock.VerifyNoOtherCalls();
    }
}

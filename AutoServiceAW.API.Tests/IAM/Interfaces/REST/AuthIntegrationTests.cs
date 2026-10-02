using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using AutoServiceAW.API.IAM.Application.Internal;
using AutoServiceAW.API.IAM.Domain.Model.Aggregates;
using AutoServiceAW.API.IAM.Domain.Repositories;
using AutoServiceAW.API.IAM.Domain.Services;
using AutoServiceAW.API.IAM.Interfaces.REST;
using AutoServiceAW.API.Shared.Domain.Repositories;
using AutoServiceAW.API.StaffCoordination.Domain.Repositories;
using AutoServiceAW.API.TenantManagement.Domain.Services;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Moq;

namespace AutoServiceAW.API.Tests.IAM.Interfaces.REST;

[TestClass]
public class AuthIntegrationTests
{
    [TestMethod]
    public async Task SignIn_WithValidCredentials_ShouldReturnOkAndAuthenticationData()
    {
        // ARRANGE
        var email = "admin@autoservice.com";
        var password = "SecurePassword123";
        var workshopId = "WS-1";

        var passwordHash =
            BCrypt.Net.BCrypt.HashPassword(password);

        var existingUser = new User(
            email,
            passwordHash,
            "admin",
            workshopId
        );

        var userRepositoryMock =
            new Mock<IUserRepository>();

        var unitOfWorkMock =
            new Mock<IUnitOfWork>();

        var mechanicRepositoryMock =
            new Mock<IMechanicRepository>();

        var workshopServiceMock =
            new Mock<IWorkshopService>();

        userRepositoryMock
            .Setup(repository =>
                repository.FindByEmailAsync(email)
            )
            .ReturnsAsync(existingUser);

        var builder =
            WebApplication.CreateBuilder();

        builder.WebHost.UseTestServer();

        builder.Configuration["Jwt:Secret"] =
            "AutoServiceTestSecretKey12345678901234567890";

        builder.Configuration["Jwt:Issuer"] =
            "AutoServiceTest";

        builder.Configuration["Jwt:Audience"] =
            "AutoServiceTestUsers";

        builder.Configuration["Jwt:ExpirationInMinutes"] =
            "60";

        builder.Services
            .AddControllers()
            .AddApplicationPart(
                typeof(AuthController).Assembly
            );

        builder.Services.AddSingleton<IUserRepository>(
            userRepositoryMock.Object
        );

        builder.Services.AddSingleton<IUnitOfWork>(
            unitOfWorkMock.Object
        );

        builder.Services.AddSingleton<IMechanicRepository>(
            mechanicRepositoryMock.Object
        );

        builder.Services.AddSingleton<IWorkshopService>(
            workshopServiceMock.Object
        );

        builder.Services.AddScoped<
            IAuthService,
            AuthService
        >();

        await using var app =
            builder.Build();

        app.MapControllers();

        await app.StartAsync();

        var client =
            app.GetTestClient();

        var request =
            new SignInResource(
                email,
                password
            );

        // ACT
        var response =
            await client.PostAsJsonAsync(
                "/api/v1/auth/sign-in",
                request
            );

        // ASSERT
        Assert.AreEqual(
            HttpStatusCode.OK,
            response.StatusCode
        );

        var json =
            await response.Content
                .ReadFromJsonAsync<JsonElement>();

        Assert.AreEqual(
            email,
            json.GetProperty("email")
                .GetString()
        );

        Assert.AreEqual(
            "admin",
            json.GetProperty("role")
                .GetString()
        );

        Assert.AreEqual(
            workshopId,
            json.GetProperty("workshopId")
                .GetString()
        );

        Assert.IsTrue(
            json.TryGetProperty(
                "token",
                out var tokenElement
            )
        );

        Assert.IsFalse(
            string.IsNullOrWhiteSpace(
                tokenElement.GetString()
            )
        );

        userRepositoryMock.Verify(
            repository =>
                repository.FindByEmailAsync(email),
            Times.Once
        );
    }
}
using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using AutoServiceAW.API.IAM.Application.Internal;
using AutoServiceAW.API.IAM.Domain.Model.Aggregates;
using AutoServiceAW.API.IAM.Domain.Repositories;
using AutoServiceAW.API.IAM.Domain.Services;
using AutoServiceAW.API.IAM.Interfaces.REST;
using AutoServiceAW.API.Shared.Domain.Repositories;
using AutoServiceAW.API.StaffCoordination.Domain.Repositories;
using AutoServiceAW.API.TenantManagement.Domain.Model.Aggregates;
using AutoServiceAW.API.TenantManagement.Domain.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using Moq;

namespace AutoServiceAW.API.Tests.IAM.Acceptance;

[TestClass]
public class AuthenticationAcceptanceTests
{
    [TestMethod]
    public async Task RegisterWorkshop_Login_AndAccessAdminEndpoint_ShouldSucceed()
    {
        // ARRANGE
        var adminEmail = "admin@autoservice.com";
        var adminPassword = "SecurePassword123";

        User? persistedUser = null;

        var userRepositoryMock =
            new Mock<IUserRepository>();

        var unitOfWorkMock =
            new Mock<IUnitOfWork>();

        var workshopServiceMock =
            new Mock<IWorkshopService>();

        var mechanicRepositoryMock =
            new Mock<IMechanicRepository>();

        userRepositoryMock
            .Setup(repository =>
                repository.FindByEmailAsync(
                    It.IsAny<string>()
                )
            )
            .ReturnsAsync(
                (string email) =>
                    persistedUser != null &&
                    persistedUser.Email == email
                        ? persistedUser
                        : null
            );

        userRepositoryMock
            .Setup(repository =>
                repository.AddAsync(
                    It.IsAny<User>(),
                    It.IsAny<CancellationToken>()
                )
            )
            .Callback<User, CancellationToken>(
                (user, cancellationToken) =>
                {
                    persistedUser = user;
                }
            )
            .Returns(Task.CompletedTask);

        unitOfWorkMock
            .Setup(unitOfWork =>
                unitOfWork.CompleteAsync()
            )
            .Returns(Task.CompletedTask);

        workshopServiceMock
            .Setup(service =>
                service.CreateAsync(
                    It.IsAny<Workshop>()
                )
            )
            .ReturnsAsync(
                (Workshop workshop) =>
                    workshop
            );

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

        builder.Services.AddSingleton<IWorkshopService>(
            workshopServiceMock.Object
        );

        builder.Services.AddSingleton<IMechanicRepository>(
            mechanicRepositoryMock.Object
        );

        builder.Services.AddScoped<
            IAuthService,
            AuthService
        >();

        var jwtSecret =
            builder.Configuration["Jwt:Secret"]!;

        var jwtKey =
            Encoding.ASCII.GetBytes(
                jwtSecret
            );

        builder.Services
            .AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme =
                    JwtBearerDefaults.AuthenticationScheme;

                options.DefaultChallengeScheme =
                    JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer(options =>
            {
                options.TokenValidationParameters =
                    new TokenValidationParameters
                    {
                        ValidateIssuer = true,
                        ValidateAudience = true,
                        ValidateLifetime = true,
                        ValidateIssuerSigningKey = true,

                        ValidIssuer =
                            "AutoServiceTest",

                        ValidAudience =
                            "AutoServiceTestUsers",

                        IssuerSigningKey =
                            new SymmetricSecurityKey(
                                jwtKey
                            )
                    };
            });

        builder.Services.AddAuthorization();

        await using var app =
            builder.Build();

        app.UseAuthentication();
        app.UseAuthorization();

        app.MapControllers();

        await app.StartAsync();

        var client =
            app.GetTestClient();

        // ACT - STEP 1: REGISTER WORKSHOP
        var registerResponse =
            await client.PostAsJsonAsync(
                "/api/v1/auth/register-workshop",
                new SignUpWorkshopResource(
                    "TorqueLab Workshop",
                    adminEmail,
                    adminPassword
                )
            );

        // ASSERT - STEP 1
        Assert.AreEqual(
            HttpStatusCode.Created,
            registerResponse.StatusCode
        );

        Assert.IsNotNull(
            persistedUser
        );

        Assert.AreEqual(
            "admin",
            persistedUser.Role
        );

        Assert.IsFalse(
            string.IsNullOrWhiteSpace(
                persistedUser.WorkshopId
            )
        );

        var workshopId =
            persistedUser.WorkshopId;

        // ACT - STEP 2: LOGIN
        var loginResponse =
            await client.PostAsJsonAsync(
                "/api/v1/auth/sign-in",
                new SignInResource(
                    adminEmail,
                    adminPassword
                )
            );

        // ASSERT - STEP 2
        Assert.AreEqual(
            HttpStatusCode.OK,
            loginResponse.StatusCode
        );

        var loginJson =
            await loginResponse.Content
                .ReadFromJsonAsync<JsonElement>();

        Assert.AreEqual(
            "admin",
            loginJson
                .GetProperty("role")
                .GetString()
        );

        Assert.AreEqual(
            workshopId,
            loginJson
                .GetProperty("workshopId")
                .GetString()
        );

        var token =
            loginJson
                .GetProperty("token")
                .GetString();

        Assert.IsFalse(
            string.IsNullOrWhiteSpace(
                token
            )
        );

        // ACT - STEP 3:
        // ACCESS AN ADMIN-ONLY ENDPOINT
        client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue(
                "Bearer",
                token
            );

        var createUserResponse =
            await client.PostAsJsonAsync(
                "/api/v1/auth/sign-up",
                new SignUpResource(
                    "mechanic@autoservice.com",
                    "MechanicPassword123",
                    "mechanic",

                    // This value must be ignored.
                    // The workshop must come from the JWT.
                    "WS-INVALID"
                )
            );

        // ASSERT - STEP 3
        Assert.AreEqual(
            HttpStatusCode.Created,
            createUserResponse.StatusCode
        );

        Assert.IsNotNull(
            persistedUser
        );

        Assert.AreEqual(
            "mechanic",
            persistedUser.Role
        );

        Assert.AreEqual(
            workshopId,
            persistedUser.WorkshopId
        );

        Assert.AreNotEqual(
            "WS-INVALID",
            persistedUser.WorkshopId
        );
    }
}
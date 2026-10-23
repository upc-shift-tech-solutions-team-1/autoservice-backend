using System.Net;
using System.Net.Http.Json;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
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
using Reqnroll;
using Microsoft.Extensions.Configuration;
using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;

namespace AutoServiceAW.API.Tests.IAM.BDD.StepDefinitions;

[Binding]
public sealed class AuthenticationSteps
{
    private readonly Mock<IUserRepository> _userRepositoryMock = new();
    private readonly Mock<IUnitOfWork> _unitOfWorkMock = new();
    private readonly Mock<IWorkshopService> _workshopServiceMock = new();
    private readonly Mock<IMechanicRepository> _mechanicRepositoryMock = new();

    private User? _registeredUser;
    private HttpResponseMessage? _response;
    private WebApplication? _app;
    [Given("an administrator with email {string} is registered")]
    public void GivenAnAdministratorIsRegistered(string email)
    {
        _registeredUser = new User(
            email,
            BCrypt.Net.BCrypt.HashPassword("SecurePassword123"),
            "admin",
            "WS-01"
        );

        _userRepositoryMock
            .Setup(repository => repository.FindByEmailAsync(email))
            .ReturnsAsync(_registeredUser);
    }
    [Given("the administrator has password {string}")]
    public void GivenTheAdministratorHasPassword(string password)
    {
        Assert.IsNotNull(
            _registeredUser,
            "The administrator must be registered first."
        );

        Assert.IsTrue(
            BCrypt.Net.BCrypt.Verify(
                password,
                _registeredUser.PasswordHash
            ),
            "The registered administrator must have the specified password."
        );
    }
    private async System.Threading.Tasks.Task<HttpClient> CreateAuthClientAsync()
    {
        var builder = WebApplication.CreateBuilder();

        builder.WebHost.UseTestServer();

        builder.Configuration.AddInMemoryCollection(
            new Dictionary<string, string?>
            {
                ["Jwt:Secret"] =
                    "AutoServiceBddTestSecretKey2026ForHmacSha256",
                ["Jwt:Issuer"] = "AutoServiceBDD",
                ["Jwt:Audience"] = "AutoServiceBDDClients",
                ["Jwt:ExpirationInMinutes"] = "60"
            }
        );
        builder.Services
            .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(options =>
            {
                options.TokenValidationParameters =
                    new TokenValidationParameters
                    {
                        ValidateIssuer = true,
                        ValidIssuer = "AutoServiceBDD",

                        ValidateAudience = true,
                        ValidAudience = "AutoServiceBDDClients",

                        ValidateLifetime = true,

                        ValidateIssuerSigningKey = true,
                        IssuerSigningKey = new SymmetricSecurityKey(
                            Encoding.ASCII.GetBytes(
                                "AutoServiceBddTestSecretKey2026ForHmacSha256"
                            )
                        )
                    };
            });

        builder.Services.AddAuthorization();

        builder.Services.AddControllers()
            .AddApplicationPart(typeof(AuthController).Assembly);

        builder.Services.AddSingleton<IUserRepository>(
            _userRepositoryMock.Object
        );

        builder.Services.AddSingleton<IUnitOfWork>(
            _unitOfWorkMock.Object
        );

        builder.Services.AddSingleton<IWorkshopService>(
            _workshopServiceMock.Object
        );

        builder.Services.AddSingleton<IMechanicRepository>(
            _mechanicRepositoryMock.Object
        );

        builder.Services.AddScoped<IAuthService, AuthService>();

        _app = builder.Build();

        _app.UseAuthentication();
        _app.UseAuthorization();

        _app.MapControllers();

        await _app.StartAsync();

        return _app.GetTestClient();
    }
    [When("the administrator signs in with valid credentials")]
    public async System.Threading.Tasks.Task
        WhenTheAdministratorSignsInWithValidCredentials()
    {
        var client = await CreateAuthClientAsync();

        var request = new SignInResource(
            Email: "admin@autoservice.com",
            Password: "SecurePassword123"
        );

        _response = await client.PostAsJsonAsync(
            "/api/v1/Auth/sign-in",
            request
        );
    }
    [Then("the authentication request should be successful")]
    public void ThenTheAuthenticationRequestShouldBeSuccessful()
    {
        Assert.IsNotNull(
            _response,
            "The authentication response should exist."
        );

        Assert.AreEqual(
            HttpStatusCode.OK,
            _response.StatusCode,
            "Valid administrator credentials should return 200 OK."
        );
    }
    [Then("the response should contain the role {string}")]
    public async System.Threading.Tasks.Task
        ThenTheResponseShouldContainTheRole(string expectedRole)
    {
        Assert.IsNotNull(
            _response,
            "The authentication response should exist."
        );

        using var json = System.Text.Json.JsonDocument.Parse(
            await _response.Content.ReadAsStringAsync()
        );

        var actualRole = json.RootElement
            .GetProperty("role")
            .GetString();

        Assert.AreEqual(
            expectedRole,
            actualRole,
            "The authenticated user must have the expected role."
        );

        Assert.AreEqual(
            "admin",
            _registeredUser?.Role
        );
    }

    [Then("the response should contain a JWT access token")]
    public async System.Threading.Tasks.Task
        ThenTheResponseShouldContainAJwtAccessToken()
    {
        Assert.IsNotNull(_response);

        using var json = System.Text.Json.JsonDocument.Parse(
            await _response.Content.ReadAsStringAsync()
        );

        var token = json.RootElement
            .GetProperty("token")
            .GetString();

        Assert.IsFalse(
            string.IsNullOrWhiteSpace(token),
            "The JWT token must not be empty."
        );

        var handler = new JwtSecurityTokenHandler();

        var parameters =
            new Microsoft.IdentityModel.Tokens.TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                IssuerSigningKey =
                    new Microsoft.IdentityModel.Tokens.SymmetricSecurityKey(
                        System.Text.Encoding.ASCII.GetBytes(
                            "AutoServiceBddTestSecretKey2026ForHmacSha256"
                        )
                    ),
                ValidateIssuer = true,
                ValidIssuer = "AutoServiceBDD",
                ValidateAudience = true,
                ValidAudience = "AutoServiceBDDClients",
                ValidateLifetime = true,
                ClockSkew = TimeSpan.Zero
            };

        var principal = handler.ValidateToken(
            token,
            parameters,
            out var validatedToken
        );

        Assert.IsInstanceOfType<JwtSecurityToken>(
            validatedToken
        );

        Assert.IsTrue(
            principal.IsInRole("admin"),
            "The JWT must contain the administrator role."
        );

        Assert.AreEqual(
            "WS-01",
            principal.FindFirst("WorkshopId")?.Value
        );
    }


    [Then("the administrator should have access to the administration area")]
    public async System.Threading.Tasks.Task
        ThenTheAdministratorShouldHaveAccessToTheAdministrationArea()
    {
        Assert.IsNotNull(_response);

        using var json = System.Text.Json.JsonDocument.Parse(
            await _response.Content.ReadAsStringAsync()
        );

        var token = json.RootElement
            .GetProperty("token")
            .GetString();

        Assert.IsFalse(string.IsNullOrWhiteSpace(token));

        var client = _app!.GetTestClient();

        using var request = new HttpRequestMessage(
            HttpMethod.Post,
            "/api/v1/Auth/sign-up"
        );
        request.Content = new StringContent(
            "{}",
            System.Text.Encoding.UTF8,
            "application/json"
        );
        request.Headers.Authorization =
            new System.Net.Http.Headers.AuthenticationHeaderValue(
                "Bearer",
                token
            );

        using var adminResponse = await client.SendAsync(request);

        Assert.AreEqual(
            HttpStatusCode.BadRequest,
            adminResponse.StatusCode,
            "The administrator should pass JWT authorization; " +
            "the request should then be rejected because its body is missing."
        );
    }
    [Given("a user with email {string} is registered")]
    public void GivenAUserWithEmailIsRegistered(string email)
    {
        GivenAnAdministratorIsRegistered(email);
    }
    [When("the user signs in with an incorrect password")]
    public async System.Threading.Tasks.Task
        WhenTheUserSignsInWithAnIncorrectPassword()
    {
        var client = await CreateAuthClientAsync();

        var request = new SignInResource(
            Email: "admin@autoservice.com",
            Password: "WrongPassword123"
        );

        _response = await client.PostAsJsonAsync(
            "/api/v1/Auth/sign-in",
            request
        );
    }
    [Then("the authentication request should be rejected")]
    public void ThenTheAuthenticationRequestShouldBeRejected()
    {
        Assert.IsNotNull(
            _response,
            "The authentication response should exist."
        );

        Assert.AreEqual(
            HttpStatusCode.Unauthorized,
            _response.StatusCode,
            "Incorrect credentials must return 401 Unauthorized."
        );
    }
    [Then("the response should not contain a JWT access token")]
    public async System.Threading.Tasks.Task
        ThenTheResponseShouldNotContainAJwtAccessToken()
    {
        Assert.IsNotNull(
            _response,
            "The authentication response should exist."
        );

        using var json = System.Text.Json.JsonDocument.Parse(
            await _response.Content.ReadAsStringAsync()
        );

        Assert.IsFalse(
            json.RootElement.TryGetProperty("token", out _),
            "An unsuccessful authentication must not return a JWT."
        );

        Assert.AreEqual(
            "Email or password is incorrect",
            json.RootElement.GetProperty("message").GetString()
        );
    }

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
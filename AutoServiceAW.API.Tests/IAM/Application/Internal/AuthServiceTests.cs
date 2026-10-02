using AutoServiceAW.API.IAM.Application.Internal;
using AutoServiceAW.API.IAM.Domain.Model.Aggregates;
using AutoServiceAW.API.IAM.Domain.Repositories;
using AutoServiceAW.API.Shared.Domain.Repositories;
using Microsoft.Extensions.Configuration;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.IdentityModel.Tokens.Jwt;
using Moq;

namespace AutoServiceAW.API.Tests.IAM.Application.Internal;

[TestClass]
public class AuthServiceTests
{
    private Mock<IUserRepository> _userRepositoryMock = null!;
    private Mock<IUnitOfWork> _unitOfWorkMock = null!;
    private IConfiguration _configuration = null!;
    private AuthService _authService = null!;

    [TestInitialize]
    public void Setup()
    {
        _userRepositoryMock = new Mock<IUserRepository>();
        _unitOfWorkMock = new Mock<IUnitOfWork>();

        _configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(
                new Dictionary<string, string?>
                {
                    ["Jwt:Secret"] =
                        "AutoServiceTestSecretKey12345678901234567890",
                    ["Jwt:Issuer"] =
                        "AutoServiceTest",
                    ["Jwt:Audience"] =
                        "AutoServiceTestUsers",
                    ["Jwt:ExpirationInMinutes"] =
                        "60"
                }
            )
            .Build();

        _authService = new AuthService(
            _userRepositoryMock.Object,
            _unitOfWorkMock.Object,
            _configuration
        );
    }

    [TestMethod]
    public async Task SignUpAsync_WithValidData_ShouldCreateUserWithHashedPassword()
    {
        // ARRANGE
        var email = "  ADMIN@AUTOSERVICE.COM  ";
        var password = "SecurePassword123";
        var role = "ADMIN";
        var workshopId = "WS-1";

        _userRepositoryMock
            .Setup(repository =>
                repository.FindByEmailAsync(
                    "admin@autoservice.com"
                )
            )
            .ReturnsAsync((User?)null);

        User? capturedUser = null;

        _userRepositoryMock
            .Setup(repository =>
                repository.AddAsync(
                    It.IsAny<User>(),
                    It.IsAny<CancellationToken>()
                )
            )
            .Callback<User, CancellationToken>(
                (user, cancellationToken) =>
                {
                    capturedUser = user;
                }
            )
            .Returns(Task.CompletedTask);

        _unitOfWorkMock
            .Setup(unitOfWork =>
                unitOfWork.CompleteAsync()
            )
            .Returns(Task.CompletedTask);

        // ACT
        var result = await _authService.SignUpAsync(
            email,
            password,
            role,
            workshopId
        );

        // ASSERT
        Assert.IsNotNull(result);
        Assert.IsNotNull(capturedUser);

        Assert.AreEqual(
            "admin@autoservice.com",
            result.Email
        );

        Assert.AreEqual(
            "admin",
            result.Role
        );

        Assert.AreEqual(
            workshopId,
            result.WorkshopId
        );

        Assert.AreNotEqual(
            password,
            result.PasswordHash
        );

        Assert.IsTrue(
            BCrypt.Net.BCrypt.Verify(
                password,
                result.PasswordHash
            )
        );

        _userRepositoryMock.Verify(
            repository =>
                repository.FindByEmailAsync(
                    "admin@autoservice.com"
                ),
            Times.Once
        );

        _userRepositoryMock.Verify(
            repository =>
                repository.AddAsync(
                    It.Is<User>(user =>
                        user.Email == "admin@autoservice.com" &&
                        user.Role == "admin" &&
                        user.WorkshopId == workshopId
                    ),
                    It.IsAny<CancellationToken>()
                ),
            Times.Once
        );

        _unitOfWorkMock.Verify(
            unitOfWork =>
                unitOfWork.CompleteAsync(),
            Times.Once
        );
    }
    [TestMethod]
public async Task SignInAsync_WithValidCredentials_ShouldReturnUserAndJwtToken()
{
    // ARRANGE
    var email = "mechanic@autoservice.com";
    var password = "MechanicPassword123";
    var workshopId = "WS-1";

    var passwordHash =
        BCrypt.Net.BCrypt.HashPassword(password);

    var existingUser = new User(
        email,
        passwordHash,
        "mechanic",
        workshopId
    );

    _userRepositoryMock
        .Setup(repository =>
            repository.FindByEmailAsync(email)
        )
        .ReturnsAsync(existingUser);

    // ACT
    var result = await _authService.SignInAsync(
        email,
        password
    );

    // ASSERT
    Assert.IsNotNull(result);

    Assert.AreEqual(
        email,
        result.Value.User.Email
    );

    Assert.AreEqual(
        "mechanic",
        result.Value.User.Role
    );

    Assert.AreEqual(
        workshopId,
        result.Value.User.WorkshopId
    );

    Assert.IsFalse(
        string.IsNullOrWhiteSpace(
            result.Value.Token
        )
    );

    var tokenHandler =
        new JwtSecurityTokenHandler();

    var jwtToken =
        tokenHandler.ReadJwtToken(
            result.Value.Token
        );

    Assert.AreEqual(
        "AutoServiceTest",
        jwtToken.Issuer
    );

    Assert.IsTrue(
        jwtToken.Audiences.Contains(
            "AutoServiceTestUsers"
        )
    );

    Assert.IsTrue(
        jwtToken.Claims.Any(claim =>
            claim.Type == "email" &&
            claim.Value == email
        )
    );

    Assert.IsTrue(
        jwtToken.Claims.Any(claim =>
            claim.Type == "role" &&
            claim.Value == "mechanic"
        )
    );

    Assert.IsTrue(
        jwtToken.Claims.Any(claim =>
            claim.Type == "WorkshopId" &&
            claim.Value == workshopId
        )
    );

    _userRepositoryMock.Verify(
        repository =>
            repository.FindByEmailAsync(email),
        Times.Once
    );

    _userRepositoryMock.Verify(
        repository =>
            repository.AddAsync(
                It.IsAny<User>(),
                It.IsAny<CancellationToken>()
            ),
        Times.Never
    );

    _unitOfWorkMock.Verify(
        unitOfWork =>
            unitOfWork.CompleteAsync(),
        Times.Never
    );
}
}
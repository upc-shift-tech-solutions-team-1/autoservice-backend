using AutoServiceAW.API.IAM.Domain.Model.Aggregates;

namespace AutoServiceAW.Tests.Unit;

public class UserTests
{
    [Fact]
    public void Constructor_ShouldCreateUserWithProvidedData()
    {
        // Arrange & Act
        var user = new User(
            "admin@autoservice.com",
            "hashed-password",
            "ADMIN",
            "WORKSHOP-001");

        // Assert
        Assert.Equal("admin@autoservice.com", user.Email);
        Assert.Equal("hashed-password", user.PasswordHash);
        Assert.Equal("ADMIN", user.Role);
        Assert.Equal("WORKSHOP-001", user.WorkshopId);
    }

    [Fact]
    public void UpdatePassword_ShouldChangePasswordHash()
    {
        // Arrange
        var user = new User(
            "admin@autoservice.com",
            "old-hash",
            "ADMIN",
            "WORKSHOP-001");

        // Act
        user.UpdatePassword("new-hash");

        // Assert
        Assert.Equal("new-hash", user.PasswordHash);
    }
}
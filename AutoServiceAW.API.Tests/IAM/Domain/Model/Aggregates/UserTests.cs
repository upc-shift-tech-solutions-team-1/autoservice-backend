using AutoServiceAW.API.IAM.Domain.Model.Aggregates;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace AutoServiceAW.API.Tests.IAM.Domain.Model.Aggregates;

[TestClass]
public class UserTests
{
    [TestMethod]
    public void Constructor_ShouldCreateUserWithProvidedData()
    {
        // Arrange
        const string email = "admin@autoservice.com";
        const string passwordHash = "hashed-password";
        const string role = "ADMIN";
        const string workshopId = "WORKSHOP-001";

        // Act
        var user = new User(
            email,
            passwordHash,
            role,
            workshopId);

        // Assert
        Assert.AreEqual(email, user.Email);
        Assert.AreEqual(passwordHash, user.PasswordHash);
        Assert.AreEqual(role, user.Role);
        Assert.AreEqual(workshopId, user.WorkshopId);
    }

    [TestMethod]
    public void UpdatePassword_ShouldChangePasswordHashWithoutModifyingIdentity()
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
        Assert.AreEqual("new-hash", user.PasswordHash);
        Assert.AreEqual("admin@autoservice.com", user.Email);
        Assert.AreEqual("ADMIN", user.Role);
        Assert.AreEqual("WORKSHOP-001", user.WorkshopId);
    }
}
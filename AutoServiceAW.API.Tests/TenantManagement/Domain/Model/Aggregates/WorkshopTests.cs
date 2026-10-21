using System.Text.RegularExpressions;
using AutoServiceAW.API.TenantManagement.Domain.Model.Aggregates;

namespace AutoServiceAW.API.Tests.TenantManagement.Domain.Model.Aggregates;

[TestClass]
public class WorkshopTests
{
    [TestMethod]
    public void Constructor_WithValidName_ShouldGenerateTenantIdentifier()
    {
        // Arrange
        const string workshopName = "AutoService Central";

        // Act
        var workshop = new Workshop(workshopName);

        // Assert
        Assert.AreEqual(workshopName, workshop.Name);
        Assert.IsFalse(string.IsNullOrWhiteSpace(workshop.TenantId));

        StringAssert.Matches(
            workshop.TenantId,
            new Regex(@"^WS-[A-Z0-9]{4}$"));
    }
}
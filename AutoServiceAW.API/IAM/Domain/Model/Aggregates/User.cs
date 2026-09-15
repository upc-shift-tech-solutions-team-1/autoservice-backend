using System.Text.Json.Serialization;

namespace AutoServiceAW.API.IAM.Domain.Model.Aggregates;

/// <summary>
/// Represents a User aggregate root within the Identity and Access Management (IAM) domain.
/// </summary>
public class User
{
    #region Properties

    /// <summary>
    /// Gets the unique identifier for the user.
    /// </summary>
    public int Id { get; private set; }

    /// <summary>
    /// Gets the unique email address of the user.
    /// </summary>
    public string Email { get; private set; }
    
    /// <summary>
    /// Gets the securely hashed password of the user.
    /// This property is excluded from JSON serialization.
    /// </summary>
    [JsonIgnore]
    public string PasswordHash { get; private set; }
    
    /// <summary>
    /// Gets the authorization role assigned to the user (e.g., Admin, Technician).
    /// </summary>
    public string Role { get; private set; }

    /// <summary>
    /// Gets the identifier of the workshop the user belongs to.
    /// </summary>
    public string WorkshopId { get; private set; }

    #endregion

    #region Constructors

    /// <summary>
    /// Initializes a new instance of the <see cref="User"/> class with authentication details and workshop context.
    /// </summary>
    /// <param name="email">The unique email address.</param>
    /// <param name="passwordHash">The pre-hashed password string.</param>
    /// <param name="role">The access level role.</param>
    /// <param name="workshopId">The linked workshop identifier.</param>
    public User(string email, string passwordHash, string role, string workshopId)
    {
        Email = email;
        PasswordHash = passwordHash;
        Role = role;
        WorkshopId = workshopId;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="User"/> class with default values.
    /// Required by ORMs such as Entity Framework Core for data mapping.
    /// </summary>
    protected User() { }

    #endregion

    #region Methods

    /// <summary>
    /// Securely updates the user's password hash state.
    /// </summary>
    /// <param name="newPasswordHash">The new pre-hashed password string.</param>
    public void UpdatePassword(string newPasswordHash)
    {
        PasswordHash = newPasswordHash;
    }

    #endregion
}
namespace AutoServiceAW.API.IAM.Domain.Model.ValueObjects;

/// <summary>
/// Defines the supported authorization roles for AutoService users.
/// </summary>
public static class UserRole
{
    public const string Admin = "admin";
    public const string Mechanic = "mechanic";

    /// <summary>
    /// Normalizes and validates a role before it is persisted.
    /// </summary>
    /// <param name="role">The incoming role value.</param>
    /// <returns>The normalized supported role.</returns>
    /// <exception cref="ArgumentException">
    /// Thrown when the role is empty or unsupported.
    /// </exception>
    public static string NormalizeAndValidate(string role)
    {
        if (string.IsNullOrWhiteSpace(role))
            throw new ArgumentException("Role is required.");

        var normalizedRole = role.Trim().ToLowerInvariant();

        if (normalizedRole != Admin &&
            normalizedRole != Mechanic)
        {
            throw new ArgumentException(
                $"Unsupported role '{role}'."
            );
        }

        return normalizedRole;
    }
}

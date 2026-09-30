using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using AutoServiceAW.API.IAM.Domain.Model.Aggregates;
using AutoServiceAW.API.IAM.Domain.Model.ValueObjects;
using AutoServiceAW.API.IAM.Domain.Repositories;
using AutoServiceAW.API.IAM.Domain.Services;
using AutoServiceAW.API.Shared.Domain.Repositories;
using Microsoft.IdentityModel.Tokens;

namespace AutoServiceAW.API.IAM.Application.Internal;

/// <summary>
/// Provides application services for Identity and Access Management (IAM),
/// handling user authentication, registration, and token generation.
/// </summary>
public class AuthService(
    IUserRepository userRepository,
    IUnitOfWork unitOfWork,
    IConfiguration configuration
) : IAuthService
{
    #region Methods

    /// <summary>
    /// Registers a new user in the system with a hashed password
    /// and a validated authorization role.
    /// </summary>
    /// <param name="email">
    /// The unique email address of the user.
    /// </param>
    /// <param name="password">
    /// The plain-text password to be securely hashed.
    /// </param>
    /// <param name="role">
    /// The authorization role assigned to the user.
    /// </param>
    /// <param name="workshopId">
    /// The workshop identifier associated with the user.
    /// </param>
    /// <returns>
    /// The created user entity.
    /// </returns>
    /// <exception cref="ArgumentException">
    /// Thrown when required registration information or the role is invalid.
    /// </exception>
    /// <exception cref="InvalidOperationException">
    /// Thrown when the email is already registered.
    /// </exception>
    public async Task<User?> SignUpAsync(
        string email,
        string password,
        string role,
        string workshopId
    )
    {
        if (string.IsNullOrWhiteSpace(email))
            throw new ArgumentException(
                "Email is required.",
                nameof(email)
            );

        if (string.IsNullOrWhiteSpace(password))
            throw new ArgumentException(
                "Password is required.",
                nameof(password)
            );

        if (string.IsNullOrWhiteSpace(workshopId))
            throw new ArgumentException(
                "Workshop ID is required.",
                nameof(workshopId)
            );

        var normalizedEmail =
            email.Trim().ToLowerInvariant();

        var normalizedRole =
            UserRole.NormalizeAndValidate(role);

        var existingUser =
            await userRepository.FindByEmailAsync(
                normalizedEmail
            );

        if (existingUser != null)
        {
            throw new InvalidOperationException(
                "Email is already taken."
            );
        }

        var passwordHash =
            BCrypt.Net.BCrypt.HashPassword(password);

        var user = new User(
            normalizedEmail,
            passwordHash,
            normalizedRole,
            workshopId.Trim()
        );

        await userRepository.AddAsync(user);

        await unitOfWork.CompleteAsync();

        return user;
    }

    /// <summary>
    /// Authenticates an existing user using their email and password.
    /// </summary>
    /// <param name="email">
    /// The registered email address.
    /// </param>
    /// <param name="password">
    /// The plain-text password to verify.
    /// </param>
    /// <returns>
    /// The authenticated user and generated JWT token,
    /// or null when authentication fails.
    /// </returns>
    public async Task<(User User, string Token)?> SignInAsync(
        string email,
        string password
    )
    {
        if (string.IsNullOrWhiteSpace(email) ||
            string.IsNullOrWhiteSpace(password))
        {
            return null;
        }

        var normalizedEmail =
            email.Trim().ToLowerInvariant();

        var user =
            await userRepository.FindByEmailAsync(
                normalizedEmail
            );

        if (user == null)
            return null;

        var validPassword =
            BCrypt.Net.BCrypt.Verify(
                password,
                user.PasswordHash
            );

        if (!validPassword)
            return null;

        var token = GenerateJwtToken(user);

        return (user, token);
    }

    /// <summary>
    /// Generates a JSON Web Token containing the authenticated
    /// user's identity, authorization role, and workshop context.
    /// </summary>
    /// <param name="user">
    /// The authenticated user.
    /// </param>
    /// <returns>
    /// A signed JWT token.
    /// </returns>
    private string GenerateJwtToken(User user)
    {
        var jwtSettings =
            configuration.GetSection("Jwt");

        var secret =
            jwtSettings["Secret"]
            ?? throw new InvalidOperationException(
                "JWT Secret configuration is missing."
            );

        var issuer =
            jwtSettings["Issuer"]
            ?? throw new InvalidOperationException(
                "JWT Issuer configuration is missing."
            );

        var audience =
            jwtSettings["Audience"]
            ?? throw new InvalidOperationException(
                "JWT Audience configuration is missing."
            );

        var expirationSetting =
            jwtSettings["ExpirationInMinutes"]
            ?? throw new InvalidOperationException(
                "JWT expiration configuration is missing."
            );

        if (!double.TryParse(
                expirationSetting,
                out var expirationInMinutes
            ))
        {
            throw new InvalidOperationException(
                "JWT expiration configuration is invalid."
            );
        }

        var key =
            Encoding.ASCII.GetBytes(secret);

        var claims = new[]
        {
            new Claim(
                ClaimTypes.NameIdentifier,
                user.Id.ToString()
            ),

            new Claim(
                ClaimTypes.Email,
                user.Email
            ),

            new Claim(
                ClaimTypes.Role,
                user.Role
            ),

            new Claim(
                "WorkshopId",
                user.WorkshopId
            )
        };

        var tokenDescriptor =
            new SecurityTokenDescriptor
            {
                Subject =
                    new ClaimsIdentity(claims),

                Expires =
                    DateTime.UtcNow.AddMinutes(
                        expirationInMinutes
                    ),

                SigningCredentials =
                    new SigningCredentials(
                        new SymmetricSecurityKey(key),
                        SecurityAlgorithms
                            .HmacSha256Signature
                    ),

                Issuer = issuer,
                Audience = audience
            };

        var tokenHandler =
            new JwtSecurityTokenHandler();

        var token =
            tokenHandler.CreateToken(
                tokenDescriptor
            );

        return tokenHandler.WriteToken(token);
    }

    #endregion
}
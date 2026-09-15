using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using AutoServiceAW.API.IAM.Domain.Model.Aggregates;
using AutoServiceAW.API.IAM.Domain.Repositories;
using AutoServiceAW.API.IAM.Domain.Services;
using AutoServiceAW.API.Shared.Domain.Repositories;
using Microsoft.IdentityModel.Tokens;

namespace AutoServiceAW.API.IAM.Application.Internal;

/// <summary>
/// Provides application services for Identity and Access Management (IAM), 
/// handling user authentication, registration, and token generation.
/// </summary>
public class AuthService(IUserRepository userRepository, IUnitOfWork unitOfWork, IConfiguration configuration) : IAuthService
{
    #region Methods

    /// <summary>
    /// Registers a new user in the system with a hashed password.
    /// </summary>
    /// <param name="email">The unique email address of the user.</param>
    /// <param name="password">The plain-text password to be hashed.</param>
    /// <param name="role">The assigned role for authorization access levels.</param>
    /// <param name="workshopId">The identifier of the workshop the user belongs to.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains the created <see cref="User"/> entity.</returns>
    /// <exception cref="Exception">Thrown when the provided email is already registered in the system.</exception>
    public async Task<User?> SignUpAsync(string email, string password, string role, string workshopId)
    {
        if (await userRepository.FindByEmailAsync(email) != null)
            throw new Exception("Email is already taken.");

        var passwordHash = BCrypt.Net.BCrypt.HashPassword(password);
        var user = new User(email, passwordHash, role, workshopId);

        await userRepository.AddAsync(user);
        await unitOfWork.CompleteAsync();
        return user;
    }

    /// <summary>
    /// Authenticates a user based on their credentials and generates an access token.
    /// </summary>
    /// <param name="email">The registered email address of the user.</param>
    /// <param name="password">The plain-text password to verify.</param>
    /// <returns>
    /// A task that represents the asynchronous operation. The task result contains a tuple with the 
    /// authenticated <see cref="User"/> and their generated JWT token string, or <see langword="null"/> if credentials are invalid.
    /// </returns>
    public async Task<(User User, string Token)?> SignInAsync(string email, string password)
    {
        var user = await userRepository.FindByEmailAsync(email);
        
        if (user == null || !BCrypt.Net.BCrypt.Verify(password, user.PasswordHash))
            return null;

        var token = GenerateJwtToken(user);
        return (user, token);
    }

    /// <summary>
    /// Generates a JSON Web Token (JWT) string encrypted with HMAC-SHA256 containing user claims.
    /// </summary>
    /// <param name="user">The authenticated user entity whose information will be embedded into claims.</param>
    /// <returns>A signed JWT token string.</returns>
    private string GenerateJwtToken(User user)
    {
        var jwtSettings = configuration.GetSection("Jwt");
        var key = Encoding.ASCII.GetBytes(jwtSettings["Secret"]!);

        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new Claim(ClaimTypes.Email, user.Email),
            new Claim(ClaimTypes.Role, user.Role),
            new Claim("WorkshopId", user.WorkshopId)
        };

        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(claims),
            Expires = DateTime.UtcNow.AddMinutes(double.Parse(jwtSettings["ExpirationInMinutes"]!)),
            SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature),
            Issuer = jwtSettings["Issuer"],
            Audience = jwtSettings["Audience"]
        };

        var tokenHandler = new JwtSecurityTokenHandler();
        var token = tokenHandler.CreateToken(tokenDescriptor);
        return tokenHandler.WriteToken(token);
    }

    #endregion
}
using System.Security.Claims;
using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace AutoServiceAW.API.Tests.WorkshopOperations;

internal sealed class TestAuthenticationHandler(
    IOptionsMonitor<AuthenticationSchemeOptions> options,
    ILoggerFactory logger,
    UrlEncoder encoder
) : AuthenticationHandler<AuthenticationSchemeOptions>(
    options,
    logger,
    encoder
)
{
    protected override System.Threading.Tasks.Task<AuthenticateResult>
        HandleAuthenticateAsync()
    {
        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, "test-admin"),
            new Claim(ClaimTypes.Role, "admin"),
            new Claim("WorkshopId", "WS-01")
        };

        var identity = new ClaimsIdentity(
            claims,
            Scheme.Name
        );

        var principal = new ClaimsPrincipal(identity);
        var ticket = new AuthenticationTicket(
            principal,
            Scheme.Name
        );

        return System.Threading.Tasks.Task.FromResult(
            AuthenticateResult.Success(ticket)
        );
    }
}

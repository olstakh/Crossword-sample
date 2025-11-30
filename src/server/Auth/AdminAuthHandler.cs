using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Options;
using System.Security.Claims;
using System.Text.Encodings.Web;

namespace CrossWords.Auth;

/// <summary>
/// Simple API key authentication handler for admin endpoints in development
/// In production, this would be replaced with Azure AD or Google OAuth
/// </summary>
public class AdminAuthHandler : AuthenticationHandler<AuthenticationSchemeOptions>
{
    private readonly IConfiguration _configuration;
    private readonly ILogger<AdminAuthHandler> _logger;

    public AdminAuthHandler(
        IOptionsMonitor<AuthenticationSchemeOptions> options,
        ILoggerFactory logger,
        UrlEncoder encoder,
        IConfiguration configuration)
        : base(options, logger, encoder)
    {
        _configuration = configuration;
        _logger = logger.CreateLogger<AdminAuthHandler>();
    }

    protected override Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        // In development, allow access without auth if configured
        var environment = _configuration["ASPNETCORE_ENVIRONMENT"] ?? "Development";
        var bypassAuth = _configuration.GetValue<bool>("Auth:BypassInDevelopment", true);
        
        if (environment == "Development" && bypassAuth)
        {
            _logger.LogDebug("Development mode: bypassing admin authentication");
            var identity = new ClaimsIdentity(new[] 
            { 
                new Claim(ClaimTypes.Name, "dev-admin"),
                new Claim(ClaimTypes.Role, "Admin")
            }, Scheme.Name);
            
            var principal = new ClaimsPrincipal(identity);
            var ticket = new AuthenticationTicket(principal, Scheme.Name);
            return Task.FromResult(AuthenticateResult.Success(ticket));
        }

        // Check for API key in header
        if (!Request.Headers.TryGetValue("X-Admin-Key", out var apiKeyHeader))
        {
            _logger.LogWarning("Admin authentication failed: No X-Admin-Key header");
            return Task.FromResult(AuthenticateResult.Fail("Missing X-Admin-Key header"));
        }

        var apiKey = apiKeyHeader.ToString();
        var validApiKey = _configuration["Auth:AdminApiKey"];

        if (string.IsNullOrWhiteSpace(validApiKey))
        {
            _logger.LogError("Admin API key not configured in appsettings.json");
            return Task.FromResult(AuthenticateResult.Fail("Admin authentication not configured"));
        }

        if (apiKey != validApiKey)
        {
            _logger.LogWarning("Admin authentication failed: Invalid API key");
            return Task.FromResult(AuthenticateResult.Fail("Invalid API key"));
        }

        // Success - create claims identity
        var authIdentity = new ClaimsIdentity(new[] 
        { 
            new Claim(ClaimTypes.Name, "admin"),
            new Claim(ClaimTypes.Role, "Admin")
        }, Scheme.Name);
        
        var authPrincipal = new ClaimsPrincipal(authIdentity);
        var authTicket = new AuthenticationTicket(authPrincipal, Scheme.Name);
        
        _logger.LogInformation("Admin authenticated successfully");
        return Task.FromResult(AuthenticateResult.Success(authTicket));
    }
}

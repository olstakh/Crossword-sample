# Authentication Setup Guide

## Overview

The application uses a flexible authentication system that adapts to the environment:
- **Development**: Optional bypass or simple API key
- **Production**: Azure AD (Entra ID), Google OAuth, or API key

## Development Setup (Default - No Auth)

By default, admin endpoints are accessible without authentication in Development mode.

### Configuration (appsettings.Development.json)

```json
{
  "Auth": {
    "BypassInDevelopment": true
  }
}
```

## Development with API Key (Optional)

To test with authentication locally:

### 1. Update appsettings.Development.json

```json
{
  "Auth": {
    "BypassInDevelopment": false,
    "AdminApiKey": "your-secret-dev-key-12345"
  }
}
```

### 2. Add header to requests

```http
X-Admin-Key: your-secret-dev-key-12345
```

## Production Setup

### Option 1: API Key (Simple)

**appsettings.Production.json:**
```json
{
  "Auth": {
    "BypassInDevelopment": false,
    "AdminApiKey": "production-secret-key-from-env-variable"
  }
}
```

**Set via environment variable:**
```bash
export Auth__AdminApiKey="your-production-secret-key"
```

**Docker:**
```yaml
environment:
  - Auth__AdminApiKey=your-production-secret-key
```

### Option 2: Azure AD (Recommended for Azure deployments)

**Install package:**
```bash
dotnet add package Microsoft.Identity.Web
```

**appsettings.Production.json:**
```json
{
  "AzureAd": {
    "Instance": "https://login.microsoftonline.com/",
    "TenantId": "your-tenant-id",
    "ClientId": "your-client-id",
    "Audience": "api://your-api-id"
  },
  "Auth": {
    "UseAzureAd": true
  }
}
```

**Update Program.cs:**
```csharp
// Add Azure AD authentication
if (builder.Configuration.GetValue<bool>("Auth:UseAzureAd"))
{
    builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
        .AddMicrosoftIdentityWebApi(builder.Configuration.GetSection("AzureAd"));
}
```

### Option 3: Google OAuth

**Install package:**
```bash
dotnet add package Microsoft.AspNetCore.Authentication.Google
```

**appsettings.Production.json:**
```json
{
  "Authentication": {
    "Google": {
      "ClientId": "your-google-client-id",
      "ClientSecret": "your-google-client-secret"
    }
  },
  "Auth": {
    "UseGoogle": true
  }
}
```

**Update Program.cs:**
```csharp
if (builder.Configuration.GetValue<bool>("Auth:UseGoogle"))
{
    builder.Services.AddAuthentication()
        .AddGoogle(options =>
        {
            options.ClientId = builder.Configuration["Authentication:Google:ClientId"];
            options.ClientSecret = builder.Configuration["Authentication:Google:ClientSecret"];
        });
}
```

## Protected Endpoints

The following endpoints require admin authentication:

### AdminController
- All endpoints (`POST /api/admin/puzzles`, `DELETE /api/admin/puzzles/{id}`, etc.)

### UserController
- `GET /api/user/all` - List all users

### Future Endpoints
- Any endpoint decorated with `[Authorize(Policy = "AdminOnly")]`

## Testing Protected Endpoints

### Without Auth (Development default)
```bash
curl http://localhost:5000/api/user/all
```

### With API Key
```bash
curl -H "X-Admin-Key: your-secret-key" http://localhost:5000/api/user/all
```

### With Azure AD Token
```bash
curl -H "Authorization: Bearer {jwt-token}" http://localhost:5000/api/user/all
```

## Environment-Specific Behavior

| Environment | Default Auth | Override |
|------------|--------------|----------|
| Development | Bypassed | Set `BypassInDevelopment: false` |
| Testing | Bypassed | Always bypassed for unit tests |
| Production | Required | Must configure auth |

## Security Best Practices

1. **Never commit secrets** to source control
2. **Use environment variables** for production keys
3. **Rotate API keys** regularly
4. **Use Azure KeyVault** or similar for secret management
5. **Enable HTTPS** in production (handled by reverse proxy)
6. **Consider rate limiting** for admin endpoints

## Troubleshooting

### "Missing X-Admin-Key header" in Development
- Check `Auth:BypassInDevelopment` is `true` in appsettings.Development.json

### "Invalid API key" in Production
- Verify environment variable is set correctly
- Check configuration binding: `Auth__AdminApiKey` (double underscore)

### 401 Unauthorized in Production
- Ensure authentication is configured
- Check headers include correct auth token/key
- Verify user has "Admin" role

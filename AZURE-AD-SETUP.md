# Azure AD Authentication Setup

## Overview

Your application now supports **two authentication modes**:

1. **AdminScheme** (Custom API Key) - Simple auth for local development
2. **Azure AD** (JWT Bearer) - Enterprise authentication for production

The app automatically selects Azure AD if `TenantId` and `ClientId` are configured, otherwise falls back to AdminScheme.

---

## Step 1: Azure Portal Configuration (Completed ✓)

You've already completed these steps:

1. ✓ Created app registration
2. ✓ Exposed API with `access_as_admin` scope
3. ✓ Created `Admin` app role
4. ✓ Assigned users to Admin role
5. ✓ Configured redirect URIs

---

## Step 2: Update Configuration Files

### Get Your Azure AD Values

From the Azure Portal app registration overview page:

- **Application (client) ID**: e.g., `12345678-1234-1234-1234-123456789abc`
- **Directory (tenant) ID**: e.g., `87654321-4321-4321-4321-cba987654321`
- **Audience**: Same as your Client ID (or use `api://{ClientId}`)

### Update appsettings.Production.json

```json
{
  "AzureAd": {
    "Instance": "https://login.microsoftonline.com/",
    "TenantId": "YOUR_TENANT_ID_HERE",
    "ClientId": "YOUR_CLIENT_ID_HERE",
    "Audience": "YOUR_CLIENT_ID_HERE"
  }
}
```

### Environment Variables (Recommended for Production)

```bash
# Windows PowerShell
$env:AzureAd__TenantId = "YOUR_TENANT_ID"
$env:AzureAd__ClientId = "YOUR_CLIENT_ID"
$env:AzureAd__Audience = "YOUR_CLIENT_ID"

# Linux/macOS
export AzureAd__TenantId="YOUR_TENANT_ID"
export AzureAd__ClientId="YOUR_CLIENT_ID"
export AzureAd__Audience="YOUR_CLIENT_ID"
```

---

## Step 3: Testing Azure AD Authentication

### Option A: Using Postman

1. **Get Access Token from Azure AD**:
   - Open a browser to: `https://login.microsoftonline.com/{TenantId}/oauth2/v2.0/authorize?client_id={ClientId}&response_type=token&redirect_uri=https://localhost:5001&scope=api://{ClientId}/access_as_admin&response_mode=fragment`
   - Sign in with a user that has the **Admin** role
   - Copy the access token from the redirect URL (after `#access_token=`)

2. **Test Protected Endpoint**:
   ```http
   GET https://localhost:5001/api/admin/puzzles
   Authorization: Bearer YOUR_ACCESS_TOKEN_HERE
   ```

### Option B: Using PowerShell Script

```powershell
# Get access token
$tenantId = "YOUR_TENANT_ID"
$clientId = "YOUR_CLIENT_ID"
$scope = "api://$clientId/access_as_admin"

# This will open browser for interactive login
$token = # (Use browser method above, or configure client secret for app-only flow)

# Test API
$headers = @{
    "Authorization" = "Bearer $token"
}
Invoke-RestMethod -Uri "https://localhost:5001/api/admin/puzzles" -Headers $headers
```

### Option C: Test Without Azure AD (Development Mode)

In Development environment, Azure AD is not required (falls back to AdminScheme with bypass):

```powershell
# Should work without any authentication
Invoke-RestMethod -Uri "https://localhost:5001/api/admin/puzzles"
```

---

## Step 4: Update Client Pages for Azure AD

If you want your admin pages (`admin.html`, `delete.html`, `users.html`) to work with Azure AD in production:

### Install MSAL.js (Microsoft Authentication Library)

Add to your HTML files:

```html
<script src="https://alcdn.msauth.net/browser/2.38.0/js/msal-browser.min.js"></script>
```

### Create `azure-auth.js`

```javascript
const msalConfig = {
    auth: {
        clientId: "YOUR_CLIENT_ID",
        authority: "https://login.microsoftonline.com/YOUR_TENANT_ID",
        redirectUri: window.location.origin
    }
};

const loginRequest = {
    scopes: ["api://YOUR_CLIENT_ID/access_as_admin"]
};

const msalInstance = new msal.PublicClientApplication(msalConfig);

async function getAccessToken() {
    try {
        const account = msalInstance.getAllAccounts()[0];
        if (!account) {
            await msalInstance.loginPopup(loginRequest);
        }
        
        const response = await msalInstance.acquireTokenSilent({
            ...loginRequest,
            account: msalInstance.getAllAccounts()[0]
        });
        
        return response.accessToken;
    } catch (error) {
        console.error("Auth error:", error);
        const response = await msalInstance.loginPopup(loginRequest);
        return response.accessToken;
    }
}

// Use in your API calls
async function fetchProtectedData() {
    const token = await getAccessToken();
    const response = await fetch('/api/admin/puzzles', {
        headers: {
            'Authorization': `Bearer ${token}`
        }
    });
    return response.json();
}
```

---

## Authentication Flow Summary

### Development Environment
- Azure AD config empty → Uses **AdminScheme**
- `BypassInDevelopment: true` → **No authentication required**
- All admin endpoints accessible without credentials

### Production Environment
- Azure AD config set → Uses **Azure AD (JWT Bearer)**
- Validates JWT tokens from Microsoft Identity Platform
- Requires users to have **Admin** role in Azure AD
- Tokens contain user claims and role information

### Production Fallback (No Azure AD)
- Azure AD config empty → Uses **AdminScheme**
- `BypassInDevelopment: false` → **X-Admin-Key header required**
- Simple API key authentication

---

## Troubleshooting

### "Unauthorized" in Production with Azure AD

1. **Check token validity**:
   - Decode JWT at https://jwt.ms
   - Verify `aud` (audience) matches your ClientId
   - Verify `roles` array contains "Admin"
   - Check `exp` (expiration) hasn't passed

2. **Check app configuration**:
   - TenantId and ClientId are correct
   - User is assigned Admin role in Azure Portal
   - Token scope matches `api://{ClientId}/access_as_admin`

3. **Check logs**:
   ```powershell
   dotnet run --environment Production
   ```
   Look for authentication-related errors

### Azure AD Config Not Taking Effect

- Restart the application after changing `appsettings.Production.json`
- Verify environment is set to Production: `$env:ASPNETCORE_ENVIRONMENT = "Production"`
- Check configuration loaded: Add logging to Program.cs

### Token Missing Admin Role

In Azure Portal:
1. Go to **App Roles** → Verify "Admin" role exists
2. Go to **Enterprise Applications** → Find your app → **Users and groups**
3. Assign user to "Admin" role

---

## Security Best Practices

### Production Deployment

1. **Never commit secrets** to source control:
   - Keep `TenantId` and `ClientId` in environment variables or Azure Key Vault
   - Use User Secrets for local development: `dotnet user-secrets set "AzureAd:ClientId" "your-value"`

2. **Use HTTPS only** in production:
   - Azure AD requires HTTPS for redirect URIs
   - Configure proper SSL certificates

3. **Restrict CORS** in production:
   - Update `Program.cs` to allow only specific origins
   - Don't use `AllowAnyOrigin()` in production

4. **Enable logging and monitoring**:
   - Configure Application Insights
   - Monitor failed authentication attempts
   - Set up alerts for suspicious activity

---

## Next Steps

1. **Fill in Azure AD configuration** in `appsettings.Production.json` with values from Azure Portal
2. **Test locally** with `ASPNETCORE_ENVIRONMENT=Production` and Azure AD token
3. **Update client JavaScript** to use MSAL.js for token acquisition
4. **Deploy to production** environment (Azure App Service, Docker, etc.)
5. **Monitor authentication** logs and user access patterns

---

## Reference Links

- [Microsoft Identity Platform Documentation](https://learn.microsoft.com/en-us/azure/active-directory/develop/)
- [Microsoft.Identity.Web Documentation](https://learn.microsoft.com/en-us/azure/active-directory/develop/microsoft-identity-web)
- [MSAL.js Browser Documentation](https://learn.microsoft.com/en-us/azure/active-directory/develop/msal-js-initializing-client-applications)
- [Azure AD App Roles](https://learn.microsoft.com/en-us/azure/active-directory/develop/howto-add-app-roles-in-azure-ad-apps)

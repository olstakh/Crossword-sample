# Testing Azure AD Authentication

## Quick Test Guide

Your client is now configured to **automatically handle authentication** based on server configuration. Here's how it works:

### How It Works

1. **Client makes API call** (e.g., load users list)
2. **Server responds**:
   - ✅ **200 OK** → Everything works, no auth needed (Development mode)
   - ❌ **401 Unauthorized** → Client automatically triggers Azure AD login
3. **If 401**: Client opens Azure AD popup, user signs in, token acquired
4. **Client retries request** with `Authorization: Bearer {token}` header
5. **Server validates token** and responds with data

### Testing Scenarios

#### Scenario 1: Development Mode (Default)
**Configuration**: `appsettings.json` has empty `TenantId` and `ClientId`

**Expected Behavior**:
- Navigate to `http://localhost:5000/users.html`
- Page loads immediately, shows user list
- **No authentication popup** (bypass is enabled)

**Test**:
```powershell
dotnet run
# Open browser: http://localhost:5000/users.html
# Should work without login
```

---

#### Scenario 2: Production with Azure AD
**Configuration**: `appsettings.Production.json` has your Azure AD values filled in

**Expected Behavior**:
1. Navigate to `http://localhost:5000/users.html`
2. Page attempts to load users
3. Server returns **401 Unauthorized**
4. **Azure AD popup appears** automatically
5. Sign in with account that has **Admin role**
6. Popup closes, token acquired
7. Page **automatically retries** and shows user list

**Test**:
```powershell
$env:ASPNETCORE_ENVIRONMENT = "Production"
dotnet run
# Open browser: http://localhost:5000/users.html
# Azure AD login popup should appear
```

---

#### Scenario 3: Production Without Azure AD (API Key)
**Configuration**: `appsettings.Production.json` has empty Azure AD, `AdminApiKey` set

**Expected Behavior**:
- API calls will fail with 401
- No popup (since Azure AD not configured)
- Need to add `X-Admin-Key` header manually (not yet implemented in client)

---

## Testing Each Admin Page

### 1. Manage Users (`users.html`)
- **Test**: Load page → Should show all users
- **API Call**: `GET /api/user/all`
- **Auth**: Required in Production

### 2. User Detail (`user-detail.html?userId=test`)
- **Test**: Click on user → Should show solved puzzles
- **API Calls**:
  - `GET /api/user/progress` (with X-User-Id header)
  - `POST /api/user/forget` (when forgetting puzzles)
- **Auth**: Required in Production

### 3. Create Puzzle (`admin.html`)
- **Test**: Create and save a puzzle
- **API Calls**:
  - `POST /api/admin/puzzles`
  - `GET /api/admin/puzzles` (download database)
  - `POST /api/admin/puzzles/upload-bulk` (upload puzzles)
- **Auth**: Required in Production

### 4. Delete Puzzles (`delete.html`)
- **Test**: View puzzles, delete one
- **API Calls**:
  - `GET /api/admin/puzzles`
  - `DELETE /api/admin/puzzles/{id}`
  - `POST /api/admin/puzzles/delete-bulk`
- **Auth**: Required in Production

---

## Debugging Azure AD Flow

### Check Browser Console

When authentication happens, you'll see logs:
```
Received 401, attempting Azure AD authentication...
Azure AD authentication initialized
No account found, initiating login...
```

### Check Network Tab

1. **First request**: Status 401
2. **Azure AD requests**: Multiple calls to `login.microsoftonline.com`
3. **Second request**: Status 200 (with Authorization header)

### Inspect Token

After login, run this in console:
```javascript
// Get token
const token = await authManager.getAccessToken();
console.log(token);

// Decode token (paste at https://jwt.ms)
// Should see:
// - "aud": "api://400c2bdc-c965-413a-b3e9-b525afc37126"
// - "roles": ["Admin"]
// - "iss": "https://login.microsoftonline.com/{tenantId}/v2.0"
```

---

## Common Issues & Solutions

### Issue: Popup Blocked
**Symptom**: No Azure AD login appears

**Solution**: 
- Allow popups for localhost
- Or user clicks "Allow popups" in browser bar

---

### Issue: "Access denied. You may not have the required Admin role."
**Symptom**: Login succeeds but still get 401

**Solution**:
1. Check Azure Portal → Enterprise Applications → Your App → Users and groups
2. Verify user is assigned to **Admin** role
3. Sign out and sign in again to get new token with role

---

### Issue: Token expired
**Symptom**: Works initially, fails after 1 hour

**Solution**: 
- Automatic! `authManager.getAccessToken()` will acquire new token silently
- If silent refresh fails, popup appears again

---

### Issue: Wrong tenant
**Symptom**: Login works but 401 persists

**Solution**:
- Verify `TenantId` in `appsettings.Production.json` matches Azure Portal
- Check browser console for token validation errors
- Ensure `Audience` matches `ClientId`

---

## Manual Token Testing

If you want to test with a token outside the browser:

### Step 1: Get Token via Browser
1. Open: `https://login.microsoftonline.com/b2f2383b-bdc5-4372-b4c3-9b0308fcb4dc/oauth2/v2.0/authorize?client_id=400c2bdc-c965-413a-b3e9-b525afc37126&response_type=token&redirect_uri=http://localhost:5000&scope=api://400c2bdc-c965-413a-b3e9-b525afc37126/access_as_admin&response_mode=fragment`

2. Sign in

3. Copy token from URL: `http://localhost:5000#access_token=eyJ0eXAiOi...`

### Step 2: Test with PowerShell
```powershell
$token = "YOUR_ACCESS_TOKEN_HERE"

$headers = @{
    "Authorization" = "Bearer $token"
}

# Test API
Invoke-RestMethod -Uri "http://localhost:5000/api/user/all" -Headers $headers
```

### Step 3: Test with curl
```bash
curl -H "Authorization: Bearer YOUR_ACCESS_TOKEN_HERE" http://localhost:5000/api/user/all
```

---

## What Changed in Client Code

### Files Modified
- `admin.html`, `delete.html`, `users.html`, `user-detail.html`
  - Added MSAL.js library: `<script src="https://alcdn.msauth.net/browser/2.38.0/js/msal-browser.min.js"></script>`
  - Added azure-auth.js: `<script src="azure-auth.js"></script>`

### Files Created
- `azure-auth.js`: Authentication manager with `authenticatedFetch()` wrapper

### Files Modified (JavaScript)
- `admin.js`: 3 fetch calls → `authenticatedFetch`
- `delete.js`: 3 fetch calls → `authenticatedFetch`
- `users.js`: 1 fetch call → `authenticatedFetch`
- `user-detail.js`: 2 fetch calls → `authenticatedFetch`

### Key Function
```javascript
// Automatically handles 401 and retries with token
const response = await authenticatedFetch('/api/user/all');
```

**Before**: `fetch()` → fails with 401 in Production  
**After**: `authenticatedFetch()` → auto-login → retry → success

---

## Next Steps

1. **Test in Development**: Should work without any auth (current behavior preserved)

2. **Test in Production**:
   ```powershell
   $env:ASPNETCORE_ENVIRONMENT = "Production"
   dotnet run
   ```
   - Navigate to admin pages
   - Azure AD popup should appear
   - Sign in and verify admin operations work

3. **Deploy to Azure**: 
   - Set environment variables in Azure App Service
   - Or use Azure Key Vault for sensitive config
   - HTTPS required for Azure AD redirect URIs

4. **Monitor Usage**: Check Application Insights for authentication events and errors

---

## Pro Tips

### Silent Token Refresh
Tokens expire after ~1 hour. MSAL.js handles this automatically:
- `acquireTokenSilent()` refreshes in background
- Only shows popup if silent refresh fails

### Multiple Accounts
If user has multiple Azure AD accounts:
```javascript
// Check current user
const user = authManager.getCurrentUser();
console.log(user); // { username, name, id }

// Sign out
await authManager.logout();
```

### Caching
Tokens cached in `sessionStorage` (default)
- Survives page refresh
- Cleared when browser/tab closed
- Change in `azure-auth.js`: `cacheLocation: "localStorage"` for persistence

---

## Security Notes

✅ **What's Protected**:
- Tokens stored in browser session storage (protected by same-origin policy)
- HTTPS required in production (Azure AD enforces this)
- Tokens validated by Microsoft Identity Platform (cryptographic signature)

⚠️ **What to Watch**:
- XSS attacks can steal tokens (ensure all user input is sanitized)
- Token lifetime is 1 hour (use refresh tokens via MSAL.js)
- Ensure CORS is restricted in production (`AllowAnyOrigin()` only for dev)

🔒 **Best Practices**:
- Use HTTPS in production
- Set Content-Security-Policy headers
- Enable logging and monitoring
- Rotate Azure AD client secrets if using client credentials flow

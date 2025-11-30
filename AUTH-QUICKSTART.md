# Admin Authentication - Quick Reference

## 🎯 What's Protected?

### All AdminController endpoints:
- `POST /api/admin/puzzles` - Add puzzle
- `DELETE /api/admin/puzzles/{id}` - Delete puzzle  
- `POST /api/admin/puzzles/delete-bulk` - Delete multiple puzzles
- `GET /api/admin/puzzles` - Get all puzzles
- `POST /api/admin/puzzles/upload-bulk` - Bulk upload

### UserController admin endpoint:
- `GET /api/user/all` - List all users

## 🚀 Local Development (Default)

**No authentication required!** Admin endpoints work out of the box.

```bash
# Start server
dotnet run --project src/server

# Test admin endpoint (works without auth)
curl http://localhost:5000/api/user/all
```

## 🔐 Production Deployment

### Option 1: Simple API Key (Easiest)

**Set environment variable:**
```bash
# Linux/Mac
export Auth__AdminApiKey="your-secret-production-key-abc123"

# Windows PowerShell
$env:Auth__AdminApiKey="your-secret-production-key-abc123"

# Docker
docker run -e Auth__AdminApiKey="your-secret-key" ...
```

**Or update appsettings.Production.json:**
```json
{
  "Auth": {
    "BypassInDevelopment": false,
    "AdminApiKey": "your-secret-production-key"
  }
}
```

**Client usage:**
```javascript
fetch('/api/user/all', {
    headers: {
        'X-Admin-Key': 'your-secret-production-key'
    }
})
```

### Option 2: Azure AD (Recommended for Azure)

See `AUTHENTICATION-SETUP.md` for full Azure AD setup.

### Option 3: Google OAuth

See `AUTHENTICATION-SETUP.md` for full Google OAuth setup.

## 🧪 Testing with Auth Locally (Optional)

Want to test authentication locally?

**1. Update appsettings.Development.json:**
```json
{
  "Auth": {
    "BypassInDevelopment": false,
    "AdminApiKey": "test-key-123"
  }
}
```

**2. Add header to requests:**
```bash
curl -H "X-Admin-Key: test-key-123" http://localhost:5000/api/user/all
```

## 📝 Configuration Files

| File | Environment | Auth Behavior |
|------|-------------|---------------|
| `appsettings.json` | Base | Has auth config template |
| `appsettings.Development.json` | Development | `BypassInDevelopment: true` (no auth) |
| `appsettings.Production.json` | Production | `BypassInDevelopment: false` (auth required) |

## ⚠️ Common Issues

### Admin pages return 401 in production
✅ Set the `Auth__AdminApiKey` environment variable

### Admin pages don't work in development  
✅ Check `BypassInDevelopment: true` in appsettings.Development.json

### API key not working
✅ Verify the header name is exactly `X-Admin-Key` (case-sensitive)
✅ Check environment variable format: `Auth__AdminApiKey` (double underscore)

## 🔒 Security Best Practices

1. **Never commit API keys** to source control
2. **Use different keys** for each environment
3. **Rotate keys regularly** (at least quarterly)
4. **Use Azure KeyVault** or AWS Secrets Manager in production
5. **Enable HTTPS** (handled by reverse proxy/load balancer)
6. **Monitor failed auth attempts** in application logs

## 📚 Further Reading

- Full setup guide: `AUTHENTICATION-SETUP.md`
- Code: `src/server/Auth/AdminAuthHandler.cs`
- Configuration: `src/server/Program.cs`

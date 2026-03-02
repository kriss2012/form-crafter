# Manual API Testing Guide

## 🧪 Test Your Template API with These Commands

**API Base URL:** `https://form-crafter.onrender.com/api`

---

## Step 1: Register a Test User

```bash
curl -X POST https://form-crafter.onrender.com/api/auth/register ^
  -H "Content-Type: application/json" ^
  -d "{\"email\":\"testuser@example.com\",\"password\":\"Test123!\",\"displayName\":\"Test User\"}"
```

**Expected Response (201 Created):**
```json
{
  "token": "eyJhbGci...",
  "user": {
    "id": "...",
    "email": "testuser@example.com",
    "displayName": "Test User",
    "emailVerified": false,
    "createdAt": "2025-10-28T..."
  }
}
```

**📝 Save your token!** Copy the `token` value for next steps.

---

## Step 2: Login (if user already exists)

```bash
curl -X POST https://form-crafter.onrender.com/api/auth/login ^
  -H "Content-Type: application/json" ^
  -d "{\"email\":\"testuser@example.com\",\"password\":\"Test123!\"}"
```

---

## Step 3: Create a Template

**Replace `YOUR_TOKEN_HERE` with your actual token from Step 1/2**

```bash
curl -X POST https://form-crafter.onrender.com/api/templates ^
  -H "Content-Type: application/json" ^
  -H "Authorization: Bearer YOUR_TOKEN_HERE" ^
  -d "{\"name\":\"My Test Form\",\"description\":\"Created via curl\",\"jsonData\":\"{\\\"id\\\":\\\"00000000-0000-0000-0000-000000000000\\\",\\\"name\\\":\\\"Test Form\\\",\\\"elements\\\":[]}\",\"category\":\"Test\",\"tags\":\"curl,test\",\"isPublic\":false}"
```

**Expected Response (201 Created):**
```json
{
  "id": "abc123...",
  "userId": "...",
  "name": "My Test Form",
  "description": "Created via curl",
  "jsonData": "{...}",
  "category": "Test",
  "tags": "curl,test",
  "isPublic": false,
  "isFeatured": false,
  "createdAt": "2025-10-28T...",
  "updatedAt": "2025-10-28T...",
  "version": 1,
  "usageCount": 0
}
```

**📝 Save the `id`** from the response!

---

## Step 4: Get All Your Templates

```bash
curl -X GET https://form-crafter.onrender.com/api/templates ^
  -H "Authorization: Bearer YOUR_TOKEN_HERE"
```

**Expected Response (200 OK):**
```json
{
  "templates": [
    {
      "id": "abc123...",
      "name": "My Test Form",
      ...
    }
  ],
  "totalCount": 1
}
```

---

## Step 5: Get a Single Template

**Replace `TEMPLATE_ID` with the id from Step 3**

```bash
curl -X GET https://form-crafter.onrender.com/api/templates/TEMPLATE_ID ^
  -H "Authorization: Bearer YOUR_TOKEN_HERE"
```

**Expected Response (200 OK):**
```json
{
  "id": "abc123...",
  "name": "My Test Form",
  "description": "Created via curl",
  ...
}
```

---

## Step 6: Update a Template

```bash
curl -X PUT https://form-crafter.onrender.com/api/templates/TEMPLATE_ID ^
  -H "Content-Type: application/json" ^
  -H "Authorization: Bearer YOUR_TOKEN_HERE" ^
  -d "{\"name\":\"Updated Test Form\",\"description\":\"Updated via curl\",\"category\":\"Updated\"}"
```

**Expected Response (200 OK):**
```json
{
  "id": "abc123...",
  "name": "Updated Test Form",
  "description": "Updated via curl",
  "category": "Updated",
  ...
}
```

---

## Step 7: Delete a Template

```bash
curl -X DELETE https://form-crafter.onrender.com/api/templates/TEMPLATE_ID ^
  -H "Authorization: Bearer YOUR_TOKEN_HERE"
```

**Expected Response (204 No Content):**
- Empty response body
- Status code: 204

---

## 🚨 Common Errors & Solutions

### Error 401 Unauthorized
**Problem:** Missing or invalid token
**Solution:**
- Make sure you copied the full token from Step 1/2
- Token format: `Bearer eyJhbGci...`
- Check if token expired (valid for 1 hour)

### Error 400 Bad Request
**Problem:** Invalid JSON or missing required fields
**Solution:** Check your JSON syntax, ensure:
- `name` is required (not empty)
- `jsonData` is required (not empty)
- JSON strings are properly escaped

### Error 404 Not Found
**Problem:** Template doesn't exist or wrong ID
**Solution:**
- Verify the TEMPLATE_ID is correct
- Check if template was deleted
- Ensure you're using your own templates (not someone else's)

### Error 500 Internal Server Error
**Problem:** Server error
**Solution:** Check Render logs or wait a moment and retry

---

## 🎯 Quick Test Script (PowerShell)

Save this as `test-api.ps1`:

```powershell
# Step 1: Register/Login
$loginResponse = Invoke-RestMethod -Uri "https://form-crafter.onrender.com/api/auth/login" `
    -Method Post `
    -Headers @{"Content-Type"="application/json"} `
    -Body '{"email":"testuser@example.com","password":"Test123!"}'

$token = $loginResponse.token
Write-Host "✅ Logged in! Token: $($token.Substring(0,20))..."

# Step 2: Create Template
$createBody = @{
    name = "PowerShell Test Form"
    description = "Created via PowerShell"
    jsonData = '{"id":"00000000-0000-0000-0000-000000000000","name":"Test","elements":[]}'
    category = "Test"
    tags = "powershell,test"
    isPublic = $false
} | ConvertTo-Json

$template = Invoke-RestMethod -Uri "https://form-crafter.onrender.com/api/templates" `
    -Method Post `
    -Headers @{
        "Content-Type"="application/json"
        "Authorization"="Bearer $token"
    } `
    -Body $createBody

Write-Host "✅ Template created! ID: $($template.id)"

# Step 3: Get All Templates
$templates = Invoke-RestMethod -Uri "https://form-crafter.onrender.com/api/templates" `
    -Method Get `
    -Headers @{"Authorization"="Bearer $token"}

Write-Host "✅ Found $($templates.totalCount) templates"

# Step 4: Delete Template
Invoke-RestMethod -Uri "https://form-crafter.onrender.com/api/templates/$($template.id)" `
    -Method Delete `
    -Headers @{"Authorization"="Bearer $token"}

Write-Host "✅ Template deleted!"
```

Run with: `.\test-api.ps1`

---

## 🧪 Run Automated Unit Tests

```bash
cd FormMaker.Tests
dotnet test --filter "FullyQualifiedName~TemplateApiTests"
```

**This will run 8 comprehensive tests:**
1. ✅ Register and Login
2. ✅ Create Template
3. ✅ Get All Templates
4. ✅ Get Single Template
5. ✅ Update Template
6. ✅ Delete Template
7. ✅ Auth Required (401 test)
8. ✅ Invalid Data (400 test)

---

## 📊 What to Verify

After testing, check that:
- [x] Registration works and returns token
- [x] Login works with credentials
- [x] CREATE returns 201 with new template
- [x] GET list returns your templates
- [x] GET single returns specific template
- [x] UPDATE changes template data
- [x] DELETE removes template
- [x] CORS headers are present (check browser DevTools)
- [x] Same account can access forms from different browsers

---

## 🎉 Success Criteria

Your API is working if:
- ✅ All curl commands succeed with expected status codes
- ✅ Token authentication works
- ✅ Forms sync across browsers (same account)
- ✅ CORS headers allow GitHub Pages origin
- ✅ Database persists data (survives Render restart)

## Need Help?

Check Render logs: https://dashboard.render.com → Your Service → Logs

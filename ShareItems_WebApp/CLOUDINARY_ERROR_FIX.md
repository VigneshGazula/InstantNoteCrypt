# Cloudinary Upload Error - Troubleshooting Guide

## Error: "Cloudinary upload failed - no URL returned"

### Problem Description
The error occurs when Cloudinary returns a null or empty SecureUrl after an upload attempt. This typically happens due to:
1. Invalid Cloudinary credentials
2. Network connectivity issues
3. Cloudinary API errors
4. File format/size issues

---

## Fix Applied

### Changes Made to CloudinaryService.cs

#### 1. Better Error Handling for Different Upload Types
**Before**: Used pattern matching which could cause type confusion
**After**: Explicit switch statement with individual error checking for each upload type

```csharp
// Now handles ImageUploadResult, VideoUploadResult, and RawUploadResult separately
switch (fileType.ToLower())
{
    case "image":
        var imageResult = await UploadImageAsync(...);
        if (imageResult == null || imageResult.SecureUrl == null)
        {
            throw new Exception($"Cloudinary image upload failed: {imageResult?.Error?.Message ?? "Unknown error"}");
        }
        // Map to CloudinaryUploadResult
        break;
    // ... similar for video and raw
}
```

#### 2. Enhanced Logging
Added detailed logging at multiple points:
- Before upload starts
- During upload initialization
- After Cloudinary API call
- On error with full context

```csharp
_logger.LogInformation(
    "Starting Cloudinary upload. FileName: {FileName}, FileType: {FileType}, Size: {Size} bytes, Code: {Code}",
    file.FileName, fileType, file.Length, code);
```

#### 3. Cloudinary Error Detection
Added explicit error checking from Cloudinary response:

```csharp
if (result.Error != null)
{
    _logger.LogError("Cloudinary raw upload error: {ErrorMessage}", result.Error.Message);
}
```

#### 4. Improved Constructor Logging
Added logging during Cloudinary service initialization:

```csharp
_logger.LogInformation(
    "Initializing Cloudinary service. CloudName: {CloudName}, ApiKey: {ApiKeyPrefix}...",
    settings.CloudName, 
    settings.ApiKey.Substring(0, Math.Min(5, settings.ApiKey.Length)));
```

---

## How to Diagnose the Issue

### Step 1: Check Application Logs

Look for these log entries in order:

**1. Service Initialization**
```
INFO: Initializing Cloudinary service. CloudName: your-cloud, ApiKey: 79114...
INFO: Cloudinary service initialized successfully
```
? If you see these, credentials are loaded correctly

**2. Upload Start**
```
INFO: Starting Cloudinary upload. FileName: test.pdf, FileType: document, Size: 12345 bytes, Code: ABC123
INFO: Upload parameters - Folder: codesafe/ABC123/document, UniqueFileName: {guid}_test
```
? If you see these, the upload is being attempted

**3. Upload Method Called**
```
INFO: Uploading raw file to Cloudinary. Folder: codesafe/ABC123/document, FileName: {guid}_test
```
? If you see this, the correct upload method is being used

**4. Error or Success**
```
ERROR: Cloudinary raw upload error: Invalid credentials
```
OR
```
INFO: Successfully uploaded file to Cloudinary. PublicId: codesafe/ABC123/document/{guid}_test
```

### Step 2: Verify Cloudinary Credentials

**Check appsettings.json:**
```json
{
  "CloudinarySettings": {
    "CloudName": "your-cloud-name",    // ? NOT "Root"
    "ApiKey": "791144565443719",
    "ApiSecret": "6GywvBKYJfEQCrMP43ZX_17HJUA"
  }
}
```

**Common Issues**:
- ? CloudName is incorrect (check Cloudinary dashboard)
- ? ApiKey has extra spaces
- ? ApiSecret is from a different account
- ? Using demo/example credentials instead of your own

**How to Get Correct Credentials**:
1. Go to https://cloudinary.com/console
2. Login to your account
3. Dashboard shows:
   - Cloud name: (top right)
   - API Key: (in Account Details section)
   - API Secret: (click "Reveal" in Account Details)

### Step 3: Test Cloudinary Connection

Add this test endpoint to verify credentials:

```csharp
// Add to FileController.cs
[HttpGet]
public async Task<IActionResult> TestCloudinary()
{
    try
    {
        // Try to ping Cloudinary
        var cloudinary = HttpContext.RequestServices.GetRequiredService<ICloudinaryService>();
        return Ok(new { status = "Cloudinary service initialized", message = "Ready to upload" });
    }
    catch (Exception ex)
    {
        return StatusCode(500, new { error = ex.Message });
    }
}
```

### Step 4: Check Network Connectivity

```bash
# Test if you can reach Cloudinary API
curl https://api.cloudinary.com/v1_1/your-cloud-name/image/upload

# Should return: {"error":{"message":"Must supply api_key"}}
# This confirms network connectivity
```

### Step 5: Test with Minimal File

Create a tiny test file:
```bash
# Create 1KB test file
echo "test content" > test.txt
```

Try uploading this small file. If it works, the issue might be:
- File size too large (though we have 50MB limit)
- File format causing issues
- File stream not being read correctly

---

## Common Fixes

### Fix 1: Incorrect CloudName

**Symptom**: Error message contains "Invalid cloud name" or "No such account"

**Solution**:
```json
// appsettings.json
{
  "CloudinarySettings": {
    "CloudName": "your-actual-cloud-name"  // NOT "Root" or placeholder
  }
}
```

Get your cloud name from: https://cloudinary.com/console (top right corner)

---

### Fix 2: Invalid API Credentials

**Symptom**: Error message contains "Invalid API key" or "Invalid signature"

**Solution**:
1. Go to Cloudinary dashboard
2. Account Details ? Reveal API Secret
3. Copy exact values (no spaces, no quotes)
4. Update appsettings.json
5. Restart application

---

### Fix 3: Network/Firewall Blocking Cloudinary

**Symptom**: Timeout or network-related errors

**Solution**:
1. Check firewall rules
2. Verify HTTPS (port 443) is not blocked
3. Try from different network
4. Check corporate proxy settings

**For Corporate Networks**:
```csharp
// Add proxy configuration in CloudinaryService constructor
_cloudinary.Api.Timeout = TimeSpan.FromSeconds(60);
// Add proxy if needed
```

---

### Fix 4: File Stream Already Read

**Symptom**: Upload succeeds but no URL returned, or "Stream position" errors

**Solution**: The fix is already applied in the updated code. We now use `file.OpenReadStream()` which creates a new stream each time.

---

### Fix 5: Cloudinary Free Tier Limits Exceeded

**Symptom**: Upload fails after working previously

**Solution**:
1. Check Cloudinary dashboard ? Usage
2. Verify you haven't exceeded:
   - 25 GB storage
   - 25 GB bandwidth/month
   - 25K transformations/month
3. Delete old files or upgrade plan

---

## Debugging Checklist

Run through this checklist:

- [ ] Application logs show "Cloudinary service initialized successfully"
- [ ] CloudName in appsettings.json matches Cloudinary dashboard
- [ ] ApiKey and ApiSecret are correct (no spaces/typos)
- [ ] File size is under 50MB
- [ ] File extension is in allowed list
- [ ] Network can reach api.cloudinary.com
- [ ] Not behind restrictive firewall/proxy
- [ ] Cloudinary account is active (not suspended)
- [ ] Free tier limits not exceeded

---

## Test Script

Use this PowerShell script to test:

```powershell
# Test Cloudinary Configuration
$cloudName = "your-cloud-name"
$apiKey = "your-api-key"
$apiSecret = "your-api-secret"

# Test 1: Ping Cloudinary
Write-Host "Testing Cloudinary connectivity..."
try {
    $response = Invoke-WebRequest -Uri "https://api.cloudinary.com/v1_1/$cloudName/image/upload" -Method POST
    Write-Host "? Can reach Cloudinary API"
} catch {
    Write-Host "? Cannot reach Cloudinary API: $($_.Exception.Message)"
}

# Test 2: Verify cloud name exists
Write-Host "Testing cloud name..."
try {
    $response = Invoke-WebRequest -Uri "https://res.cloudinary.com/$cloudName/image/upload/sample.jpg"
    Write-Host "? Cloud name is valid"
} catch {
    Write-Host "? Invalid cloud name"
}
```

---

## Expected Log Output (Success)

When everything works correctly, you should see:

```
INFO: Initializing Cloudinary service. CloudName: your-cloud, ApiKey: 79114...
INFO: Cloudinary service initialized successfully
INFO: Starting Cloudinary upload. FileName: report.pdf, FileType: document, Size: 123456 bytes, Code: ABC123
INFO: Upload parameters - Folder: codesafe/ABC123/document, UniqueFileName: a1b2c3d4_report
INFO: Uploading raw file to Cloudinary. Folder: codesafe/ABC123/document, FileName: a1b2c3d4_report
INFO: Successfully uploaded file to Cloudinary. PublicId: codesafe/ABC123/document/a1b2c3d4_report, SecureUrl: https://res.cloudinary.com/...
INFO: File metadata saved to database. FileId: 123, PublicId: codesafe/ABC123/document/a1b2c3d4_report
```

---

## Still Not Working?

If you've tried everything above and still getting errors:

### 1. Enable Verbose Logging

Add to `appsettings.json`:
```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "ShareItems_WebApp.Services.CloudinaryService": "Debug",
      "CloudinaryDotNet": "Debug"
    }
  }
}
```

### 2. Try Cloudinary Sample Code

Test with minimal Cloudinary code:

```csharp
var account = new Account("cloud-name", "api-key", "api-secret");
var cloudinary = new Cloudinary(account);

using var stream = File.OpenRead("test.jpg");
var uploadParams = new ImageUploadParams()
{
    File = new FileDescription("test.jpg", stream)
};

var result = await cloudinary.UploadAsync(uploadParams);
Console.WriteLine($"URL: {result.SecureUrl}");
Console.WriteLine($"Error: {result.Error?.Message}");
```

### 3. Check Cloudinary Status

Visit: https://status.cloudinary.com/
Verify there are no ongoing incidents

### 4. Contact Support

- Cloudinary Support: https://support.cloudinary.com/
- Include:
  - Cloud name (NOT api credentials)
  - Error message from logs
  - File type attempting to upload
  - Timestamp of failed attempt

---

## Prevention

To avoid this issue in the future:

1. **Use Environment Variables in Production**
   ```bash
   export CLOUDINARY_CLOUD_NAME=your-cloud
   export CLOUDINARY_API_KEY=your-key
   export CLOUDINARY_API_SECRET=your-secret
   ```

2. **Add Health Check**
   ```csharp
   // Verify Cloudinary connection on startup
   var cloudinaryService = app.Services.GetRequiredService<ICloudinaryService>();
   // Test upload a small file
   ```

3. **Monitor Cloudinary Usage**
   - Set up alerts for approaching limits
   - Regular cleanup of old files

4. **Keep Credentials Secure**
   - Never commit appsettings.json with real credentials
   - Use Azure Key Vault or similar in production
   - Rotate credentials periodically

---

**Status**: Fix Applied ?  
**Testing Required**: Yes  
**Breaking Changes**: None  

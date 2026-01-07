# Cloudinary Integration - Quick Start Guide

## ?? Quick Setup (5 Minutes)

### Step 1: Verify Configuration
Check `appsettings.json`:
```json
{
  "CloudinarySettings": {
    "CloudName": "your-cloud-name",
    "ApiKey": "your-api-key",
    "ApiSecret": "your-api-secret"
  }
}
```

? Replace with your actual Cloudinary credentials  
? Get them from: https://cloudinary.com/console

### Step 2: Install Package (Already Done)
```bash
dotnet add package CloudinaryDotNet
```

### Step 3: Run Migration (Already Done)
```bash
dotnet ef database update
```

### Step 4: Test the Integration

#### Test 1: Upload a File
1. Run the application
2. Navigate to Dashboard
3. Upload a PDF file
4. Check Cloudinary dashboard ? Should see file in `codesafe/{code}/documents/`

#### Test 2: Download a File
1. Click download link
2. Should redirect to Cloudinary URL
3. File should download from Cloudinary CDN

#### Test 3: Delete a File
1. Delete a file from dashboard
2. Check Cloudinary dashboard ? File should be removed
3. Check database ? Record should be removed

---

## ?? What's New

### Before (Local Storage)
```
Upload ? Save to disk (wwwroot/uploads) ? Save DB record
Download ? Read from disk ? Send bytes to client
Delete ? Delete from disk ? Delete from DB
```

### After (Cloudinary)
```
Upload ? Upload to Cloudinary ? Save DB record with URL
Download ? Redirect to Cloudinary URL
Delete ? Delete from Cloudinary ? Delete from DB
```

---

## ?? How to Verify It's Working

### Check 1: Database
```sql
SELECT TOP 10 FileUrl, PublicId FROM NoteFiles
```
Should see:
- `FileUrl`: `https://res.cloudinary.com/...`
- `PublicId`: `codesafe/{code}/{type}/...`

### Check 2: Cloudinary Dashboard
1. Go to https://cloudinary.com/console
2. Click "Media Library"
3. Navigate to `codesafe` folder
4. Should see uploaded files organized by code and type

### Check 3: Application Logs
```
INFO: Uploading file to Cloudinary. FileName: test.pdf
INFO: Successfully uploaded file to Cloudinary. PublicId: codesafe/ABC123/documents/...
INFO: File metadata saved to database. FileId: 1
```

---

## ?? Supported File Types

### Documents (15 types)
.pdf .doc .docx .txt .rtf .odt .ppt .pptx .xls .xlsx .csv .md .json .xml .yaml .yml

### Images (11 types)
.jpg .jpeg .png .webp .gif .bmp .tiff .tif .svg .ico .heic

### Videos (9 types)
.mp4 .mov .mkv .webm .avi .wmv .flv .m4v .3gp

### Others (13 types)
.zip .rar .7z .tar .gz .psd .ai .figma .blend .obj .stl .log .dat

### Blocked (Security)
.exe .bat .cmd .sh .ps1 .js .vbs .jar .php .py .rb

---

## ?? Configuration Options

### Development Environment
Use `appsettings.Development.json`:
```json
{
  "CloudinarySettings": {
    "CloudName": "dev-cloud",
    "ApiKey": "dev-key",
    "ApiSecret": "dev-secret"
  }
}
```

### Production Environment
Use environment variables:
```bash
export CLOUDINARY_CLOUD_NAME=prod-cloud
export CLOUDINARY_API_KEY=prod-key
export CLOUDINARY_API_SECRET=prod-secret
```

---

## ?? Troubleshooting

### Problem: "Invalid Cloudinary credentials"
**Solution**: 
1. Check CloudName, ApiKey, ApiSecret
2. Verify they match your Cloudinary dashboard
3. Restart application

### Problem: "File upload fails"
**Solution**:
1. Check file size < 50MB
2. Check file extension is allowed
3. Check application logs for errors
4. Verify internet connection

### Problem: "Download returns 404"
**Solution**:
1. Check FileUrl in database is not empty
2. Verify file exists in Cloudinary dashboard
3. Check PublicId format: `codesafe/{code}/{type}/...`

---

## ?? Folder Structure

```
Cloudinary Media Library
??? codesafe/
    ??? ABC123/
    ?   ??? documents/
    ?   ?   ??? {guid}_report.pdf
    ?   ??? images/
    ?   ?   ??? {guid}_screenshot.png
    ?   ??? videos/
    ?       ??? {guid}_demo.mp4
    ??? XYZ789/
        ??? others/
            ??? {guid}_data.zip
```

---

## ?? Security Checklist

- [x] File size validation (50MB max)
- [x] Extension whitelist validation
- [x] Blocked executables (.exe, .bat, etc.)
- [x] PIN protection for locked notes
- [x] Secure HTTPS URLs
- [x] No credentials in source control
- [x] Transaction safety with rollback

---

## ?? Monitoring

### Cloudinary Usage
- Dashboard: https://cloudinary.com/console
- Check: Storage, Bandwidth, Transformations
- Free tier: 25GB storage, 25GB bandwidth/month

### Application Logs
```bash
# View upload logs
grep "Uploading file to Cloudinary" logs/*.log

# View errors
grep "ERROR" logs/*.log | grep Cloudinary
```

---

## ? Success Criteria

After setup, you should see:

1. ? Files uploaded to Cloudinary
2. ? Database records with FileUrl and PublicId
3. ? Download links redirect to Cloudinary
4. ? Delete removes from both Cloudinary and DB
5. ? Files organized in codesafe/{code}/{type} folders
6. ? No files in local wwwroot/uploads directory
7. ? Application logs show successful uploads

---

## ?? Next Steps

1. **Test all file types**: Upload document, image, video, others
2. **Test PIN protection**: Try uploading/downloading locked files
3. **Monitor usage**: Check Cloudinary dashboard for storage/bandwidth
4. **Optimize**: Set up auto-delete for old files if needed
5. **Scale**: Add bulk upload, progress tracking, etc.

---

## ?? Support

- Cloudinary Docs: https://cloudinary.com/documentation
- CloudinaryDotNet SDK: https://github.com/cloudinary/CloudinaryDotNet
- Application Logs: Check `logs/` directory

---

**Status**: ? Ready to Use  
**Setup Time**: ~5 minutes  
**Difficulty**: Easy  

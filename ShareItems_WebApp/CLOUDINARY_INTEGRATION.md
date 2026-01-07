# Cloudinary Integration - Complete Implementation Guide

## Overview
This document describes the complete Cloudinary integration for the CodeSafe (formerly InstantNoteCrypt) application. All file uploads now use Cloudinary as the storage backend instead of local file system.

---

## Architecture

### Storage Backend: Cloudinary
- **All files** stored in Cloudinary cloud storage
- **No local file storage** - wwwroot/uploads directory no longer used
- **Secure URLs** - All files served via HTTPS from Cloudinary
- **Automatic organization** - Files organized by code and type

### Folder Structure in Cloudinary
```
codesafe/
??? {code1}/
?   ??? documents/
?   ?   ??? {guid}_filename.pdf
?   ?   ??? {guid}_filename.docx
?   ??? images/
?   ?   ??? {guid}_photo.jpg
?   ?   ??? {guid}_screenshot.png
?   ??? videos/
?   ?   ??? {guid}_video.mp4
?   ??? others/
?       ??? {guid}_archive.zip
?       ??? {guid}_data.xlsx
??? {code2}/
    ??? ...
```

---

## Configuration

### appsettings.json
```json
{
  "CloudinarySettings": {
    "CloudName": "your-cloud-name",
    "ApiKey": "your-api-key",
    "ApiSecret": "your-api-secret"
  }
}
```

?? **IMPORTANT**: Never commit real credentials to source control!

### Environment Variables (Production)
For production, use environment variables:
- `CLOUDINARY_CLOUD_NAME`
- `CLOUDINARY_API_KEY`
- `CLOUDINARY_API_SECRET`

---

## Database Schema

### NoteFile Entity (Updated)
```csharp
public class NoteFile
{
    public int Id { get; set; }
    public int NoteId { get; set; }
    public string FileName { get; set; }          // Original filename
    public string StoredFileName { get; set; }     // GUID-based name
    public string FileType { get; set; }           // document|image|video|others
    public string ContentType { get; set; }        // MIME type
    public long FileSize { get; set; }             // Bytes
    
    // NEW: Cloudinary fields
    public string FileUrl { get; set; }            // Secure HTTPS URL
    public string PublicId { get; set; }           // Cloudinary public ID
    
    // DEPRECATED: No longer used
    public string? FilePath { get; set; }          // Kept for backward compatibility
    
    public DateTime UploadedAt { get; set; }
    public Note Note { get; set; }
}
```

### Migration
```bash
dotnet ef migrations add CloudinaryIntegration
dotnet ef database update
```

---

## Services

### 1. CloudinarySettings
**Location**: `Settings/CloudinarySettings.cs`

```csharp
public class CloudinarySettings
{
    public string CloudName { get; set; }
    public string ApiKey { get; set; }
    public string ApiSecret { get; set; }
    
    public bool IsValid() { ... }
}
```

### 2. ICloudinaryService
**Location**: `Services/ICloudinaryService.cs`

```csharp
public interface ICloudinaryService
{
    Task<CloudinaryUploadResult> UploadFileAsync(IFormFile file, string code, string fileType);
    Task<bool> DeleteFileAsync(string publicId, string resourceType);
    Task<bool> DeleteFolderAsync(string code);
}
```

### 3. CloudinaryService
**Location**: `Services/CloudinaryService.cs`

**Key Features**:
- ? Uploads using in-memory streams (no disk I/O)
- ? Automatic folder organization
- ? Support for images, videos, and raw files
- ? GUID-based unique filenames
- ? Comprehensive logging
- ? Error handling with rollback support

**Upload Logic**:
```
File Type ? Upload Method ? Cloudinary Resource Type
?????????????????????????????????????????????????????
image     ? ImageUploadParams  ? image
video     ? VideoUploadParams  ? video
document  ? RawUploadParams    ? raw
others    ? RawUploadParams    ? raw
```

### 4. FileStorageService (Updated)
**Location**: `Services/FileStorageService.cs`

**Changes**:
- ? Removed local file system operations
- ? Added Cloudinary upload/delete operations
- ? Transaction safety with rollback
- ? Validation using FileValidationHelper
- ? Database-Cloudinary synchronization

---

## File Upload Flow

### Step-by-Step Process

1. **Validation**
   ```
   ? File size (max 50MB)
   ? Extension not blocked (.exe, .bat, etc.)
   ? Extension matches file type
   ```

2. **Cloudinary Upload**
   ```
   ? Open file stream (in-memory)
   ? Generate unique filename (GUID)
   ? Determine upload params (image/video/raw)
   ? Upload to Cloudinary folder: codesafe/{code}/{fileType}
   ? Receive SecureUrl and PublicId
   ```

3. **Database Save**
   ```
   ? Create NoteFile record
   ? Store FileUrl and PublicId
   ? Save to database
   ? If DB fails ? Delete from Cloudinary (rollback)
   ```

4. **Success**
   ```
   ? Return NoteFile with Cloudinary URL
   ? File accessible via secure HTTPS URL
   ```

---

## File Type Rules

### Allowed Extensions (Whitelist)

#### Documents (15 types)
```
.pdf .doc .docx .txt .rtf .odt
.ppt .pptx .xls .xlsx .csv
.md .json .xml .yaml .yml
```

#### Images (11 types)
```
.jpg .jpeg .png .webp .gif .bmp
.tiff .tif .svg .ico .heic
```

#### Videos (9 types)
```
.mp4 .mov .mkv .webm .avi
.wmv .flv .m4v .3gp
```

#### Others (13 types)
```
.zip .rar .7z .tar .gz
.psd .ai .figma .blend
.obj .stl .log .dat
```

### Blocked Extensions (Security)
```
.exe .bat .cmd .sh .ps1
.js .vbs .jar .php .py .rb
```

**Rule**: Only explicitly whitelisted extensions are allowed. Everything else is rejected.

---

## API Endpoints

### Upload File
```http
POST /File/Upload
Content-Type: multipart/form-data

Parameters:
- code: string (note code)
- file: IFormFile
- fileType: string (auto-detected, can be overridden)
- pin: string? (optional, if note is locked)

Response:
{
  "success": true,
  "fileId": 123,
  "fileName": "report.pdf",
  "fileSize": 1024000,
  "fileType": "document",
  "uploadedAt": "2024-01-07T14:00:00Z"
}
```

### Download File
```http
GET /File/Download?fileId=123&pin=xxxx

Redirects to: https://res.cloudinary.com/{cloud}/raw/upload/...
```

### Get Files
```http
GET /File/GetFiles?code=ABC123&fileType=document&pin=xxxx

Response:
[
  {
    "id": 123,
    "fileName": "report.pdf",
    "fileType": "document",
    "fileSize": 1024000,
    "contentType": "application/pdf",
    "fileUrl": "https://res.cloudinary.com/...",
    "uploadedAt": "2024-01-07T14:00:00Z"
  }
]
```

### Delete File
```http
POST /File/Delete
Parameters:
- fileId: int
- pin: string? (if note is locked)

Response:
{
  "success": true,
  "message": "File deleted successfully"
}
```

---

## Security Features

### 1. PIN Protection
- Files linked to locked notes require PIN for access
- PIN validated before upload/download/delete
- Prevents unauthorized access

### 2. Extension Validation
- Whitelist-only approach
- Blocked executables and scripts
- Content-type verification

### 3. File Size Limits
- Maximum 50MB per file
- Prevents abuse and excessive storage

### 4. Secure URLs
- All files served via HTTPS
- Cloudinary secure URLs (not public URLs)
- URLs tied to Cloudinary account

### 5. No Public Browsing
- Files organized in private folders
- Cloudinary dashboard access required
- No direct folder listing

---

## Error Handling

### Upload Failures
```csharp
try {
    uploadResult = await cloudinaryService.UploadFileAsync(...);
    await database.SaveAsync();
}
catch (CloudinaryException) {
    // Upload failed - show error to user
}
catch (DbUpdateException) {
    // DB save failed - rollback Cloudinary upload
    await cloudinaryService.DeleteFileAsync(publicId);
}
```

### Download Failures
```csharp
// File not found in DB
if (file == null) return NotFound("File not found");

// Invalid PIN
if (note.HasPin && !ValidatePin(pin))
    return Unauthorized("Invalid PIN");

// Cloudinary URL invalid (rare)
// User gets 404 from Cloudinary
```

### Delete Failures
```csharp
// Delete from Cloudinary first
var deleted = await cloudinary.DeleteFileAsync(...);

// Continue with DB deletion even if Cloudinary fails
// (orphaned files cleaned up manually if needed)
await database.RemoveAsync(file);
```

---

## Testing

### Manual Testing Checklist

**Upload Tests**:
- [ ] Upload PDF document ? Categorized as "document"
- [ ] Upload JPG image ? Categorized as "image"
- [ ] Upload MP4 video ? Categorized as "video"
- [ ] Upload ZIP archive ? Categorized as "others"
- [ ] Upload 60MB file ? Rejected (size limit)
- [ ] Upload .exe file ? Rejected (blocked extension)

**Download Tests**:
- [ ] Download document ? Redirects to Cloudinary URL
- [ ] Download image ? Opens in browser
- [ ] Download video ? Streams from Cloudinary
- [ ] Download locked note file without PIN ? Unauthorized

**Delete Tests**:
- [ ] Delete file ? Removed from Cloudinary and DB
- [ ] Delete locked note file without PIN ? Unauthorized
- [ ] Delete all note files ? All removed from Cloudinary

**Organization Tests**:
- [ ] Files uploaded to correct folder: codesafe/{code}/{type}
- [ ] Multiple files for same code ? Same folder
- [ ] Different codes ? Different folders

---

## Deployment

### Prerequisites
1. Cloudinary account created
2. Cloud name, API key, and API secret obtained
3. CloudinaryDotNet NuGet package installed (v1.27.9+)

### Configuration Steps

#### Development
1. Add credentials to `appsettings.Development.json`
2. Never commit this file to source control

#### Production
1. Set environment variables:
   ```bash
   CLOUDINARY_CLOUD_NAME=your-cloud-name
   CLOUDINARY_API_KEY=your-api-key
   CLOUDINARY_API_SECRET=your-api-secret
   ```

2. Update `Program.cs` to read from environment:
   ```csharp
   var cloudName = Environment.GetEnvironmentVariable("CLOUDINARY_CLOUD_NAME");
   // etc.
   ```

### Database Migration
```bash
# Apply migration
dotnet ef database update

# Verify new columns exist
SELECT * FROM NoteFiles
# Should see FileUrl and PublicId columns
```

### Cleanup Old Files (Optional)
```bash
# Remove old uploads directory
rm -rf wwwroot/uploads
```

---

## Monitoring

### Cloudinary Dashboard
- Monitor storage usage
- View uploaded files
- Check bandwidth usage
- Review transformation usage

### Application Logs
```
INFO: Uploading file to Cloudinary. FileName: report.pdf, FileType: document
INFO: Successfully uploaded to Cloudinary. PublicId: codesafe/ABC123/documents/...
INFO: File metadata saved to database. FileId: 123

ERROR: Failed to upload file to Cloudinary: Network timeout
ERROR: Database save failed. Attempting rollback...
```

---

## Troubleshooting

### Issue: "Invalid Cloudinary credentials"
**Solution**: Verify CloudName, ApiKey, ApiSecret in appsettings.json

### Issue: "File upload fails silently"
**Solution**: Check application logs for Cloudinary errors

### Issue: "Files not showing in Cloudinary dashboard"
**Solution**: Check folder path format: codesafe/{code}/{fileType}

### Issue: "Download returns 404"
**Solution**: Verify FileUrl is stored correctly in database

### Issue: "Database has FileUrl but Cloudinary file missing"
**Solution**: File may have been manually deleted from Cloudinary dashboard

---

## Migration from Local Storage

If migrating existing files:

1. **Export file metadata**:
   ```sql
   SELECT * FROM NoteFiles WHERE FileUrl IS NULL OR FileUrl = ''
   ```

2. **For each file**:
   - Read from local disk
   - Upload to Cloudinary
   - Update FileUrl and PublicId
   - Verify upload
   - Delete local file

3. **Cleanup**:
   - Remove old uploads directory
   - Update FilePath to NULL in database

---

## Best Practices

### DO ?
- Use environment variables in production
- Validate file extensions against whitelist
- Log all upload/delete operations
- Implement rollback on failures
- Use secure URLs (HTTPS)
- Organize files in folders
- Check file size before upload

### DON'T ?
- Don't commit credentials to source control
- Don't trust client-provided file types
- Don't allow unlimited file sizes
- Don't skip PIN validation
- Don't delete from DB without Cloudinary cleanup
- Don't use public URLs for sensitive files
- Don't allow executable files

---

## Performance Considerations

### Upload Performance
- ? Streaming upload (no disk I/O)
- ? Parallel uploads possible
- ? Cloudinary CDN for fast delivery

### Download Performance
- ? Direct Cloudinary CDN serving
- ? Global CDN distribution
- ? Automatic format optimization

### Storage Optimization
- ? Cloudinary auto-compression for images
- ? Lazy loading support
- ? Responsive image generation

---

## Cost Optimization

### Free Tier Limits
- 25 GB storage
- 25 GB bandwidth/month
- 25K transformations/month

### Recommendations
- Delete unused files regularly
- Monitor bandwidth usage
- Use Cloudinary transformations efficiently
- Set file size limits

---

## Future Enhancements

### Planned Features
- [ ] Signed URLs for time-limited access
- [ ] Image transformations (resize, crop)
- [ ] Video streaming optimization
- [ ] Bulk upload support
- [ ] Progress tracking for large files
- [ ] Duplicate file detection
- [ ] Automatic file expiration

### Possible Improvements
- [ ] Client-side upload (direct to Cloudinary)
- [ ] Thumbnail generation for documents
- [ ] Preview generation for videos
- [ ] File versioning
- [ ] Backup to secondary storage

---

## Summary

### What Changed
- ? Local file system storage removed
- ? Cloudinary cloud storage implemented
- ? Secure HTTPS URLs for all files
- ? Automatic folder organization
- ? Transaction safety with rollback
- ? Comprehensive file type support

### What Stayed the Same
- ? PIN protection still works
- ? File type categorization
- ? Upload/download/delete APIs
- ? Database structure (extended)
- ? Security validation

### Impact
- ?? Scalable storage
- ?? Global CDN delivery
- ?? Secure file serving
- ?? Better monitoring
- ?? No server storage costs
- ? Faster file delivery

---

**Status**: ? Production Ready  
**Version**: 1.0.0  
**Last Updated**: January 7, 2024  
**Migration Required**: Yes (database columns added)  

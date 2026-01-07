# Cloudinary Integration - Testing Guide

## ?? Testing Checklist

Use this checklist to verify the Cloudinary integration is working correctly.

---

## Pre-Testing Setup

### 1. Verify Configuration
```bash
# Check appsettings.json
cat appsettings.json | grep -A 5 CloudinarySettings
```

Expected output:
```json
"CloudinarySettings": {
  "CloudName": "your-cloud-name",
  "ApiKey": "your-api-key",
  "ApiSecret": "your-api-secret"
}
```

### 2. Verify Database Migration
```sql
-- Check if new columns exist
SELECT COLUMN_NAME 
FROM INFORMATION_SCHEMA.COLUMNS 
WHERE TABLE_NAME = 'NoteFiles' 
  AND COLUMN_NAME IN ('FileUrl', 'PublicId');
```

Expected: 2 rows returned

### 3. Start Application
```bash
dotnet run
```

Check logs for:
```
info: Application started successfully
info: Cloudinary configuration loaded
```

---

## Test Suite 1: File Upload

### Test 1.1: Upload PDF Document
**Steps**:
1. Navigate to `/Dashboard/{code}`
2. Select file: `test-document.pdf`
3. Click "Upload File"

**Expected Result**:
- ? Success message: "File uploaded successfully as document."
- ? File appears in files list
- ? Cloudinary dashboard shows file in `codesafe/{code}/documents/`

**Verify in Database**:
```sql
SELECT TOP 1 FileName, FileType, FileUrl, PublicId 
FROM NoteFiles 
ORDER BY Id DESC;
```

Expected:
- `FileName`: test-document.pdf
- `FileType`: document
- `FileUrl`: https://res.cloudinary.com/...
- `PublicId`: codesafe/{code}/documents/...

---

### Test 1.2: Upload JPG Image
**Steps**:
1. Select file: `test-image.jpg`
2. Click "Upload File"

**Expected Result**:
- ? Success message: "File uploaded successfully as image."
- ? File categorized as "image"
- ? Cloudinary shows file in `codesafe/{code}/images/`

---

### Test 1.3: Upload MP4 Video
**Steps**:
1. Select file: `test-video.mp4`
2. Click "Upload File"

**Expected Result**:
- ? Success message: "File uploaded successfully as video."
- ? File categorized as "video"
- ? Cloudinary shows file in `codesafe/{code}/videos/`

---

### Test 1.4: Upload ZIP Archive
**Steps**:
1. Select file: `test-archive.zip`
2. Click "Upload File"

**Expected Result**:
- ? Success message: "File uploaded successfully as others."
- ? File categorized as "others"
- ? Cloudinary shows file in `codesafe/{code}/others/`

---

### Test 1.5: Upload Large File (>50MB)
**Steps**:
1. Create or find file > 50MB
2. Try to upload

**Expected Result**:
- ? Error message: "File size exceeds maximum allowed size of 50 MB"
- ? File NOT uploaded
- ? No record in database

---

### Test 1.6: Upload Blocked Extension (.exe)
**Steps**:
1. Try to upload `program.exe`

**Expected Result**:
- ? Error message: "File extension '.exe' is not allowed for security reasons"
- ? File NOT uploaded
- ? No record in database

---

## Test Suite 2: File Download

### Test 2.1: Download Document
**Steps**:
1. Click "Download" link for a PDF file

**Expected Result**:
- ? Browser redirects to Cloudinary URL
- ? URL pattern: `https://res.cloudinary.com/{cloudName}/raw/upload/...`
- ? File downloads successfully

**Verify**:
```bash
# Check browser network tab
# Should see redirect (302) to Cloudinary URL
# Then download from Cloudinary (200)
```

---

### Test 2.2: Download Image
**Steps**:
1. Click "Download" link for an image

**Expected Result**:
- ? Redirects to Cloudinary URL
- ? Image displays in browser OR downloads
- ? Served from Cloudinary CDN

---

### Test 2.3: Download Protected File (PIN Required)
**Steps**:
1. Set PIN on note
2. Try to download file without entering PIN

**Expected Result**:
- ? Error or prompt for PIN
- ? File NOT accessible without PIN

**Then**:
1. Enter correct PIN
2. Try download again

**Expected Result**:
- ? File downloads successfully

---

## Test Suite 3: File Delete

### Test 3.1: Delete File
**Steps**:
1. Upload a test file
2. Note the PublicId from database
3. Delete the file from dashboard

**Expected Result**:
- ? Success message
- ? File removed from files list
- ? Record removed from database
- ? File removed from Cloudinary

**Verify in Database**:
```sql
SELECT * FROM NoteFiles WHERE Id = {deletedFileId};
```
Expected: 0 rows

**Verify in Cloudinary**:
- Check Media Library
- File should NOT appear in folder

---

### Test 3.2: Delete All Note Files
**Steps**:
1. Upload multiple files to a note
2. Delete the note (Destroy Note feature)

**Expected Result**:
- ? All files deleted from Cloudinary
- ? All records removed from database
- ? Folder in Cloudinary becomes empty

---

## Test Suite 4: File Filtering

### Test 4.1: Filter Documents
**Steps**:
1. Upload files of different types
2. Click "Documents" button

**Expected Result**:
- ? Only document files shown (.pdf, .doc, .docx, etc.)
- ? Images, videos, others hidden

---

### Test 4.2: Filter Images
**Steps**:
1. Click "Photos" button

**Expected Result**:
- ? Only image files shown (.jpg, .png, .webp, etc.)
- ? Documents, videos, others hidden

---

### Test 4.3: Filter Videos
**Steps**:
1. Click "Videos" button

**Expected Result**:
- ? Only video files shown (.mp4, .mov, .mkv, etc.)
- ? Documents, images, others hidden

---

### Test 4.4: Filter Others
**Steps**:
1. Click "Others" button

**Expected Result**:
- ? Only other files shown (.zip, .rar, .xlsx, etc.)
- ? Documents, images, videos hidden

---

## Test Suite 5: Automatic File Type Detection

### Test 5.1: PDF ? Document
Upload: `report.pdf`  
Expected Type: `document`

### Test 5.2: Word ? Document
Upload: `memo.docx`  
Expected Type: `document`

### Test 5.3: Excel ? Others
Upload: `data.xlsx`  
Expected Type: `others`

### Test 5.4: PowerPoint ? Others
Upload: `presentation.pptx`  
Expected Type: `others`

### Test 5.5: Text ? Others
Upload: `notes.txt`  
Expected Type: `others`

### Test 5.6: JSON ? Others
Upload: `config.json`  
Expected Type: `others`

### Test 5.7: Various Images
Upload multiple image formats:
- `photo.jpg` ? `image`
- `screenshot.png` ? `image`
- `graphic.webp` ? `image`
- `icon.svg` ? `image`

### Test 5.8: Various Videos
Upload multiple video formats:
- `clip.mp4` ? `video`
- `movie.mov` ? `video`
- `recording.mkv` ? `video`

---

## Test Suite 6: Error Handling

### Test 6.1: Invalid Cloudinary Credentials
**Steps**:
1. Change CloudName in appsettings.json to invalid value
2. Restart application
3. Try to upload file

**Expected Result**:
- ? Application startup error OR upload error
- ? Clear error message about credentials
- ? File NOT uploaded

---

### Test 6.2: Network Timeout
**Steps**:
1. Disconnect internet
2. Try to upload file

**Expected Result**:
- ? Error message about network failure
- ? File NOT saved to database
- ? No partial upload

---

### Test 6.3: Database Error During Upload
**Steps**:
1. Temporarily break database connection
2. Try to upload file

**Expected Result**:
- ? Error message about database
- ? File NOT saved to database
- ? File DELETED from Cloudinary (rollback)

---

## Test Suite 7: Folder Organization

### Test 7.1: Multiple Codes
**Steps**:
1. Upload files to code "ABC123"
2. Upload files to code "XYZ789"

**Expected Result in Cloudinary**:
```
codesafe/
??? ABC123/
?   ??? documents/
?   ??? images/
??? XYZ789/
    ??? documents/
    ??? videos/
```

---

### Test 7.2: Multiple File Types per Code
**Steps**:
1. Upload document, image, video, and other to same code

**Expected Result**:
```
codesafe/
??? CODE123/
    ??? documents/
    ?   ??? file1.pdf
    ??? images/
    ?   ??? file2.jpg
    ??? videos/
    ?   ??? file3.mp4
    ??? others/
        ??? file4.zip
```

---

## Test Suite 8: PIN Protection

### Test 8.1: Upload to Locked Note
**Steps**:
1. Create note with PIN
2. Try to upload file without PIN

**Expected Result**:
- ? Upload fails (if API checks PIN)
- OR ? Upload succeeds (if PIN only protects access)

---

### Test 8.2: Download from Locked Note
**Steps**:
1. Upload file to locked note
2. Try to download without PIN

**Expected Result**:
- ? Access denied
- ? File NOT accessible

**Then**:
1. Enter correct PIN
2. Try download

**Expected Result**:
- ? File downloads successfully

---

## Test Suite 9: Performance

### Test 9.1: Upload Speed
**Steps**:
1. Upload 1MB file ? Time it
2. Upload 10MB file ? Time it
3. Upload 50MB file ? Time it

**Expected Results**:
- 1MB: ~2-3 seconds
- 10MB: ~5-8 seconds
- 50MB: ~20-30 seconds

---

### Test 9.2: Download Speed
**Steps**:
1. Download file multiple times
2. Check if CDN caching works

**Expected Result**:
- First download: Normal speed
- Subsequent downloads: Faster (cached)

---

### Test 9.3: Multiple Concurrent Uploads
**Steps**:
1. Upload 3-5 files simultaneously

**Expected Result**:
- ? All uploads succeed
- ? No conflicts
- ? Each file has unique PublicId

---

## Test Suite 10: Cloudinary Dashboard Verification

### Test 10.1: Media Library Check
**Steps**:
1. Login to https://cloudinary.com/console
2. Click "Media Library"
3. Navigate to `codesafe` folder

**Expected Result**:
- ? Folder exists
- ? Sub-folders for each code
- ? Files organized by type
- ? Filenames match GUID pattern

---

### Test 10.2: Usage Statistics
**Steps**:
1. Check Dashboard ? Usage tab

**Expected Result**:
- ? Storage usage increases after uploads
- ? Bandwidth usage tracked
- ? Within free tier limits (or as expected)

---

## Test Suite 11: Logging

### Test 11.1: Successful Upload Logs
**Check logs for**:
```
INFO: Uploading file to Cloudinary. FileName: test.pdf, FileType: document
INFO: Successfully uploaded file to Cloudinary. PublicId: codesafe/...
INFO: File metadata saved to database. FileId: 123
```

---

### Test 11.2: Error Logs
**Trigger error and check logs for**:
```
ERROR: Failed to upload file to Cloudinary: [error details]
ERROR: Error saving file metadata to database. Attempting rollback...
```

---

## Regression Testing

After any code changes, re-run:
- [ ] Test 1.1 (Upload PDF)
- [ ] Test 2.1 (Download file)
- [ ] Test 3.1 (Delete file)
- [ ] Test 4.1 (Filter documents)
- [ ] Test 5.1-5.8 (Auto-detection)

---

## Test Result Template

```
Test: [Test Name]
Date: [Date]
Tester: [Your Name]

Steps:
1. [Step 1]
2. [Step 2]
3. [Step 3]

Expected Result:
[Description]

Actual Result:
[What actually happened]

Status: ? PASS / ? FAIL

Notes:
[Any additional observations]
```

---

## Success Criteria

The integration is successful if:

- ? All upload tests pass
- ? All download tests pass
- ? All delete tests pass
- ? All filtering tests pass
- ? All file type detection tests pass
- ? Error handling works correctly
- ? Cloudinary dashboard shows correct organization
- ? Logs are comprehensive
- ? Performance is acceptable
- ? PIN protection works

---

## Failure Escalation

If any test fails:

1. Check application logs
2. Check Cloudinary dashboard
3. Check database records
4. Review code changes
5. Debug with breakpoints
6. Verify configuration
7. Document issue
8. Fix and re-test

---

**Testing Complete**: ___/___/___ (Date)  
**All Tests Passed**: ? Yes  ? No  
**Notes**: _______________________________

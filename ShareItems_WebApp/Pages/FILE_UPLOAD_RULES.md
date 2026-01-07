# File Upload Rules - Complete Documentation

## Overview
The InstantNoteCrypt application supports a comprehensive list of file types across four categories, with strict security controls to prevent malicious file uploads.

## Supported File Categories

### ?? DOCUMENTS (16 extensions)
Files for reading, editing, and data management.

| Extension | Description | Category |
|-----------|-------------|----------|
| .pdf | Portable Document Format | document |
| .doc | Microsoft Word (legacy) | document |
| .docx | Microsoft Word | document |
| .txt | Plain Text | document |
| .rtf | Rich Text Format | document |
| .odt | OpenDocument Text | document |
| .ppt | Microsoft PowerPoint (legacy) | document |
| .pptx | Microsoft PowerPoint | document |
| .xls | Microsoft Excel (legacy) | document |
| .xlsx | Microsoft Excel | document |
| .csv | Comma-Separated Values | document |
| .md | Markdown | document |
| .json | JSON Data | document |
| .xml | XML Data | document |
| .yaml | YAML Configuration | document |
| .yml | YAML Configuration (short) | document |

**Total: 16 document types**

---

### ??? IMAGES (11 extensions)
Visual content including photos, graphics, and icons.

| Extension | Description | Category |
|-----------|-------------|----------|
| .jpg | JPEG Image | image |
| .jpeg | JPEG Image (long) | image |
| .png | Portable Network Graphics | image |
| .webp | WebP Image | image |
| .gif | Graphics Interchange Format | image |
| .bmp | Bitmap Image | image |
| .tiff | Tagged Image File Format | image |
| .tif | TIFF (short) | image |
| .svg | Scalable Vector Graphics | image |
| .ico | Icon File | image |
| .heic | High Efficiency Image | image |

**Total: 11 image types**

---

### ?? VIDEOS (9 extensions)
Moving pictures and multimedia content.

| Extension | Description | Category |
|-----------|-------------|----------|
| .mp4 | MPEG-4 Video | video |
| .mov | QuickTime Movie | video |
| .mkv | Matroska Video | video |
| .webm | WebM Video | video |
| .avi | Audio Video Interleave | video |
| .wmv | Windows Media Video | video |
| .flv | Flash Video | video |
| .m4v | iTunes Video | video |
| .3gp | 3GPP Multimedia | video |

**Total: 9 video types**

---

### ?? OTHERS (13 extensions)
Archives, design files, 3D models, and specialized formats.

| Extension | Description | Category | Type |
|-----------|-------------|----------|------|
| .zip | ZIP Archive | others | Archive |
| .rar | RAR Archive | others | Archive |
| .7z | 7-Zip Archive | others | Archive |
| .tar | Tarball Archive | others | Archive |
| .gz | GZip Archive | others | Archive |
| .psd | Adobe Photoshop | others | Design |
| .ai | Adobe Illustrator | others | Design |
| .figma | Figma Design | others | Design |
| .blend | Blender 3D | others | 3D Model |
| .obj | 3D Object | others | 3D Model |
| .stl | Stereolithography | others | 3D Model |
| .log | Log File | others | System |
| .dat | Data File | others | Data |

**Total: 13 other types**

---

## Security - Forbidden Extensions

### ?? NOT ALLOWED (11 extensions)
These file types are **BLOCKED** for security reasons:

| Extension | Type | Reason |
|-----------|------|--------|
| .exe | Executable | Can run malicious code |
| .bat | Batch Script | Can execute commands |
| .cmd | Command Script | Can execute commands |
| .sh | Shell Script | Can execute commands |
| .ps1 | PowerShell Script | Can execute commands |
| .js | JavaScript | Can execute code |
| .vbs | VBScript | Can execute code |
| .jar | Java Archive | Can execute code |
| .php | PHP Script | Server-side code |
| .py | Python Script | Can execute code |
| .rb | Ruby Script | Can execute code |

**?? WARNING**: Any attempt to upload these file types will be rejected with a security error.

---

## Summary Statistics

| Category | Extensions | Percentage |
|----------|-----------|-----------|
| Documents | 16 | 32.7% |
| Images | 11 | 22.4% |
| Videos | 9 | 18.4% |
| Others | 13 | 26.5% |
| **Total Allowed** | **49** | **100%** |
| Forbidden | 11 | - |

---

## Validation Flow

```
???????????????????????????????????????
? User selects file                   ?
???????????????????????????????????????
              ?
              ?
???????????????????????????????????????
? Extract file extension              ?
???????????????????????????????????????
              ?
              ?
???????????????????????????????????????
? Check if FORBIDDEN                  ?
? (.exe, .bat, .cmd, etc.)           ?
???????????????????????????????????????
              ?
     YES ??????????? NO
      ?              ?
      ?              ?
????????????  ???????????????????????
? REJECT   ?  ? Check if ALLOWED    ?
? Security ?  ? (49 types)          ?
? Error    ?  ???????????????????????
????????????             ?
                YES ??????????? NO
                 ?              ?
                 ?              ?
         ????????????????  ????????????
         ? Check Size   ?  ? REJECT   ?
         ? (<50MB)      ?  ? Unsupported
         ????????????????  ????????????
                ?
       YES ??????????? NO
        ?              ?
        ?              ?
   ???????????   ????????????
   ? ACCEPT  ?   ? REJECT   ?
   ? Upload  ?   ? Too Large?
   ???????????   ????????????
```

---

## Error Messages

### Forbidden File
```
Security Error: Files with extension '.exe' are not allowed for security reasons. 
Executable and script files are forbidden.
```

### Unsupported File
```
Unsupported file type.
Supported file types:
DOCUMENT: .pdf, .doc, .docx, .txt, .rtf, .odt, .ppt, .pptx, .xls, .xlsx, .csv, .md, .json, .xml, .yaml, .yml
IMAGE: .jpg, .jpeg, .png, .webp, .gif, .bmp, .tiff, .tif, .svg, .ico, .heic
VIDEO: .mp4, .mov, .mkv, .webm, .avi, .wmv, .flv, .m4v, .3gp
OTHERS: .zip, .rar, .7z, .tar, .gz, .psd, .ai, .figma, .blend, .obj, .stl, .log, .dat
```

### File Too Large
```
File size exceeds maximum allowed size of 50 MB.
```

### Success
```
File uploaded successfully as document.
File uploaded successfully as image.
File uploaded successfully as video.
File uploaded successfully as others.
```

---

## Use Case Examples

### ? ALLOWED Examples

| File Name | Extension | Detected As | Result |
|-----------|-----------|-------------|--------|
| report.pdf | .pdf | document | ? Uploaded |
| photo.jpg | .jpg | image | ? Uploaded |
| video.mp4 | .mp4 | video | ? Uploaded |
| backup.zip | .zip | others | ? Uploaded |
| data.xlsx | .xlsx | document | ? Uploaded |
| presentation.pptx | .pptx | document | ? Uploaded |
| logo.svg | .svg | image | ? Uploaded |
| movie.mkv | .mkv | video | ? Uploaded |
| design.psd | .psd | others | ? Uploaded |
| config.yaml | .yaml | document | ? Uploaded |

### ? FORBIDDEN Examples

| File Name | Extension | Reason | Result |
|-----------|-----------|--------|--------|
| installer.exe | .exe | Executable | ? Blocked (Security) |
| script.bat | .bat | Batch script | ? Blocked (Security) |
| code.js | .js | JavaScript | ? Blocked (Security) |
| program.jar | .jar | Java executable | ? Blocked (Security) |
| malware.vbs | .vbs | VBScript | ? Blocked (Security) |
| hack.sh | .sh | Shell script | ? Blocked (Security) |
| server.php | .php | PHP script | ? Blocked (Security) |
| app.py | .py | Python script | ? Blocked (Security) |

### ? UNSUPPORTED Examples

| File Name | Extension | Reason | Result |
|-----------|-----------|--------|--------|
| song.mp3 | .mp3 | Not in allowed list | ? Rejected |
| archive.iso | .iso | Not in allowed list | ? Rejected |
| database.db | .db | Not in allowed list | ? Rejected |

---

## Implementation Details

### Code Location
- **Validation**: `Helpers/FileValidationHelper.cs`
- **Upload Handler**: `Pages/Dashboard.cshtml.cs` ? `OnPostUploadFileAsync()`
- **UI Display**: `Pages/Dashboard.cshtml`

### Key Methods

```csharp
// Check if extension is forbidden
FileValidationHelper.IsForbiddenExtension(fileName)

// Detect file category
FileValidationHelper.DetectFileType(fileName)

// Validate file size
FileValidationHelper.IsValidFileSize(fileSize)

// Get supported types message
FileValidationHelper.GetSupportedFileTypesMessage()
```

---

## Testing Checklist

### Documents (16 types)
- [ ] Upload .pdf ? document
- [ ] Upload .docx ? document
- [ ] Upload .txt ? document
- [ ] Upload .xlsx ? document
- [ ] Upload .pptx ? document
- [ ] Upload .json ? document
- [ ] Upload .xml ? document
- [ ] Upload .yaml ? document

### Images (11 types)
- [ ] Upload .jpg ? image
- [ ] Upload .png ? image
- [ ] Upload .gif ? image
- [ ] Upload .svg ? image
- [ ] Upload .webp ? image

### Videos (9 types)
- [ ] Upload .mp4 ? video
- [ ] Upload .mkv ? video
- [ ] Upload .avi ? video
- [ ] Upload .webm ? video

### Others (13 types)
- [ ] Upload .zip ? others
- [ ] Upload .rar ? others
- [ ] Upload .psd ? others
- [ ] Upload .blend ? others

### Security (11 forbidden)
- [ ] Upload .exe ? Security Error
- [ ] Upload .bat ? Security Error
- [ ] Upload .js ? Security Error
- [ ] Upload .jar ? Security Error
- [ ] Upload .php ? Security Error

### Edge Cases
- [ ] Upload 60MB file ? Size error
- [ ] Upload .unknown ? Unsupported error
- [ ] Upload without extension ? Error

---

## Compliance & Security

### Security Principles
? **Whitelist approach** - Only explicitly allowed extensions  
? **Blacklist enforcement** - Forbidden extensions blocked first  
? **Size limits** - 50MB maximum  
? **Extension validation** - Case-insensitive checking  
? **Double-check** - Both extension and detection validated  

### Best Practices
? **No executables** - Cannot run code on server  
? **No scripts** - Cannot execute commands  
? **Clear errors** - Users know why file rejected  
? **Comprehensive list** - 49 common file types supported  
? **Future-proof** - Easy to add new types  

---

## Conclusion

The file upload system supports **49 different file types** across 4 categories while blocking **11 dangerous file types** for security. This provides a balance between functionality and security.

**Status**: ? Production Ready  
**Last Updated**: Today  
**Total Supported Extensions**: 49  
**Total Forbidden Extensions**: 11  
**Maximum File Size**: 50 MB

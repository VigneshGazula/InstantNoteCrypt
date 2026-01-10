# ?? CodeSafe - Secure Note & File Storage Platform

<div align="center">

![CodeSafe Banner](https://img.shields.io/badge/CodeSafe-Secure%20Storage-7c7cff?style=for-the-badge)
[![.NET](https://img.shields.io/badge/.NET-8.0-512BD4?style=for-the-badge&logo=dotnet)](https://dotnet.microsoft.com/)
[![ASP.NET Core](https://img.shields.io/badge/ASP.NET%20Core-Razor%20Pages-512BD4?style=for-the-badge)](https://docs.microsoft.com/en-us/aspnet/core/)
[![License](https://img.shields.io/badge/License-MIT-green?style=for-the-badge)](LICENSE)

**A zero-knowledge, end-to-end encrypted platform for secure note and file storage**

[Features](#-features) • [Demo](#-demo) • [Installation](#-installation) • [Usage](#-usage) • [Security](#-security) • [Tech Stack](#-tech-stack)

</div>

---

## ?? About

**CodeSafe** is a privacy-first web application that allows users to securely store notes and files using unique access codes. Built with a **zero-knowledge architecture**, the platform ensures that only users with the correct code (and optional PIN) can access their data. No user accounts required, no personal information tracked.

### ? Key Highlights

- ?? **Zero-Knowledge Architecture** - Your data is encrypted; we can't access it
- ?? **No User Accounts** - Access everything with just a code
- ?? **Optional PIN Protection** - Add an extra layer of security
- ?? **Multi-Format Support** - Store documents, images, videos, and more
- ?? **Cloud Storage** - Files stored securely on Cloudinary
- ?? **Modern UI** - Beautiful dark glassmorphism design
- ??? **Server-Side Security** - PIN enforcement at the server level

---

## ?? Features

### ?? Code-Based Access System

- **Unique Access Codes**: Each note/storage space is accessed via a custom code (e.g., `mysecret123`)
- **No Registration Required**: Instant access without creating accounts
- **Code Privacy**: Codes are never exposed or logged

### ?? Advanced Security

#### PIN Protection
- Set optional PIN for additional security
- Server-side PIN verification (no client-side bypass)
- Session-based authentication (30-minute timeout)
- PIN update and removal features
- **Protected Operations**:
  - View dashboard
  - Save notes
  - Upload files
  - Download files
  - Delete files
  - Modify content

#### Security Features
- ? End-to-end encryption for PINs
- ? Server-side authorization checks
- ? URL manipulation prevention
- ? Session-based access control
- ? Secure cookie handling (HttpOnly, Secure)
- ? HTTPS enforcement

### ?? Note Management

- **Rich Text Notes**: Store and edit text content
- **Character Counter**: Real-time character count
- **Auto-Save**: Save notes with a single click
- **Encrypted Storage**: All notes are stored securely

### ?? File Upload & Management

#### Supported File Types

**?? Documents**
```
PDF, DOC, DOCX, TXT, RTF, ODT, PPT, PPTX, XLS, XLSX, CSV, MD, JSON, XML, YAML
```

**??? Images**
```
JPG, JPEG, PNG, WEBP, GIF, BMP, TIFF, SVG, ICO, HEIC
```

**?? Videos**
```
MP4, MOV, MKV, WEBM, AVI, WMV, FLV, M4V, 3GP
```

**?? Others**
```
ZIP, RAR, 7Z, TAR, GZ, PSD, AI, Figma, Blend, OBJ, STL, LOG, DAT
```

#### File Features
- **Automatic Type Detection**: Files are automatically categorized
- **50MB File Size Limit**: Supports large files
- **Cloud Storage**: Files stored on Cloudinary CDN
- **Organized Categories**: Separate tabs for documents, photos, videos, and others
- **Download Protection**: PIN verification required for downloads
- **Bulk Management**: Load and manage files by type

#### Security Restrictions
?? **Forbidden File Types** (for security):
```
.exe, .bat, .cmd, .sh, .ps1, .js, .vbs, .jar, .php, .py, .rb
```

### ?? User Interface

- **Dark Glassmorphism Design**: Modern, elegant UI
- **Responsive Layout**: Works on desktop, tablet, and mobile
- **Tab-Based Navigation**: Easy access to different sections
- **Animated Transitions**: Smooth user experience
- **Real-Time Feedback**: Success/error messages
- **Loading Indicators**: Upload progress feedback

### ??? Dashboard Features

#### Organized Tabs
1. **?? Notes** - Write and save encrypted notes
2. **?? Documents** - Upload and manage documents
3. **??? Photos** - Store and organize images
4. **?? Videos** - Upload video files
5. **?? Others** - Miscellaneous files
6. **?? Security** - PIN management and security settings

#### File Operations
- **Upload**: Drag-and-drop or click to upload
- **Download**: Secure direct download links
- **Delete**: Remove individual files
- **Load**: View files by category

### ??? Data Protection

- **Encrypted PINs**: PINs are encrypted using .NET Data Protection
- **Secure Sessions**: Server-side session storage
- **No Data Tracking**: Zero knowledge of user content
- **Self-Destruct**: Permanently delete notes and all files
- **Privacy First**: No logs, no tracking, no monitoring

---

## ?? Demo

### Home Page
![Home Page](https://via.placeholder.com/800x400?text=CodeSafe+Home+Page)

### Dashboard
![Dashboard](https://via.placeholder.com/800x400?text=CodeSafe+Dashboard)

### PIN Protection
![PIN Verification](https://via.placeholder.com/800x400?text=PIN+Verification)

---

## ?? Tech Stack

### Backend
- **Framework**: ASP.NET Core 8.0
- **Architecture**: Razor Pages
- **Database**: SQL Server
- **ORM**: Entity Framework Core
- **Cloud Storage**: Cloudinary

### Frontend
- **UI Framework**: Razor Pages
- **Styling**: Custom CSS (Glassmorphism)
- **JavaScript**: Vanilla JS (minimal)
- **Icons**: Unicode Emojis

### Security
- **Encryption**: .NET Data Protection API
- **Sessions**: ASP.NET Core Session Middleware
- **Authentication**: Custom authorization helper
- **Storage**: Server-side session state

### Infrastructure
- **File Storage**: Cloudinary CDN
- **Database**: SQL Server
- **Deployment**: Azure (or any ASP.NET host)

---

## ?? Installation

### Prerequisites

- [.NET 8.0 SDK](https://dotnet.microsoft.com/download)
- [SQL Server](https://www.microsoft.com/en-us/sql-server/sql-server-downloads) (or SQL Server Express)
- [Cloudinary Account](https://cloudinary.com/) (free tier available)

### Step 1: Clone the Repository

```bash
git clone https://github.com/VigneshGazula/InstantNoteCrypt.git
cd InstantNoteCrypt/ShareItems_WebApp
```

### Step 2: Configure Cloudinary

1. Sign up for a free Cloudinary account at [cloudinary.com](https://cloudinary.com/)
2. Get your Cloud Name, API Key, and API Secret
3. Update `appsettings.json`:

```json
{
  "CloudinarySettings": {
    "CloudName": "your-cloud-name",
    "ApiKey": "your-api-key",
    "ApiSecret": "your-api-secret"
  }
}
```

### Step 3: Configure Database

Update the connection string in `appsettings.json`:

```json
{
  "ConnectionStrings": {
    "DefaultConnectionString": "Server=(localdb)\\mssqllocaldb;Database=CodeSafeDb;Trusted_Connection=True;MultipleActiveResultSets=true"
  }
}
```

Or set environment variable:

```bash
# Windows
set DB_CONNECTION="Your-Connection-String"

# Linux/Mac
export DB_CONNECTION="Your-Connection-String"
```

### Step 4: Run Migrations

```bash
dotnet ef database update
```

### Step 5: Run the Application

```bash
dotnet run
```

The application will be available at `https://localhost:5001`

---

## ?? Usage

### Creating a Note

1. **Visit the Home Page**
2. **Enter a Unique Code** (e.g., `mysecret123`)
3. **Click "Unlock Note"**
4. **Start adding content!**

### Setting Up PIN Protection

1. Navigate to **?? Security** tab
2. Click **Set PIN Protection**
3. Enter a PIN (minimum 4 characters)
4. Confirm the PIN
5. Click **Lock Note**

### Uploading Files

1. Go to the appropriate tab:
   - **?? Documents** for PDFs, Word files, etc.
   - **??? Photos** for images
   - **?? Videos** for video files
   - **?? Others** for archives and other files

2. Click **Choose File** or drag-and-drop
3. Click **Upload**
4. File is automatically categorized and uploaded to cloud storage

### Downloading Files

1. Click **Load Files** in the appropriate tab
2. Click the **?? Download** icon next to any file
3. If PIN is set, you'll be prompted to verify it

### Managing Security

#### Update PIN
1. Go to **?? Security** tab
2. Enter current PIN
3. Enter new PIN
4. Confirm new PIN
5. Click **Update PIN**

#### Remove PIN
1. Go to **?? Security** tab
2. Enter current PIN
3. Click **Unlock Note**

#### Destroy Note
?? **Warning: This action is permanent!**

1. Go to **?? Security** tab
2. Scroll to **Danger Zone**
3. Enter PIN (if set)
4. Click **Destroy Note**
5. All notes and files will be permanently deleted

---

## ?? Security

### Server-Side PIN Enforcement

CodeSafe implements **enterprise-grade server-side security**:

- ? **Every route is protected** - No client-side bypass possible
- ? **Session-based verification** - Stored server-side only
- ? **URL manipulation blocked** - Direct access redirects to PIN page
- ? **Download protection** - Files require PIN verification
- ? **Secure sessions** - HttpOnly, Secure cookies with 30-min timeout

### Security Architecture

```
User Request
    ?
Authorization Helper (Server-Side)
    ?
PIN Required?
    ?
Session Verified? ? YES ? Access Granted
    ?
    NO
    ?
Redirect to PIN Verification
```

### Data Encryption

- **PINs**: Encrypted using .NET Data Protection API
- **Storage**: Secure encryption at rest
- **Transport**: HTTPS enforced
- **Sessions**: Server-side only, never client-side

### Privacy Guarantees

- ?? **No user accounts** - Anonymous access
- ?? **No tracking** - Zero analytics or monitoring
- ?? **No logs** - Content never logged
- ?? **No backdoors** - True zero-knowledge architecture

---

## ??? Architecture

### Project Structure

```
ShareItems_WebApp/
??? Controllers/          # API controllers (if any)
??? Entities/            # Database models
?   ??? Note.cs
?   ??? NoteFile.cs
?   ??? UserContext.cs
??? Helpers/             # Utility helpers
?   ??? FileValidationHelper.cs
?   ??? NoteAuthorizationHelper.cs
??? Migrations/          # EF Core migrations
??? Pages/               # Razor Pages
?   ??? Dashboard.cshtml
?   ??? Dashboard.cshtml.cs
?   ??? Index.cshtml
?   ??? Index.cshtml.cs
?   ??? VerifyPin.cshtml
?   ??? VerifyPin.cshtml.cs
?   ??? Shared/
?       ??? _Layout.cshtml
?       ??? _FilesList.cshtml
??? Services/            # Business logic services
?   ??? INoteService.cs
?   ??? NoteService.cs
?   ??? IFileStorageService.cs
?   ??? FileStorageService.cs
?   ??? ICloudinaryService.cs
?   ??? CloudinaryService.cs
?   ??? IEncryptionService.cs
??? Settings/            # Configuration classes
?   ??? CloudinarySettings.cs
??? wwwroot/             # Static files
?   ??? global-theme.css
?   ??? site.css
??? appsettings.json     # Configuration
??? Program.cs           # Application entry point
```

### Database Schema

**Notes Table**
```sql
CREATE TABLE Notes (
    Id INT PRIMARY KEY IDENTITY,
    Code NVARCHAR(255) UNIQUE NOT NULL,
    Content NVARCHAR(MAX),
    Pin NVARCHAR(255),  -- Encrypted
    CreatedAt DATETIME2 NOT NULL,
    UpdatedAt DATETIME2 NOT NULL
)
```

**NoteFiles Table**
```sql
CREATE TABLE NoteFiles (
    Id INT PRIMARY KEY IDENTITY,
    NoteId INT FOREIGN KEY REFERENCES Notes(Id),
    FileName NVARCHAR(255) NOT NULL,
    FileType NVARCHAR(50) NOT NULL,  -- document/image/video/others
    FileSize BIGINT NOT NULL,
    FileUrl NVARCHAR(MAX) NOT NULL,  -- Cloudinary URL
    CloudinaryPublicId NVARCHAR(255),
    UploadedAt DATETIME2 NOT NULL
)
```

---

## ?? Configuration

### Environment Variables

```bash
# Database Connection (optional, overrides appsettings.json)
DB_CONNECTION="Server=...;Database=...;..."

# Cloudinary (configured in appsettings.json)
```

### appsettings.json

```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning"
    }
  },
  "AllowedHosts": "*",
  "ConnectionStrings": {
    "DefaultConnectionString": "Your-SQL-Server-Connection-String"
  },
  "CloudinarySettings": {
    "CloudName": "your-cloudinary-cloud-name",
    "ApiKey": "your-cloudinary-api-key",
    "ApiSecret": "your-cloudinary-api-secret"
  }
}
```

### Session Configuration

Sessions are configured in `Program.cs`:

```csharp
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);  // 30-minute timeout
    options.Cookie.HttpOnly = true;                  // Prevent JavaScript access
    options.Cookie.IsEssential = true;               // GDPR compliance
    options.Cookie.SecurePolicy = CookieSecurePolicy.Always; // HTTPS only
});
```

---

## ?? Testing

### Manual Testing Checklist

- [ ] Create note with unique code
- [ ] Save note content
- [ ] Upload document file
- [ ] Upload image file
- [ ] Upload video file
- [ ] Set PIN protection
- [ ] Verify PIN requirement on access
- [ ] Download file with PIN
- [ ] Update PIN
- [ ] Remove PIN
- [ ] Delete individual file
- [ ] Destroy entire note

### Security Testing

- [ ] Attempt direct dashboard access without PIN
- [ ] Try URL manipulation to bypass PIN
- [ ] Test file download without verification
- [ ] Verify session timeout (30 min)
- [ ] Test forbidden file type upload
- [ ] Verify file size limits

---

## ?? Performance

- **File Upload**: Up to 50MB per file
- **Session Timeout**: 30 minutes of inactivity
- **Storage**: Unlimited notes (database dependent)
- **CDN**: Cloudinary global CDN for fast file delivery
- **Scalability**: Horizontal scaling supported

---

## ?? Contributing

Contributions are welcome! Please follow these steps:

1. Fork the repository
2. Create a feature branch (`git checkout -b feature/AmazingFeature`)
3. Commit your changes (`git commit -m 'Add some AmazingFeature'`)
4. Push to the branch (`git push origin feature/AmazingFeature`)
5. Open a Pull Request

### Development Guidelines

- Follow C# coding conventions
- Add XML documentation for public methods
- Update README if adding new features
- Test thoroughly before submitting PR
- Maintain security best practices

---

## ?? License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.

---

## ????? Author

**Vignesh Gazula**

- GitHub: [@VigneshGazula](https://github.com/VigneshGazula)
- Repository: [InstantNoteCrypt](https://github.com/VigneshGazula/InstantNoteCrypt)

---

## ?? Acknowledgments

- [ASP.NET Core](https://docs.microsoft.com/en-us/aspnet/core/) - Web framework
- [Entity Framework Core](https://docs.microsoft.com/en-us/ef/core/) - ORM
- [Cloudinary](https://cloudinary.com/) - Cloud storage
- [SQL Server](https://www.microsoft.com/en-us/sql-server/) - Database

---

## ?? Support

If you encounter any issues or have questions:

1. Check the [Issues](https://github.com/VigneshGazula/InstantNoteCrypt/issues) page
2. Create a new issue with detailed information
3. Provide error logs and steps to reproduce

---

## ??? Roadmap

### Planned Features

- [ ] **Expiration Dates**: Auto-delete notes after specified time
- [ ] **Note Sharing**: Share encrypted notes with others
- [ ] **Multi-Language Support**: Internationalization
- [ ] **File Previews**: In-app preview for images and PDFs
- [ ] **Bulk Upload**: Upload multiple files at once
- [ ] **Search**: Search within notes and files
- [ ] **Tags**: Categorize notes with tags
- [ ] **Export**: Download all data as ZIP
- [ ] **2FA**: Two-factor authentication support
- [ ] **API**: RESTful API for external integrations

---

## ?? Disclaimer

**User Responsibility Notice:**

- Users are solely responsible for the content they upload or store on this platform
- The developers do not access, monitor, or control user data
- Not responsible for any data loss or information leaks caused by user actions
- Users are strongly advised to enable PIN protection for sensitive content
- Do not upload illegal, harmful, or confidential data you are not authorized to store

---

## ?? Screenshots

### ?? Home Page
Clean, modern interface with code entry

### ?? Dashboard
Organized tabs for different content types

### ?? PIN Protection
Secure access control

### ?? File Management
Easy upload and download with automatic categorization

---

<div align="center">

**Built with ?? using ASP.NET Core 8.0**

? **Star this repository if you find it helpful!** ?

</div>

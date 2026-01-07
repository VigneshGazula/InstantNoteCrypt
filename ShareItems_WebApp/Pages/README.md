# InstantNoteCrypt - Test UI Pages

## Overview
Simple Razor Pages UI created for testing the note and file upload/download functionality.

## Pages

### 1. Index Page (/)
- **Route**: `/` or `/Index`
- **Purpose**: Entry point to access or create a note
- **Usage**:
  - Enter a Code
  - Click "Access Note"
  - If code exists:
    - **Locked note**: Redirects to PIN verification page
    - **Unlocked note**: Opens dashboard directly
  - If code doesn't exist: Creates new note and opens dashboard

### 2. Verify PIN Page (/VerifyPin/{code})
- **Route**: `/VerifyPin/{code}`
- **Purpose**: Authenticate user for locked notes
- **Behavior**:
  - Displays lock icon and code
  - Prompts for PIN entry
  - Validates PIN against encrypted stored value
  - On success: Redirects to dashboard
  - On failure: Shows error message
  - If note has no PIN: Auto-redirects to dashboard
- **Navigation**: Go Back to Home link available

### 3. Dashboard Page (/Dashboard/{code})
- **Route**: `/Dashboard/{code}`
- **Purpose**: Manage note content and files

#### Features:

**Navigation**
- Go Back button to return to home page

**Safety Lock (PIN Protection)**
- Set a PIN to lock the note (minimum 4 characters)
- Update PIN (requires current PIN validation)
- Remove PIN to unlock the note (requires current PIN)
- PIN required for accessing locked notes
- Visual indicator shows lock status (?? Locked / ?? Unlocked)

**Note Management**
- View and edit note content
- Save changes to note

**File Upload**
- Upload files with automatic type detection
- File type automatically determined by extension
- Supported types:
  - **Documents**: .pdf, .doc, .docx
  - **Images**: .jpg, .jpeg, .png, .webp
  - **Videos**: .mp4, .mov
  - **Others**: .zip, .rar, .7z, .txt, .csv, .xlsx, .xls, .pptx, .ppt
- Maximum file size: 50MB
- Validation includes file size and extension checks

**File Access**
- View files by category (Documents, Photos, Videos, Others)
- Download files

**Destroy Note (Danger Zone)**
- Permanently delete note and all associated files
- PIN validation required if note is locked
- Confirmation prompt for unlocked notes
- Cannot be undone - redirects to home after deletion

## How to Use

1. Run the application
2. Navigate to the Index page
3. Enter a code (e.g., "test123")
4. Access the dashboard
5. Test the following:
- ? Enter code for locked note ? PIN verification required
- ? Enter code for unlocked note ? Direct dashboard access
- ? Create new note ? Direct dashboard access
- ? Save note content
- ? Set PIN to lock note
- ? Update PIN (requires current PIN)
- ? Remove PIN to unlock note
- ? Upload files (select file and type)
- ? View files by category
- ? Download files
- ? Go back to home page
- ? Destroy note (with PIN validation if locked)

## Technical Notes

- No styling applied (basic HTML only)
- All handlers use async/await
- File types: document, image, video
- Files stored with encryption via FileStorageService
- Downloads return original file with correct content type

## Handlers

**Index Page**
- `OnGet()` - Display form and show success messages
- `OnPostAsync()` - Create/access note, check for PIN, redirect accordingly

**Verify PIN Page**
- `OnGetAsync()` - Display PIN entry form (or redirect if no PIN)
- `OnPostAsync()` - Validate PIN and grant access

**Dashboard Page**
- `OnGetAsync()` - Load note
- `OnPostSaveNoteAsync()` - Save note content
- `OnPostSetPinAsync()` - Set PIN to lock note
- `OnPostUpdatePinAsync()` - Update existing PIN
- `OnPostRemovePinAsync()` - Remove PIN from note
- `OnPostUploadFileAsync()` - Upload file
- `OnPostLoadFilesAsync()` - Filter files by type
- `OnGetDownloadAsync()` - Download file by ID
- `OnPostDestroyNoteAsync()` - Permanently delete note and all files

using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using ShareItems_WebApp.Entities;
using ShareItems_WebApp.Services;
using ShareItems_WebApp.Helpers;

namespace ShareItems_WebApp.Pages
{
    public class DashboardModel : PageModel
    {
        private readonly INoteService _noteService;
        private readonly IFileStorageService _fileStorageService;
        private readonly NoteAuthorizationHelper _authHelper;

        public DashboardModel(
            INoteService noteService, 
            IFileStorageService fileStorageService,
            NoteAuthorizationHelper authHelper)
        {
            _noteService = noteService;
            _fileStorageService = fileStorageService;
            _authHelper = authHelper;
        }

        [BindProperty(SupportsGet = true)]
        public string Code { get; set; } = string.Empty;

        [BindProperty]
        public string? CurrentFileType { get; set; }

        [BindProperty]
        public string? ActiveTab { get; set; }

        public string? NoteContent { get; set; }
        public int NoteId { get; set; }
        public bool HasPin { get; set; }
        public IEnumerable<NoteFile>? Files { get; set; }
        public string? Message { get; set; }
        public string? ErrorMessage { get; set; }

        public async Task<IActionResult> OnGetAsync()
        {
            // CRITICAL: Enforce PIN protection
            var authResult = await _authHelper.ValidateNoteAccessAsync(Code);
            
            if (!authResult.IsAuthorized)
            {
                if (authResult.NeedsPinVerification)
                {
                    return RedirectToPage("/VerifyPin", new { code = Code });
                }
                return RedirectToPage("/Index");
            }

            var note = authResult.Note!;
            NoteId = note.Id;
            NoteContent = note.Content;
            HasPin = !string.IsNullOrWhiteSpace(note.Pin);

            return Page();
        }

        public async Task<IActionResult> OnPostSaveNoteAsync(string content)
        {
            // CRITICAL: Enforce PIN protection
            var authResult = await _authHelper.ValidateNoteAccessAsync(Code);
            
            if (!authResult.IsAuthorized)
            {
                if (authResult.NeedsPinVerification)
                {
                    return RedirectToPage("/VerifyPin", new { code = Code });
                }
                return RedirectToPage("/Index");
            }

            var note = authResult.Note!;
            NoteId = note.Id;
            await _noteService.UpdateNoteContentAsync(note.Id, content ?? string.Empty);
            NoteContent = content;
            HasPin = !string.IsNullOrWhiteSpace(note.Pin);
            Message = "Note saved successfully.";

            // Preserve active tab (defaults to notes if not specified)
            if (string.IsNullOrEmpty(ActiveTab))
            {
                ActiveTab = "notes";
            }

            return Page();
        }

        public async Task<IActionResult> OnPostUploadFileAsync(IFormFile file)
        {
            // CRITICAL: Enforce PIN protection
            var authResult = await _authHelper.ValidateNoteAccessAsync(Code);
            
            if (!authResult.IsAuthorized)
            {
                if (authResult.NeedsPinVerification)
                {
                    return RedirectToPage("/VerifyPin", new { code = Code });
                }
                return RedirectToPage("/Index");
            }

            var note = authResult.Note!;
            NoteId = note.Id;
            NoteContent = note.Content;
            HasPin = !string.IsNullOrWhiteSpace(note.Pin);

            if (file == null || file.Length == 0)
            {
                ErrorMessage = "Please select a file.";
                return Page();
            }

            // Security check: Block forbidden extensions
            if (FileValidationHelper.IsForbiddenExtension(file.FileName))
            {
                var extension = Path.GetExtension(file.FileName);
                ErrorMessage = $"Security Error: Files with extension '{extension}' are not allowed for security reasons. Executable and script files are forbidden.";
                return Page();
            }

            // Automatically detect file type based on extension
            var detectedFileType = FileValidationHelper.DetectFileType(file.FileName);

            if (string.IsNullOrWhiteSpace(detectedFileType))
            {
                ErrorMessage = $"Unsupported file type. {FileValidationHelper.GetSupportedFileTypesMessage()}";
                return Page();
            }

            // Validate file size
            if (!FileValidationHelper.IsValidFileSize(file.Length))
            {
                ErrorMessage = $"File size exceeds maximum allowed size of {FileValidationHelper.GetFileSizeString(FileValidationHelper.MaxFileSize)}.";
                return Page();
            }

            try
            {
                await _fileStorageService.SaveFileAsync(note.Id, file, detectedFileType, Code);
                Message = $"File uploaded successfully as {detectedFileType}.";
                CurrentFileType = detectedFileType;
                
                // Set active tab based on file type
                ActiveTab = detectedFileType switch
                {
                    "document" => "documents",
                    "image" => "photos",
                    "video" => "videos",
                    _ => "others"
                };
                
                // Load files for the current type
                Files = await _fileStorageService.GetFilesByNoteIdAndTypeAsync(note.Id, detectedFileType);
            }
            catch (TaskCanceledException)
            {
                ErrorMessage = "Upload timeout - the file is too large or your connection is too slow. Please try a smaller file.";
            }
            catch (Exception ex)
            {
                // Check if it's a timeout-related exception
                if (ex.Message.Contains("timeout", StringComparison.OrdinalIgnoreCase) || 
                    ex.Message.Contains("canceled", StringComparison.OrdinalIgnoreCase))
                {
                    ErrorMessage = "Upload timeout - please try with a smaller file or check your internet connection.";
                }
                else if (ex.Message.Contains("cloud storage", StringComparison.OrdinalIgnoreCase))
                {
                    ErrorMessage = "Failed to upload to cloud storage. Please try again later.";
                }
                else
                {
                    ErrorMessage = $"Upload failed: {ex.Message}";
                }
            }

            return Page();
        }

        public async Task<IActionResult> OnPostLoadFilesAsync(string fileType)
        {
            // CRITICAL: Enforce PIN protection
            var authResult = await _authHelper.ValidateNoteAccessAsync(Code);
            
            if (!authResult.IsAuthorized)
            {
                if (authResult.NeedsPinVerification)
                {
                    return RedirectToPage("/VerifyPin", new { code = Code });
                }
                return RedirectToPage("/Index");
            }

            var note = authResult.Note!;
            NoteId = note.Id;
            NoteContent = note.Content;
            HasPin = !string.IsNullOrWhiteSpace(note.Pin);
            CurrentFileType = fileType;

            // Set active tab based on file type
            ActiveTab = fileType switch
            {
                "document" => "documents",
                "image" => "photos",
                "video" => "videos",
                "others" => "others",
                _ => "notes"
            };

            if (string.IsNullOrWhiteSpace(fileType))
            {
                Files = await _fileStorageService.GetFilesByNoteIdAsync(note.Id);
            }
            else
            {
                Files = await _fileStorageService.GetFilesByNoteIdAndTypeAsync(note.Id, fileType);
            }

            return Page();
        }

        public async Task<IActionResult> OnGetDownloadAsync(int fileId)
        {
            // CRITICAL: Get file first to determine which note it belongs to
            var file = await _fileStorageService.GetFileByIdAsync(fileId);

            if (file == null)
            {
                return RedirectToPage("/Index");
            }

            // Get the note for this file
            var note = await _noteService.GetNoteByIdAsync(file.NoteId);
            
            if (note == null)
            {
                return RedirectToPage("/Index");
            }

            // CRITICAL: Enforce PIN protection using the note's code
            var authResult = await _authHelper.ValidateNoteAccessAsync(note.Code);
            
            if (!authResult.IsAuthorized)
            {
                if (authResult.NeedsPinVerification)
                {
                    return RedirectToPage("/VerifyPin", new { code = note.Code });
                }
                return RedirectToPage("/Index");
            }

            // Redirect to Cloudinary secure URL for download
            return Redirect(file.FileUrl);
        }

        public async Task<IActionResult> OnPostSetPinAsync(string pin, string confirmPin)
        {
            // CRITICAL: Enforce PIN protection
            var authResult = await _authHelper.ValidateNoteAccessAsync(Code);
            
            if (!authResult.IsAuthorized)
            {
                if (authResult.NeedsPinVerification)
                {
                    return RedirectToPage("/VerifyPin", new { code = Code });
                }
                return RedirectToPage("/Index");
            }

            var note = authResult.Note!;
            NoteId = note.Id;
            NoteContent = note.Content;
            HasPin = !string.IsNullOrWhiteSpace(note.Pin);

            if (string.IsNullOrWhiteSpace(pin) || string.IsNullOrWhiteSpace(confirmPin))
            {
                ErrorMessage = "PIN and Confirm PIN are required.";
                return Page();
            }

            if (pin != confirmPin)
            {
                ErrorMessage = "PINs do not match.";
                return Page();
            }

            if (pin.Length < 4)
            {
                ErrorMessage = "PIN must be at least 4 characters.";
                return Page();
            }

            try
            {
                await _noteService.SetNotePinAsync(note.Id, pin);
                HasPin = true;
                Message = "PIN set successfully. Note is now locked.";
                
                // Mark the new PIN as verified in session
                _authHelper.MarkPinAsVerified(Code);
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Failed to set PIN: {ex.Message}";
            }

            // Stay on security tab
            ActiveTab = "security";

            return Page();
        }

        public async Task<IActionResult> OnPostRemovePinAsync(string removePin)
        {
            // CRITICAL: Enforce PIN protection
            var authResult = await _authHelper.ValidateNoteAccessAsync(Code);
            
            if (!authResult.IsAuthorized)
            {
                if (authResult.NeedsPinVerification)
                {
                    return RedirectToPage("/VerifyPin", new { code = Code });
                }
                return RedirectToPage("/Index");
            }

            var note = authResult.Note!;
            NoteId = note.Id;
            NoteContent = note.Content;
            HasPin = !string.IsNullOrWhiteSpace(note.Pin);

            if (string.IsNullOrWhiteSpace(removePin))
            {
                ErrorMessage = "Please enter the current PIN.";
                return Page();
            }

            var isValid = await _noteService.ValidatePinAsync(note.Id, removePin);

            if (!isValid)
            {
                ErrorMessage = "Invalid PIN. Cannot unlock note.";
                return Page();
            }

            try
            {
                await _noteService.RemoveNotePinAsync(note.Id);
                HasPin = false;
                Message = "PIN removed successfully. Note is now unlocked.";
                
                // Clear PIN verification from session
                _authHelper.ClearPinVerification(Code);
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Failed to remove PIN: {ex.Message}";
            }

            // Stay on security tab
            ActiveTab = "security";

            return Page();
        }

        public async Task<IActionResult> OnPostUpdatePinAsync(string currentPin, string newPin, string confirmNewPin)
        {
            // CRITICAL: Enforce PIN protection
            var authResult = await _authHelper.ValidateNoteAccessAsync(Code);
            
            if (!authResult.IsAuthorized)
            {
                if (authResult.NeedsPinVerification)
                {
                    return RedirectToPage("/VerifyPin", new { code = Code });
                }
                return RedirectToPage("/Index");
            }

            var note = authResult.Note!;
            NoteId = note.Id;
            NoteContent = note.Content;
            HasPin = !string.IsNullOrWhiteSpace(note.Pin);

            if (string.IsNullOrWhiteSpace(currentPin) || string.IsNullOrWhiteSpace(newPin) || string.IsNullOrWhiteSpace(confirmNewPin))
            {
                ErrorMessage = "All fields are required.";
                return Page();
            }

            // Validate current PIN
            var isValid = await _noteService.ValidatePinAsync(note.Id, currentPin);

            if (!isValid)
            {
                ErrorMessage = "Current PIN is incorrect.";
                return Page();
            }

            // Check if new PINs match
            if (newPin != confirmNewPin)
            {
                ErrorMessage = "New PINs do not match.";
                return Page();
            }

            // Check new PIN length
            if (newPin.Length < 4)
            {
                ErrorMessage = "New PIN must be at least 4 characters.";
                return Page();
            }

            try
            {
                await _noteService.SetNotePinAsync(note.Id, newPin);
                Message = "PIN updated successfully.";
                
                // PIN is updated but session verification remains valid
                _authHelper.MarkPinAsVerified(Code);
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Failed to update PIN: {ex.Message}";
            }

            // Stay on security tab
            ActiveTab = "security";

            return Page();
        }

        public async Task<IActionResult> OnPostDestroyNoteAsync(string? destroyPin)
        {
            // CRITICAL: Enforce PIN protection
            var authResult = await _authHelper.ValidateNoteAccessAsync(Code);
            
            if (!authResult.IsAuthorized)
            {
                if (authResult.NeedsPinVerification)
                {
                    return RedirectToPage("/VerifyPin", new { code = Code });
                }
                return RedirectToPage("/Index");
            }

            var note = authResult.Note!;
            NoteId = note.Id;
            NoteContent = note.Content;
            HasPin = !string.IsNullOrWhiteSpace(note.Pin);

            // If note has PIN, validate it
            if (HasPin)
            {
                if (string.IsNullOrWhiteSpace(destroyPin))
                {
                    ErrorMessage = "Please enter the PIN to destroy this note.";
                    return Page();
                }

                var isValid = await _noteService.ValidatePinAsync(note.Id, destroyPin);

                if (!isValid)
                {
                    ErrorMessage = "Invalid PIN. Cannot destroy note.";
                    return Page();
                }
            }

            try
            {
                var deleted = await _noteService.DeleteNoteAsync(note.Id);

                if (deleted)
                {
                    // Clear session verification
                    _authHelper.ClearPinVerification(Code);
                    
                    TempData["Message"] = "Note and all associated files have been permanently deleted.";
                    return RedirectToPage("/Index");
                }
                else
                {
                    ErrorMessage = "Failed to delete note.";
                    return Page();
                }
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Failed to destroy note: {ex.Message}";
                return Page();
            }
        }

        public async Task<IActionResult> OnPostDeleteFileAsync(int fileId)
        {
            // CRITICAL: Enforce PIN protection
            var authResult = await _authHelper.ValidateNoteAccessAsync(Code);
            
            if (!authResult.IsAuthorized)
            {
                if (authResult.NeedsPinVerification)
                {
                    return RedirectToPage("/VerifyPin", new { code = Code });
                }
                return RedirectToPage("/Index");
            }

            var note = authResult.Note!;
            NoteId = note.Id;
            NoteContent = note.Content;
            HasPin = !string.IsNullOrWhiteSpace(note.Pin);

            try
            {
                var deleted = await _fileStorageService.DeleteFileAsync(fileId);

                if (deleted)
                {
                    Message = "File deleted successfully.";
                }
                else
                {
                    ErrorMessage = "Failed to delete file.";
                }
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Failed to delete file: {ex.Message}";
            }

            // Reload files to refresh the list
            if (!string.IsNullOrWhiteSpace(CurrentFileType))
            {
                Files = await _fileStorageService.GetFilesByNoteIdAndTypeAsync(note.Id, CurrentFileType);
                
                // Set active tab based on current file type
                ActiveTab = CurrentFileType switch
                {
                    "document" => "documents",
                    "image" => "photos",
                    "video" => "videos",
                    _ => "others"
                };
            }

            return Page();
        }
    }
}



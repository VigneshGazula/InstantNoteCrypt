using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using ShareItems_WebApp.Services;

namespace ShareItems_WebApp.Pages
{
    public class IndexModel : PageModel
    {
        private readonly INoteService _noteService;

        public IndexModel(INoteService noteService)
        {
            _noteService = noteService;
        }

        [BindProperty]
        public string Code { get; set; } = string.Empty;

        public string? ErrorMessage { get; set; }
        public string? Message { get; set; }

        public void OnGet()
        {
            // Check for TempData message (e.g., after deleting a note)
            if (TempData.ContainsKey("Message"))
            {
                Message = TempData["Message"]?.ToString();
            }
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (string.IsNullOrWhiteSpace(Code))
            {
                ErrorMessage = "Code is required.";
                return Page();
            }

            var note = await _noteService.GetNoteByCodeAsync(Code);

            if (note == null)
            {
                // Create new note with empty content
                note = await _noteService.CreateNoteAsync(Code);
                // New note has no PIN, go directly to dashboard
                return RedirectToPage("/Dashboard", new { code = Code });
            }

            // Check if note has PIN protection
            if (!string.IsNullOrWhiteSpace(note.Pin))
            {
                // Redirect to PIN verification page
                return RedirectToPage("/VerifyPin", new { code = Code });
            }

            // No PIN, go directly to dashboard
            return RedirectToPage("/Dashboard", new { code = Code });
        }
    }
}

using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using ShareItems_WebApp.Services;

namespace ShareItems_WebApp.Pages
{
    public class VerifyPinModel : PageModel
    {
        private readonly INoteService _noteService;

        public VerifyPinModel(INoteService noteService)
        {
            _noteService = noteService;
        }

        [BindProperty(SupportsGet = true)]
        public string Code { get; set; } = string.Empty;

        [BindProperty]
        public string Pin { get; set; } = string.Empty;

        public string? ErrorMessage { get; set; }

        public async Task<IActionResult> OnGetAsync()
        {
            if (string.IsNullOrWhiteSpace(Code))
            {
                return RedirectToPage("/Index");
            }

            var note = await _noteService.GetNoteByCodeAsync(Code);

            if (note == null)
            {
                return RedirectToPage("/Index");
            }

            // If note doesn't have a PIN, redirect directly to dashboard
            if (string.IsNullOrWhiteSpace(note.Pin))
            {
                return RedirectToPage("/Dashboard", new { code = Code });
            }

            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (string.IsNullOrWhiteSpace(Code))
            {
                return RedirectToPage("/Index");
            }

            var note = await _noteService.GetNoteByCodeAsync(Code);

            if (note == null)
            {
                ErrorMessage = "Note not found.";
                return Page();
            }

            if (string.IsNullOrWhiteSpace(Pin))
            {
                ErrorMessage = "Please enter the PIN.";
                return Page();
            }

            var isValid = await _noteService.ValidatePinAsync(note.Id, Pin);

            if (!isValid)
            {
                ErrorMessage = "Invalid PIN. Please try again.";
                return Page();
            }

            // PIN is valid, redirect to dashboard
            return RedirectToPage("/Dashboard", new { code = Code });
        }
    }
}

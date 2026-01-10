using Microsoft.AspNetCore.Mvc;
using ShareItems_WebApp.Entities;
using ShareItems_WebApp.Services;

namespace ShareItems_WebApp.Helpers
{
    /// <summary>
    /// Centralized server-side PIN authorization for note access.
    /// Ensures that all routes requiring note access are properly protected.
    /// </summary>
    public class NoteAuthorizationHelper
    {
        private readonly INoteService _noteService;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public NoteAuthorizationHelper(INoteService noteService, IHttpContextAccessor httpContextAccessor)
        {
            _noteService = noteService;
            _httpContextAccessor = httpContextAccessor;
        }

        /// <summary>
        /// Validates note access and enforces PIN protection.
        /// </summary>
        /// <param name="code">The note access code</param>
        /// <returns>
        /// AuthorizationResult containing:
        /// - IsAuthorized: true if access is allowed
        /// - Note: the note entity if access is allowed
        /// - RedirectAction: redirect to VerifyPin if PIN is required but not verified
        /// </returns>
        public async Task<AuthorizationResult> ValidateNoteAccessAsync(string code)
        {
            // Validate code parameter
            if (string.IsNullOrWhiteSpace(code))
            {
                return AuthorizationResult.Failed(null);
            }

            // Fetch the note
            var note = await _noteService.GetNoteByCodeAsync(code);

            if (note == null)
            {
                return AuthorizationResult.Failed(null);
            }

            // Check if note has PIN protection
            bool hasPin = !string.IsNullOrWhiteSpace(note.Pin);

            if (!hasPin)
            {
                // No PIN required, grant access
                return AuthorizationResult.Success(note);
            }

            // PIN is required - check session for verification
            var session = _httpContextAccessor.HttpContext?.Session;
            if (session == null)
            {
                return AuthorizationResult.RequiresPinVerification(code);
            }

            string sessionKey = GetPinVerificationSessionKey(code);
            bool isPinVerified = session.GetString(sessionKey) == "verified";

            if (isPinVerified)
            {
                // PIN already verified in this session
                return AuthorizationResult.Success(note);
            }

            // PIN not verified, redirect to verification page
            return AuthorizationResult.RequiresPinVerification(code);
        }

        /// <summary>
        /// Marks PIN as verified for a specific note code in the current session.
        /// </summary>
        public void MarkPinAsVerified(string code)
        {
            var session = _httpContextAccessor.HttpContext?.Session;
            if (session != null)
            {
                string sessionKey = GetPinVerificationSessionKey(code);
                session.SetString(sessionKey, "verified");
            }
        }

        /// <summary>
        /// Clears PIN verification for a specific note code (e.g., when PIN is removed).
        /// </summary>
        public void ClearPinVerification(string code)
        {
            var session = _httpContextAccessor.HttpContext?.Session;
            if (session != null)
            {
                string sessionKey = GetPinVerificationSessionKey(code);
                session.Remove(sessionKey);
            }
        }

        /// <summary>
        /// Gets the session key for PIN verification status.
        /// </summary>
        private string GetPinVerificationSessionKey(string code)
        {
            return $"PinVerified_{code}";
        }
    }

    /// <summary>
    /// Result of note authorization check.
    /// </summary>
    public class AuthorizationResult
    {
        public bool IsAuthorized { get; set; }
        public Note? Note { get; set; }
        public bool NeedsPinVerification { get; set; }
        public string? RedirectCode { get; set; }

        public static AuthorizationResult Success(Note note)
        {
            return new AuthorizationResult
            {
                IsAuthorized = true,
                Note = note,
                NeedsPinVerification = false
            };
        }

        public static AuthorizationResult Failed(Note? note)
        {
            return new AuthorizationResult
            {
                IsAuthorized = false,
                Note = note,
                NeedsPinVerification = false
            };
        }

        public static AuthorizationResult RequiresPinVerification(string code)
        {
            return new AuthorizationResult
            {
                IsAuthorized = false,
                Note = null,
                NeedsPinVerification = true,
                RedirectCode = code
            };
        }
    }
}

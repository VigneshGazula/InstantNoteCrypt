namespace ShareItems_WebApp.Helpers
{
    public static class FileValidationHelper
    {
        public const long MaxFileSize = 50 * 1024 * 1024; // 50MB

        // Forbidden extensions for security reasons
        public static readonly string[] ForbiddenExtensions = new[]
        {
            ".exe", ".bat", ".cmd", ".sh", ".ps1", ".js", ".vbs", ".jar", ".php", ".py", ".rb"
        };

        public static readonly Dictionary<string, string[]> AllowedExtensions = new()
        {
            { "document", new[] { ".pdf", ".doc", ".docx", ".txt", ".rtf", ".odt", ".ppt", ".pptx", ".xls", ".xlsx", ".csv", ".md", ".json", ".xml", ".yaml", ".yml" } },
            { "image", new[] { ".jpg", ".jpeg", ".png", ".webp", ".gif", ".bmp", ".tiff", ".tif", ".svg", ".ico", ".heic" } },
            { "video", new[] { ".mp4", ".mov", ".mkv", ".webm", ".avi", ".wmv", ".flv", ".m4v", ".3gp" } },
            { "others", new[] { ".zip", ".rar", ".7z", ".tar", ".gz", ".psd", ".ai", ".figma", ".blend", ".obj", ".stl", ".log", ".dat" } }
        };

        public static readonly Dictionary<string, string[]> AllowedMimeTypes = new()
        {
            { "document", new[] { 
                "application/pdf", 
                "application/msword", 
                "application/vnd.openxmlformats-officedocument.wordprocessingml.document",
                "text/plain",
                "application/rtf",
                "application/vnd.oasis.opendocument.text",
                "application/vnd.ms-powerpoint",
                "application/vnd.openxmlformats-officedocument.presentationml.presentation",
                "application/vnd.ms-excel",
                "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                "text/csv",
                "text/markdown",
                "application/json",
                "application/xml",
                "text/xml",
                "application/x-yaml",
                "text/yaml"
            } },
            { "image", new[] { 
                "image/jpeg", 
                "image/png", 
                "image/webp",
                "image/gif",
                "image/bmp",
                "image/tiff",
                "image/svg+xml",
                "image/x-icon",
                "image/heic"
            } },
            { "video", new[] { 
                "video/mp4", 
                "video/quicktime",
                "video/x-matroska",
                "video/webm",
                "video/x-msvideo",
                "video/x-ms-wmv",
                "video/x-flv",
                "video/x-m4v",
                "video/3gpp"
            } },
            { "others", new[] { 
                "application/zip", 
                "application/x-rar-compressed", 
                "application/x-7z-compressed",
                "application/x-tar",
                "application/gzip",
                "image/vnd.adobe.photoshop",
                "application/postscript",
                "application/octet-stream"
            } }
        };

        public static bool IsValidFileType(string fileType)
        {
            return AllowedExtensions.ContainsKey(fileType.ToLower());
        }

        public static bool IsValidExtension(string extension, string fileType)
        {
            if (!AllowedExtensions.ContainsKey(fileType.ToLower()))
            {
                return false;
            }
            return AllowedExtensions[fileType.ToLower()].Contains(extension.ToLower());
        }

        public static bool IsValidMimeType(string mimeType, string fileType)
        {
            if (!AllowedMimeTypes.ContainsKey(fileType.ToLower()))
            {
                return false;
            }
            return AllowedMimeTypes[fileType.ToLower()].Contains(mimeType.ToLower());
        }

        public static bool IsValidFileSize(long fileSize)
        {
            return fileSize > 0 && fileSize <= MaxFileSize;
        }

        /// <summary>
        /// Checks if the file extension is forbidden for security reasons
        /// </summary>
        /// <param name="fileName">The name of the file including extension</param>
        /// <returns>True if the extension is forbidden, false otherwise</returns>
        public static bool IsForbiddenExtension(string fileName)
        {
            if (string.IsNullOrWhiteSpace(fileName))
            {
                return false;
            }

            var extension = Path.GetExtension(fileName).ToLower();
            return ForbiddenExtensions.Contains(extension);
        }

        public static string GetFileSizeString(long bytes)
        {
            string[] sizes = { "B", "KB", "MB", "GB" };
            double len = bytes;
            int order = 0;
            while (len >= 1024 && order < sizes.Length - 1)
            {
                order++;
                len = len / 1024;
            }
            return $"{len:0.##} {sizes[order]}";
        }

        /// <summary>
        /// Automatically detects the file type category based on file extension
        /// </summary>
        /// <param name="fileName">The name of the file including extension</param>
        /// <returns>The file type category: "document", "image", "video", "others", or null if not supported/forbidden</returns>
        public static string? DetectFileType(string fileName)
        {
            if (string.IsNullOrWhiteSpace(fileName))
            {
                return null;
            }

            var extension = Path.GetExtension(fileName).ToLower();

            if (string.IsNullOrWhiteSpace(extension))
            {
                return null;
            }

            // Security check: Block forbidden extensions first
            if (ForbiddenExtensions.Contains(extension))
            {
                return null;
            }

            // Check allowed extensions
            foreach (var category in AllowedExtensions)
            {
                if (category.Value.Contains(extension))
                {
                    return category.Key;
                }
            }

            return null; // Unsupported file type
        }

        /// <summary>
        /// Gets a user-friendly error message for unsupported file types
        /// </summary>
        /// <returns>String listing all supported file types</returns>
        public static string GetSupportedFileTypesMessage()
        {
            var supportedTypes = new List<string>();
            
            foreach (var category in AllowedExtensions)
            {
                var extensions = string.Join(", ", category.Value);
                supportedTypes.Add($"{category.Key.ToUpper()}: {extensions}");
            }

            return "Supported file types:\n" + string.Join("\n", supportedTypes);
        }
    }
}

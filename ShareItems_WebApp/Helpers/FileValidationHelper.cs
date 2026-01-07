namespace ShareItems_WebApp.Helpers
{
    public static class FileValidationHelper
    {
        public const long MaxFileSize = 50 * 1024 * 1024; // 50MB

        public static readonly Dictionary<string, string[]> AllowedExtensions = new()
        {
            { "document", new[] { ".pdf", ".doc", ".docx" } },
            { "image", new[] { ".jpg", ".jpeg", ".png", ".webp" } },
            { "video", new[] { ".mp4", ".mov" } }
        };

        public static readonly Dictionary<string, string[]> AllowedMimeTypes = new()
        {
            { "document", new[] { "application/pdf", "application/msword", "application/vnd.openxmlformats-officedocument.wordprocessingml.document" } },
            { "image", new[] { "image/jpeg", "image/png", "image/webp" } },
            { "video", new[] { "video/mp4", "video/quicktime" } }
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
    }
}

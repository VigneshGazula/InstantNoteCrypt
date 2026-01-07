namespace ShareItems_WebApp.Settings
{
    /// <summary>
    /// Configuration settings for Cloudinary integration
    /// </summary>
    public class CloudinarySettings
    {
        public string CloudName { get; set; } = string.Empty;
        public string ApiKey { get; set; } = string.Empty;
        public string ApiSecret { get; set; } = string.Empty;

        /// <summary>
        /// Validates that all required settings are present
        /// </summary>
        public bool IsValid()
        {
            return !string.IsNullOrWhiteSpace(CloudName) &&
                   !string.IsNullOrWhiteSpace(ApiKey) &&
                   !string.IsNullOrWhiteSpace(ApiSecret);
        }
    }
}

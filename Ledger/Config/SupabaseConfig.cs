using System;

namespace Ledger.Config
{
    public class SupabaseConfig
    {
        public string ApiUrl { get; set; } = string.Empty;
        public string ApiKey { get; set; } = string.Empty;

        // Optional: Add method to validate configuration
        public bool IsValid()
        {
            return !string.IsNullOrEmpty(ApiUrl) && !string.IsNullOrEmpty(ApiKey);
        }
    }
}
using System;

namespace Ledger.Config
{
    public class OpenAiConfig
    {
        public string ApiKey { get; set; } = string.Empty;
        public string DefaultModel { get; set; } = "gpt-4";
        public string VisionModel { get; set; } = "gpt-4-vision-preview";
        public int MaxTokens { get; set; } = 2000;
        public float Temperature { get; set; } = 0.7f;

        // Optional: Add method to validate configuration
        public bool IsValid()
        {
            return !string.IsNullOrEmpty(ApiKey) && !string.IsNullOrEmpty(DefaultModel);
        }
    }
}
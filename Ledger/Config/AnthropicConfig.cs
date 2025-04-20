using System;

namespace Ledger.Config
{
    public class AnthropicConfig
    {
        public string ApiKey { get; set; } = string.Empty;
        public string ApiVersion { get; set; } = "2023-06-01";
        public string ModelName { get; set; } = "claude-3-sonnet-20240229";
        public int MaxTokens { get; set; } = 4096;
        public float Temperature { get; set; } = 0.7f;

        public bool IsValid()
        {
            return !string.IsNullOrEmpty(ApiKey) && !string.IsNullOrEmpty(ModelName);
        }
    }
}
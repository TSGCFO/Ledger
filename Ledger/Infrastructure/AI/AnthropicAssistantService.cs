using Ledger.Config;
using Ledger.Interfaces;
using Ledger.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Ledger.Infrastructure.AI
{
    public class AnthropicAssistantService : IAiAssistantService
    {
        private readonly HttpClient _httpClient;
        private readonly AnthropicConfig _config;
        private readonly IDatabaseService _databaseService;
        private bool _isInitialized = false;
        private readonly string _apiUrl = "https://api.anthropic.com/v1/messages";

        // JSON classes for serialization
        private class AnthropicMessage
        {
            [JsonPropertyName("role")]
            public string Role { get; set; } = string.Empty;

            [JsonPropertyName("content")]
            public object Content { get; set; } = new();
        }

        private class AnthropicTextContent
        {
            [JsonPropertyName("type")]
            public string Type { get; set; } = "text";

            [JsonPropertyName("text")]
            public string Text { get; set; } = string.Empty;
        }

        private class AnthropicImageContent
        {
            [JsonPropertyName("type")]
            public string Type { get; set; } = "image";

            [JsonPropertyName("source")]
            public AnthropicImageSource Source { get; set; } = new();
        }

        private class AnthropicImageSource
        {
            [JsonPropertyName("type")]
            public string Type { get; set; } = "base64";

            [JsonPropertyName("media_type")]
            public string MediaType { get; set; } = "image/jpeg";

            [JsonPropertyName("data")]
            public string Data { get; set; } = string.Empty;
        }

        private class AnthropicRequest
        {
            [JsonPropertyName("model")]
            public string Model { get; set; } = string.Empty;

            [JsonPropertyName("messages")]
            public List<AnthropicMessage> Messages { get; set; } = new();

            [JsonPropertyName("max_tokens")]
            public int MaxTokens { get; set; }

            [JsonPropertyName("temperature")]
            public float Temperature { get; set; }
        }

        private class AnthropicResponse
        {
            [JsonPropertyName("content")]
            public List<AnthropicContentBlock>? Content { get; set; }

            [JsonPropertyName("error")]
            public AnthropicError? Error { get; set; }
        }

        private class AnthropicContentBlock
        {
            [JsonPropertyName("type")]
            public string Type { get; set; } = string.Empty;

            [JsonPropertyName("text")]
            public string Text { get; set; } = string.Empty;
        }

        private class AnthropicError
        {
            [JsonPropertyName("type")]
            public string Type { get; set; } = string.Empty;

            [JsonPropertyName("message")]
            public string Message { get; set; } = string.Empty;
        }

        public AnthropicAssistantService(AnthropicConfig config, IDatabaseService databaseService)
        {
            _config = config;
            _databaseService = databaseService;
            _httpClient = new HttpClient();
        }

        public async Task<bool> InitializeAsync(string apiKey, string model = "claude-3-sonnet-20240229")
        {
            try
            {
                _config.ApiKey = apiKey;
                _config.ModelName = model;

                // Configure HTTP client with appropriate headers
                _httpClient.DefaultRequestHeaders.Clear();
                _httpClient.DefaultRequestHeaders.Add("x-api-key", apiKey);
                _httpClient.DefaultRequestHeaders.Add("anthropic-version", _config.ApiVersion);
                _httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                // Test connection with a simple query
                var messages = new List<AnthropicMessage>
                {
                    new AnthropicMessage
                    {
                        Role = "user",
                        Content = new AnthropicTextContent { Text = "Hello, are you connected?" }
                    }
                };

                var request = new AnthropicRequest
                {
                    Model = _config.ModelName,
                    Messages = messages,
                    MaxTokens = 50,
                    Temperature = _config.Temperature
                };

                var json = JsonSerializer.Serialize(request);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var response = await _httpClient.PostAsync(_apiUrl, content);
                response.EnsureSuccessStatusCode();

                _isInitialized = true;
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to initialize Anthropic: {ex.Message}");
                return false;
            }
        }

        private void EnsureInitialized()
        {
            if (!_isInitialized)
                throw new InvalidOperationException("Anthropic client is not initialized. Call InitializeAsync first.");
        }

        public async Task<string> ProcessQueryAsync(string userQuery)
        {
            EnsureInitialized();

            try
            {
                var systemPrompt = "You are a financial assistant for a cheque cashing business. " +
                                  "You help answer questions about transactions, customers, and vendors. " +
                                  "Be concise and direct in your responses. If you don't know something, say so.";

                var messages = new List<AnthropicMessage>
                {
                    new AnthropicMessage
                    {
                        Role = "system",
                        Content = systemPrompt
                    },
                    new AnthropicMessage
                    {
                        Role = "user",
                        Content = new AnthropicTextContent { Text = userQuery }
                    }
                };

                var request = new AnthropicRequest
                {
                    Model = _config.ModelName,
                    Messages = messages,
                    MaxTokens = _config.MaxTokens,
                    Temperature = _config.Temperature
                };

                var json = JsonSerializer.Serialize(request);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var response = await _httpClient.PostAsync(_apiUrl, content);
                response.EnsureSuccessStatusCode();

                var responseBody = await response.Content.ReadAsStringAsync();
                var anthropicResponse = JsonSerializer.Deserialize<AnthropicResponse>(responseBody);

                if (anthropicResponse?.Content != null && anthropicResponse.Content.Count > 0)
                {
                    return anthropicResponse.Content[0].Text;
                }
                else if (anthropicResponse?.Error != null)
                {
                    return $"Error: {anthropicResponse.Error.Message}";
                }
                else
                {
                    return "No response content received from the AI assistant.";
                }
            }
            catch (Exception ex)
            {
                return $"Error processing your query: {ex.Message}";
            }
        }

        public async Task<ChequeTransaction?> CreateTransactionFromTextAsync(string transactionDescription)
        {
            EnsureInitialized();

            try
            {
                var systemPrompt = "You are an AI assistant for a cheque cashing business. " +
                                  "Extract transaction details from the user's description and return them in a structured JSON format " +
                                  "with the following fields (where available): customerId, vendorId, chequeNumber, chequeAmount, date. " +
                                  "Format the response as valid JSON only, with no additional text.";

                var messages = new List<AnthropicMessage>
                {
                    new AnthropicMessage
                    {
                        Role = "system",
                        Content = systemPrompt
                    },
                    new AnthropicMessage
                    {
                        Role = "user",
                        Content = new AnthropicTextContent { Text = transactionDescription }
                    }
                };

                var request = new AnthropicRequest
                {
                    Model = _config.ModelName,
                    Messages = messages,
                    MaxTokens = _config.MaxTokens,
                    Temperature = 0.3f // Lower temperature for more deterministic responses
                };

                var json = JsonSerializer.Serialize(request);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var response = await _httpClient.PostAsync(_apiUrl, content);
                response.EnsureSuccessStatusCode();

                var responseBody = await response.Content.ReadAsStringAsync();
                var anthropicResponse = JsonSerializer.Deserialize<AnthropicResponse>(responseBody);

                if (anthropicResponse?.Content != null && anthropicResponse.Content.Count > 0)
                {
                    var jsonResponse = anthropicResponse.Content[0].Text;
                    // Clean up the response if it includes markdown formatting
                    jsonResponse = jsonResponse.Replace("```json", "").Replace("```", "").Trim();

                    // Parse the JSON response into a transaction object
                    var transaction = JsonSerializer.Deserialize<ChequeTransaction>(jsonResponse);
                    return transaction ?? CreateDefaultTransaction("AUTO");
                }
                else
                {
                    // Return default transaction if unable to parse
                    return CreateDefaultTransaction("ERROR");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error creating transaction from text: {ex.Message}");
                // Return default transaction on error
                return CreateDefaultTransaction("ERROR");
            }
        }

        public async Task<ChequeTransaction?> ExtractDataFromImageAsync(Stream imageStream)
        {
            EnsureInitialized();

            try
            {
                // Convert image to base64
                byte[] imageBytes;
                using (var memoryStream = new MemoryStream())
                {
                    await imageStream.CopyToAsync(memoryStream);
                    imageBytes = memoryStream.ToArray();
                }

                var base64Image = Convert.ToBase64String(imageBytes);

                var systemPrompt = "You are an AI assistant for a cheque cashing business. " +
                                   "Extract the following information from the cheque image: " +
                                   "cheque number, amount, date, and payee name. " +
                                   "Return the data in JSON format with the following fields: " +
                                   "customerId (default to 1 if not visible), vendorId (default to 'VND1' if not visible), " +
                                   "chequeNumber, chequeAmount, date (in yyyy-MM-dd format). " +
                                   "Format the response as valid JSON only, with no additional text.";

                // Create a message with mixed content (text and image)
                var messageContent = new List<object>
                {
                    new AnthropicTextContent { Text = "Extract data from this cheque image:" },
                    new AnthropicImageContent
                    {
                        Source = new AnthropicImageSource
                        {
                            MediaType = "image/jpeg",
                            Data = base64Image
                        }
                    }
                };

                var messages = new List<AnthropicMessage>
                {
                    new AnthropicMessage { Role = "system", Content = systemPrompt },
                    new AnthropicMessage { Role = "user", Content = messageContent }
                };

                var request = new AnthropicRequest
                {
                    Model = _config.ModelName,
                    Messages = messages,
                    MaxTokens = _config.MaxTokens,
                    Temperature = 0.3f // Lower temperature for more deterministic responses
                };

                var json = JsonSerializer.Serialize(request);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var response = await _httpClient.PostAsync(_apiUrl, content);
                response.EnsureSuccessStatusCode();

                var responseBody = await response.Content.ReadAsStringAsync();
                var anthropicResponse = JsonSerializer.Deserialize<AnthropicResponse>(responseBody);

                if (anthropicResponse?.Content != null && anthropicResponse.Content.Count > 0)
                {
                    var jsonResponse = anthropicResponse.Content[0].Text;
                    // Clean up the response if it includes markdown formatting
                    jsonResponse = jsonResponse.Replace("```json", "").Replace("```", "").Trim();

                    // Parse the JSON response into a transaction object
                    var transaction = JsonSerializer.Deserialize<ChequeTransaction>(jsonResponse);
                    return transaction ?? CreateDefaultTransaction("IMAGE");
                }
                else
                {
                    return CreateDefaultTransaction("IMAGE");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error extracting data from image: {ex.Message}");
                return CreateDefaultTransaction("ERROR");
            }
        }

        public async Task<string> AnalyzeTransactionsAsync(string query)
        {
            EnsureInitialized();

            try
            {
                // Get relevant data to include in the prompt
                var transactions = await _databaseService.GetTransactionsAsync();
                var transactionData = JsonSerializer.Serialize(transactions.Take(20)); // Limit to avoid token issues

                var systemPrompt = "You are a financial analyst for a cheque cashing business. " +
                                  "Analyze the transaction data provided and answer the user's query. " +
                                  "Be data-driven and provide insights where possible.";

                var userText = $"Transaction data: {transactionData}\n\nQuery: {query}";

                var messages = new List<AnthropicMessage>
                {
                    new AnthropicMessage { Role = "system", Content = systemPrompt },
                    new AnthropicMessage { Role = "user", Content = new AnthropicTextContent { Text = userText } }
                };

                var request = new AnthropicRequest
                {
                    Model = _config.ModelName,
                    Messages = messages,
                    MaxTokens = _config.MaxTokens,
                    Temperature = 0.5f
                };

                var json = JsonSerializer.Serialize(request);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var response = await _httpClient.PostAsync(_apiUrl, content);
                response.EnsureSuccessStatusCode();

                var responseBody = await response.Content.ReadAsStringAsync();
                var anthropicResponse = JsonSerializer.Deserialize<AnthropicResponse>(responseBody);

                if (anthropicResponse?.Content != null && anthropicResponse.Content.Count > 0)
                {
                    return anthropicResponse.Content[0].Text;
                }
                else if (anthropicResponse?.Error != null)
                {
                    return $"Error: {anthropicResponse.Error.Message}";
                }
                else
                {
                    return "No analysis could be generated.";
                }
            }
            catch (Exception ex)
            {
                return $"Error analyzing transactions: {ex.Message}";
            }
        }

        public async Task<string> GenerateReportAsync(string reportType, string parameters)
        {
            EnsureInitialized();

            try
            {
                // Customize the prompt based on the report type
                string systemPrompt;
                string userData;

                switch (reportType.ToLower())
                {
                    case "customer":
                        var customers = await _databaseService.GetCustomersAsync();
                        userData = JsonSerializer.Serialize(customers);
                        systemPrompt = "Generate a customer report with the given data.";
                        break;

                    case "transaction":
                        var transactions = await _databaseService.GetTransactionsAsync();
                        userData = JsonSerializer.Serialize(transactions.Take(50)); // Limit to avoid token issues
                        systemPrompt = "Generate a transaction summary report with the given data.";
                        break;

                    case "profit":
                        var allTransactions = await _databaseService.GetTransactionsAsync();
                        userData = JsonSerializer.Serialize(allTransactions.Take(50)); // Limit to avoid token issues
                        systemPrompt = "Generate a profit analysis report with the given data.";
                        break;

                    default:
                        return "Unsupported report type. Available types: customer, transaction, profit";
                }

                var userText = $"Data: {userData}\n\nParameters: {parameters}\n\nGenerate the report in a readable format.";

                var messages = new List<AnthropicMessage>
                {
                    new AnthropicMessage
                    {
                        Role = "system",
                        Content = $"You are a report generator for a cheque cashing business. {systemPrompt}"
                    },
                    new AnthropicMessage
                    {
                        Role = "user",
                        Content = new AnthropicTextContent { Text = userText }
                    }
                };

                var request = new AnthropicRequest
                {
                    Model = _config.ModelName,
                    Messages = messages,
                    MaxTokens = _config.MaxTokens,
                    Temperature = 0.3f
                };

                var json = JsonSerializer.Serialize(request);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var response = await _httpClient.PostAsync(_apiUrl, content);
                response.EnsureSuccessStatusCode();

                var responseBody = await response.Content.ReadAsStringAsync();
                var anthropicResponse = JsonSerializer.Deserialize<AnthropicResponse>(responseBody);

                if (anthropicResponse?.Content != null && anthropicResponse.Content.Count > 0)
                {
                    return anthropicResponse.Content[0].Text;
                }
                else if (anthropicResponse?.Error != null)
                {
                    return $"Error: {anthropicResponse.Error.Message}";
                }
                else
                {
                    return "No report could be generated.";
                }
            }
            catch (Exception ex)
            {
                return $"Error generating report: {ex.Message}";
            }
        }

        private ChequeTransaction CreateDefaultTransaction(string prefix)
        {
            return new ChequeTransaction
            {
                Date = DateTime.Today,
                CustomerId = 1,
                VendorId = "VND1",
                ChequeNumber = prefix + DateTime.Now.Ticks.ToString().Substring(10),
                ChequeAmount = 250.00m
            };
        }
    }
}
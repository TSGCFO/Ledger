using Azure;
using Azure.AI.OpenAI;
using Ledger.Config;
using Ledger.Interfaces;
using Ledger.Models;
using OpenAI.Chat;
using OpenAI;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace Ledger.Infrastructure.AI
{
    public class OpenAiAssistantService : IAiAssistantService
    {
        private OpenAIClient? _openAiClient;
        private readonly OpenAiConfig _config;
        private readonly IDatabaseService _databaseService;
        private bool _isInitialized = false;

        public OpenAiAssistantService(OpenAiConfig config, IDatabaseService databaseService)
        {
            _config = config;
            _databaseService = databaseService;
        }

        public async Task<bool> InitializeAsync(string apiKey, string model = "gpt-4")
        {
            try
            {
                _config.ApiKey = apiKey;
                _config.DefaultModel = model;

                _openAiClient = new OpenAIClient(apiKey, new OpenAIClientOptions());
                _isInitialized = true;

                // Verify the connection by making a simple request
                var chatCompletionsOptions = new ChatCompletionsOptions()
                {
                    Messages =
                    {
                        new ChatMessage(ChatRole.System, "You are a helpful assistant."),
                        new ChatMessage(ChatRole.User, "Hello, are you connected?")
                    },
                    MaxTokens = 50,
                    Temperature = 0.7f
                };

                var response = await _openAiClient.GetChatCompletionsAsync(_config.DefaultModel, chatCompletionsOptions);
                return response.Value.Choices.Count > 0;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to initialize OpenAI: {ex.Message}");
                return false;
            }
        }

        private void EnsureInitialized()
        {
            if (!_isInitialized || _openAiClient == null)
                throw new InvalidOperationException("OpenAI client is not initialized. Call InitializeAsync first.");
        }

        public async Task<string> ProcessQueryAsync(string userQuery)
        {
            EnsureInitialized();

            try
            {
                var systemPrompt = "You are a financial assistant for a cheque cashing business. " +
                                  "You help answer questions about transactions, customers, and vendors. " +
                                  "Be concise and direct in your responses. If you don't know something, say so.";

                var chatCompletionsOptions = new ChatCompletionsOptions()
                {
                    Messages =
                    {
                        new ChatMessage(ChatRole.System, systemPrompt),
                        new ChatMessage(ChatRole.User, userQuery)
                    },
                    MaxTokens = _config.MaxTokens,
                    Temperature = _config.Temperature
                };

                var response = await _openAiClient!.GetChatCompletionsAsync(_config.DefaultModel, chatCompletionsOptions);
                return response.Value.Choices[0].Message.Content;
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

                var chatCompletionsOptions = new ChatCompletionsOptions()
                {
                    Messages =
                    {
                        new ChatMessage(ChatRole.System, systemPrompt),
                        new ChatMessage(ChatRole.User, transactionDescription)
                    },
                    MaxTokens = _config.MaxTokens,
                    Temperature = 0.3f, // Lower temperature for more deterministic responses
                    ResponseFormat = ChatCompletionsResponseFormat.JsonObject
                };

                var response = await _openAiClient!.GetChatCompletionsAsync(_config.DefaultModel, chatCompletionsOptions);
                var jsonResponse = response.Value.Choices[0].Message.Content;

                // Parse the JSON response into a transaction object
                return JsonSerializer.Deserialize<ChequeTransaction>(jsonResponse);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error creating transaction from text: {ex.Message}");
                return null;
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
                var dataUri = $"data:image/jpeg;base64,{base64Image}";

                var systemPrompt = "You are an AI assistant for a cheque cashing business. " +
                                  "Extract the following information from the cheque image: " +
                                  "cheque number, amount, date, and payee name. " +
                                  "Return the data in JSON format with the following fields: " +
                                  "chequeNumber, chequeAmount, date. " +
                                  "Format the response as valid JSON only, with no additional text.";

                var messages = new List<ChatMessage>
                {
                    new ChatMessage(ChatRole.System, systemPrompt),
                    new ChatMessage(ChatRole.User, new ChatMessageContentItem[]
                    {
                        new ChatMessageTextContentItem("Extract data from this cheque image:"),
                        new ChatMessageImageContentItem(dataUri)
                    })
                };

                var chatCompletionsOptions = new ChatCompletionsOptions(_config.VisionModel)
                {
                    Messages = messages,
                    MaxTokens = _config.MaxTokens,
                    Temperature = 0.3f, // Lower temperature for more deterministic responses
                    ResponseFormat = ChatCompletionsResponseFormat.JsonObject
                };

                var response = await _openAiClient!.GetChatCompletionsAsync(chatCompletionsOptions);
                var jsonResponse = response.Value.Choices[0].Message.Content;

                // Parse the JSON response into a transaction object
                return JsonSerializer.Deserialize<ChequeTransaction>(jsonResponse);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error extracting data from image: {ex.Message}");
                return null;
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

                var chatCompletionsOptions = new ChatCompletionsOptions()
                {
                    Messages =
                    {
                        new ChatMessage(ChatRole.System, systemPrompt),
                        new ChatMessage(ChatRole.User, $"Transaction data: {transactionData}\n\nQuery: {query}")
                    },
                    MaxTokens = _config.MaxTokens,
                    Temperature = 0.5f
                };

                var response = await _openAiClient!.GetChatCompletionsAsync(_config.DefaultModel, chatCompletionsOptions);
                return response.Value.Choices[0].Message.Content;
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

                var chatCompletionsOptions = new ChatCompletionsOptions()
                {
                    Messages =
                    {
                        new ChatMessage(ChatRole.System, $"You are a report generator for a cheque cashing business. {systemPrompt}"),
                        new ChatMessage(ChatRole.User, $"Data: {userData}\n\nParameters: {parameters}\n\nGenerate the report in a readable format.")
                    },
                    MaxTokens = _config.MaxTokens,
                    Temperature = 0.3f
                };

                var response = await _openAiClient!.GetChatCompletionsAsync(_config.DefaultModel, chatCompletionsOptions);
                return response.Value.Choices[0].Message.Content;
            }
            catch (Exception ex)
            {
                return $"Error generating report: {ex.Message}";
            }
        }
    }
}
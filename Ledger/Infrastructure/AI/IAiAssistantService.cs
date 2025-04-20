using System.IO;
using System.Threading.Tasks;

namespace Ledger.Interfaces
{
    public interface IAiAssistantService
    {
        // Text-based AI interactions
        Task<string> ProcessQueryAsync(string userQuery);

        // Transaction creation from natural language
        Task<Models.ChequeTransaction?> CreateTransactionFromTextAsync(string transactionDescription);

        // Process document images (e.g., cheque images)
        Task<Models.ChequeTransaction?> ExtractDataFromImageAsync(Stream imageStream);

        // Transaction analysis and suggestions
        Task<string> AnalyzeTransactionsAsync(string query);

        // Generate reports
        Task<string> GenerateReportAsync(string reportType, string parameters);

        // Initialize the AI service
        Task<bool> InitializeAsync(string apiKey, string model = "gpt-4");
    }
}
using Ledger.Config;
using Ledger.Converters;
using Ledger.ViewModels;
using Ledger.Views;
using Ledger.Infrastructure.AI;
using Ledger.Infrastructure.Database;
using Ledger.Interfaces;
using Microsoft.Extensions.Logging;

namespace Ledger
{
    public static class MauiProgram
    {
        public static MauiApp CreateMauiApp()
        {
            var builder = MauiApp.CreateBuilder();
            builder
                .UseMauiApp<App>()
                .ConfigureFonts(fonts =>
                {
                    fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                    fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
                });

            // Register configurations
            builder.Services.AddSingleton<SupabaseConfig>(new SupabaseConfig());
            builder.Services.AddSingleton<AnthropicConfig>(new AnthropicConfig
            {
                ModelName = "claude-3-7-sonnet-20250219",
                ApiVersion = "2023-06-01",
                MaxTokens = 4096,
                Temperature = 0.7f
            });

            // Register services
            builder.Services.AddSingleton<IDatabaseService, SupabaseDatabaseService>();
            builder.Services.AddSingleton<IAiAssistantService, AnthropicAssistantService>();

            // Register converters
            builder.Services.AddSingleton<InverseBoolConverter>();

            // Register ViewModels
            builder.Services.AddSingleton<ChatViewModel>();

            // Register Pages
            builder.Services.AddSingleton<ChatPage>();

#if DEBUG
            builder.Logging.AddDebug();
#endif

            return builder.Build();
        }
    }
}
using Ledger.Config;
using Ledger.Interfaces;
using System;
using System.Threading.Tasks;
using System.Windows.Input;
using Microsoft.Maui.Controls;

namespace Ledger.ViewModels
{
    public class SettingsViewModel : ViewModelBase
    {
        private readonly AnthropicConfig _config;
        private readonly IAiAssistantService _aiService;
        private string _apiKey = string.Empty;
        private bool _isConnected;
        private string _statusMessage = string.Empty;

        public SettingsViewModel(AnthropicConfig config, IAiAssistantService aiService)
        {
            _config = config;
            _aiService = aiService;
            Title = "Settings";

            SaveCommand = new Command(async () => await SaveSettingsAsync(), () => !string.IsNullOrEmpty(ApiKey) && !IsBusy);
            TestConnectionCommand = new Command(async () => await TestConnectionAsync(), () => !string.IsNullOrEmpty(ApiKey) && !IsBusy);

            // Load saved API key if available
            ApiKey = _config.ApiKey;
        }

        public string ApiKey
        {
            get => _apiKey;
            set
            {
                if (SetProperty(ref _apiKey, value))
                {
                    (SaveCommand as Command)?.ChangeCanExecute();
                    (TestConnectionCommand as Command)?.ChangeCanExecute();
                }
            }
        }

        public bool IsConnected
        {
            get => _isConnected;
            set => SetProperty(ref _isConnected, value);
        }

        public string StatusMessage
        {
            get => _statusMessage;
            set => SetProperty(ref _statusMessage, value);
        }

        public ICommand SaveCommand { get; }
        public ICommand TestConnectionCommand { get; }

        private async Task SaveSettingsAsync()
        {
            if (string.IsNullOrEmpty(ApiKey) || IsBusy)
                return;

            try
            {
                IsBusy = true;
                _config.ApiKey = ApiKey;

                // Test connection to validate the API key
                var connectionResult = await TestConnectionAsync();

                if (connectionResult)
                {
                    StatusMessage = "Settings saved successfully!";
                }
            }
            catch (Exception ex)
            {
                StatusMessage = $"Error saving settings: {ex.Message}";
            }
            finally
            {
                IsBusy = false;
            }
        }

        private async Task<bool> TestConnectionAsync()
        {
            if (string.IsNullOrEmpty(ApiKey) || IsBusy)
                return false;

            try
            {
                IsBusy = true;
                StatusMessage = "Testing connection...";

                // Try to initialize the AI service with the API key
                var result = await _aiService.InitializeAsync(ApiKey);

                IsConnected = result;
                StatusMessage = result
                    ? "Connection successful!"
                    : "Connection failed. Please check your API key.";

                return result;
            }
            catch (Exception ex)
            {
                IsConnected = false;
                StatusMessage = $"Connection error: {ex.Message}";
                return false;
            }
            finally
            {
                IsBusy = false;
            }
        }
    }
}
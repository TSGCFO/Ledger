using Ledger.Interfaces;
using Ledger.Models;
using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Windows.Input;
using Microsoft.Maui.Controls;

namespace Ledger.ViewModels
{
    public class ChatViewModel : ViewModelBase
    {
        private readonly IAiAssistantService _aiService;
        private string _userMessage = string.Empty;

        public ChatViewModel(IAiAssistantService aiService)
        {
            _aiService = aiService;
            Title = "AI Assistant";
            Messages = new ObservableCollection<ChatMessage>();
            SendCommand = new Command(async () => await SendMessageAsync(), () => !string.IsNullOrWhiteSpace(UserMessage) && !IsBusy);
            ClearCommand = new Command(ClearMessages);

            // Add a welcome message
            Messages.Add(new ChatMessage
            {
                Text = "Hello! I'm your financial assistant. How can I help you today?",
                IsFromUser = false,
                Timestamp = DateTime.Now
            });
        }

        public ObservableCollection<ChatMessage> Messages { get; }

        public string UserMessage
        {
            get => _userMessage;
            set
            {
                if (SetProperty(ref _userMessage, value))
                {
                    (SendCommand as Command)?.ChangeCanExecute();
                }
            }
        }

        public ICommand SendCommand { get; }
        public ICommand ClearCommand { get; }

        private async Task SendMessageAsync()
        {
            if (string.IsNullOrWhiteSpace(UserMessage) || IsBusy)
                return;

            try
            {
                IsBusy = true;

                // Add user message to the chat
                var userMessageText = UserMessage.Trim();
                Messages.Add(new ChatMessage
                {
                    Text = userMessageText,
                    IsFromUser = true,
                    Timestamp = DateTime.Now
                });

                // Clear input
                UserMessage = string.Empty;

                // Add a typing indicator
                Messages.Add(new ChatMessage
                {
                    Text = "Typing...",
                    IsFromUser = false,
                    IsTyping = true,
                    Timestamp = DateTime.Now
                });

                // Get AI response
                var aiResponse = await _aiService.ProcessQueryAsync(userMessageText);

                // Remove typing indicator
                var typingMessage = Messages[Messages.Count - 1];
                if (typingMessage.IsTyping)
                {
                    Messages.Remove(typingMessage);
                }

                // Add AI response
                Messages.Add(new ChatMessage
                {
                    Text = aiResponse,
                    IsFromUser = false,
                    Timestamp = DateTime.Now
                });
            }
            catch (Exception ex)
            {
                Messages.Add(new ChatMessage
                {
                    Text = $"Error: {ex.Message}",
                    IsFromUser = false,
                    IsError = true,
                    Timestamp = DateTime.Now
                });
            }
            finally
            {
                IsBusy = false;
            }
        }

        private void ClearMessages()
        {
            Messages.Clear();

            // Add the welcome message back
            Messages.Add(new ChatMessage
            {
                Text = "Hello! I'm your financial assistant. How can I help you today?",
                IsFromUser = false,
                Timestamp = DateTime.Now
            });
        }

        public override async Task InitializeAsync(object? parameter)
        {
            // Initialize the AI service if needed
            // This could involve setting up the API key
            // For now, assume it's already initialized
        }
    }

    public class ChatMessage
    {
        public string Text { get; set; } = string.Empty;
        public bool IsFromUser { get; set; }
        public bool IsTyping { get; set; }
        public bool IsError { get; set; }
        public DateTime Timestamp { get; set; }
    }
}
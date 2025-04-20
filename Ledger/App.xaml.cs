using Ledger.Views;
using Microsoft.Maui.Controls.PlatformConfiguration;

namespace Ledger
{
    public partial class App : Application
    {
        private readonly ChatPage _chatPage;

        public App(ChatPage chatPage)
        {
            InitializeComponent();
            _chatPage = chatPage;
        }

        protected override Window CreateWindow(IActivationState? activationState)
        {
            var window = base.CreateWindow(activationState);

            // Set the page to our chat page wrapped in a navigation page
            window.Page = new NavigationPage(_chatPage);

            return window;
        }
    }
}
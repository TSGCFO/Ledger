using Microsoft.Maui.Controls.PlatformConfiguration;

namespace Ledger
{
    public partial class App : Application
    {
        private readonly AppShell _appShell;

        public App(AppShell appShell)
        {
            InitializeComponent(); // Ensure this method is generated in App.xaml
            _appShell = appShell;
        }

        protected override Window CreateWindow(IActivationState? activationState)
        {
            var window = base.CreateWindow(activationState);

            // Set the Shell as the main page
            window.Page = _appShell;

            return window;
        }
    }
}

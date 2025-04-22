using Microsoft.Maui.Controls.PlatformConfiguration;

namespace Ledger
{
    public partial class App : Application
    {
        private readonly AppShell _appShell;

        public App(AppShell appShell)
        {
            try
            {
                InitializeComponent();
                _appShell = appShell;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error initializing App: {ex.Message}");
                throw; // Re-throw to see the error
            }
        }

        protected override Window CreateWindow(IActivationState? activationState)
        {
            try
            {
                var window = base.CreateWindow(activationState);
                window.Page = _appShell;
                return window;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error creating window: {ex.Message}");
                throw; // Re-throw to see the error
            }
        }
    }
}
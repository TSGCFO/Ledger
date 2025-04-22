namespace Ledger
{
    public partial class AppShell : Shell
    {
        public AppShell()
        {
            InitializeComponent();

            // Register routes for navigation
            Routing.RegisterRoute(nameof(Ledger.Views.ChatPage), typeof(Ledger.Views.ChatPage));
            Routing.RegisterRoute(nameof(Ledger.Views.SettingsPage), typeof(Ledger.Views.SettingsPage));
            Routing.RegisterRoute(nameof(Ledger.Views.TransactionEntryPage), typeof(Ledger.Views.TransactionEntryPage));
        }
    }
}
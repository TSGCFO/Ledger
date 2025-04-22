using Ledger.ViewModels;

namespace Ledger.Views
{
    public partial class TransactionEntryPage : ContentPage
    {
        private TransactionEntryViewModel _viewModel;

        public TransactionEntryPage(TransactionEntryViewModel viewModel)
        {
            InitializeComponent();
            _viewModel = viewModel;
            BindingContext = viewModel;
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();
            await _viewModel.InitializeAsync(null);
        }
    }
}
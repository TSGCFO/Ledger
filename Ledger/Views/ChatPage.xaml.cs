using Ledger.ViewModels;
using System.Collections.Specialized;

namespace Ledger.Views
{
    public partial class ChatPage : ContentPage
    {
        private ChatViewModel _viewModel;

        public ChatPage(ChatViewModel viewModel)
        {
            InitializeComponent();
            _viewModel = viewModel;
            BindingContext = viewModel;

            // Add event handler for collection changes to enable auto-scrolling
            _viewModel.Messages.CollectionChanged += Messages_CollectionChanged;
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();
            await _viewModel.InitializeAsync(null);
        }

        protected override void OnDisappearing()
        {
            base.OnDisappearing();

            // Remove event handler when page is disposed
            _viewModel.Messages.CollectionChanged -= Messages_CollectionChanged;
        }

        private void Messages_CollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
        {
            // Scroll to the last item when items are added
            if (e.Action == NotifyCollectionChangedAction.Add && e.NewItems != null && MessagesCollection != null)
            {
                MessagesCollection.ScrollTo(e.NewItems[0], position: ScrollToPosition.End, animate: true);
            }
        }
    }
}
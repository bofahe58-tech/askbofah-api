using System.Collections.Specialized;
using AskBofah.ViewModels;

namespace AskBofah.Views
{
    public partial class ChatView : ContentView
    {
        public ChatView(ChatViewModel viewModel)
        {
            InitializeComponent();

            BindingContext = viewModel;

            if (viewModel.Messages is INotifyCollectionChanged collection)
            {
                collection.CollectionChanged += OnMessagesChanged;
            }
        }

        private void OnMessagesChanged(
            object? sender,
            NotifyCollectionChangedEventArgs e)
        {
            MainThread.BeginInvokeOnMainThread(async () =>
            {
                if (BindingContext is not ChatViewModel vm)
                    return;

                if (vm.Messages.Count == 0)
                    return;

                await Task.Delay(20);

                try
                {
                    MessagesView?.ScrollTo(
                        vm.Messages.Count - 1,
                        position: ScrollToPosition.End,
                        animate: true);
                }
                catch
                {
                    // Scroll failures are non-fatal — just skip
                }
            });
        }
    }
}
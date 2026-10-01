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

            // Auto-scroll when new messages arrive
            if (viewModel.Messages is INotifyCollectionChanged collection)
            {
                collection.CollectionChanged += OnMessagesChanged;
            }

            // Auto-scroll after layout is ready
            Loaded += (s, e) =>
            {
                if (viewModel.Messages.Count > 0)
                    TryScrollToEnd(viewModel.Messages.Count - 1);
            };
        }

        private void OnMessagesChanged(
            object? sender,
            NotifyCollectionChangedEventArgs e)
        {
            if (BindingContext is not ChatViewModel vm)
                return;

            if (vm.Messages.Count == 0)
                return;

            TryScrollToEnd(vm.Messages.Count - 1);
        }

        private void TryScrollToEnd(int index)
        {
            MainThread.BeginInvokeOnMainThread(async () =>
            {
                await Task.Delay(20);
                try
                {
                    MessagesView?.ScrollTo(
                        index,
                        position: ScrollToPosition.End,
                        animate: true);
                }
                catch
                {
                    // Scroll failures are non-fatal — ignore
                }
            });
        }
    }
}
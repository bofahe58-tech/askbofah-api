using System.Collections.Specialized;
using AskBofah.ViewModels;

namespace AskBofah.Views
{
    public partial class ChatPage : ContentPage
    {
        public ChatPage(ChatViewModel vm)
        {
            InitializeComponent();
            BindingContext = vm;

            // Auto-scroll when new messages arrive
            if (vm.Messages is INotifyCollectionChanged col)
            {
                col.CollectionChanged += (s, e) =>
                {
                    if (vm.Messages.Count > 0)
                        MessagesView.ScrollTo(vm.Messages.Count - 1, animate: true);
                };
            }
        }
    }
}
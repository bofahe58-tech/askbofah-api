using AskBofah.ViewModels;
using Microsoft.Extensions.DependencyInjection;

namespace AskBofah.Views
{
    public partial class MainPage : ContentPage
    {
        private readonly IServiceProvider _services;
        private readonly MainViewModel _viewModel;

        public MainPage(MainViewModel viewModel, IServiceProvider services)
        {
            InitializeComponent();

            _viewModel = viewModel;
            _services = services;

            BindingContext = _viewModel;
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();

            if (WorkspaceHost.Content is null)
            {
                var chatVm = _services.GetRequiredService<ChatViewModel>();
                WorkspaceHost.Content = new ChatView(chatVm);
            }
        }
    }
}
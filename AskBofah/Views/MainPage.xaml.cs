using AskBofah.ViewModels;
using Microsoft.Extensions.DependencyInjection;

namespace AskBofah.Views
{
    public partial class MainPage : ContentPage
    {
        private readonly IServiceProvider _services;
        private readonly MainViewModel _viewModel;

        private bool _sidebarExpanded = true;

        public MainPage(
            MainViewModel viewModel,
            IServiceProvider services)
        {
            InitializeComponent();

            _viewModel = viewModel;
            _services = services;

            BindingContext = _viewModel;

            _viewModel.PropertyChanged += OnViewModelPropertyChanged;

            SizeChanged += OnPageSizeChanged;
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();

            // Run AFTER the page is fully initialized and visible
            if (WorkspaceHost.Content is null)
            {
                ShowSection(_viewModel.ActiveSection ?? "Chat");
            }
        }

        private void OnViewModelPropertyChanged(
            object? sender,
            System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(MainViewModel.ActiveSection))
            {
                MainThread.BeginInvokeOnMainThread(() =>
                {
                    ShowSection(_viewModel.ActiveSection);
                });
            }

            if (e.PropertyName == nameof(MainViewModel.IsSidebarExpanded))
            {
                MainThread.BeginInvokeOnMainThread(() =>
                {
                    SetSidebar(_viewModel.IsSidebarExpanded);
                });
            }
        }

        private void OnPageSizeChanged(
            object? sender,
            EventArgs e)
        {
            if (Width <= 0)
                return;

            if (Width < 760)
            {
                SetSidebar(false);
            }
            else
            {
                SetSidebar(_viewModel.IsSidebarExpanded);
            }
        }

        private void SetSidebar(bool expanded)
        {
            if (_sidebarExpanded == expanded)
                return;

            _sidebarExpanded = expanded;

            RootGrid.ColumnDefinitions[0].Width =
                expanded ? 264 : 76;
        }

        private void ShowSection(string section)
        {
            if (WorkspaceHost is null)
                return;

            switch (section)
            {
                case "Chat":
                    var chatViewModel =
                        _services.GetRequiredService<ChatViewModel>();
                    WorkspaceHost.Content = new ChatView(chatViewModel);
                    break;

                case "Images":
                    WorkspaceHost.Content = new WorkspaceView(
                        "Images",
                        "Create and manage visual ideas, generated artwork and image projects.");
                    break;

                case "Library":
                    WorkspaceHost.Content = new WorkspaceView(
                        "Library",
                        "Keep documents, conversations, saved knowledge and files organized.");
                    break;

                case "Scheduled":
                    WorkspaceHost.Content = new WorkspaceView(
                        "Scheduled",
                        "Manage upcoming automations, recurring tasks and planned actions.");
                    break;

                case "Plugins":
                    WorkspaceHost.Content = new WorkspaceView(
                        "Plugins",
                        "Connect powerful services and extend what Ask Bofah can do.");
                    break;

                case "Projects":
                    WorkspaceHost.Content = new WorkspaceView(
                        "Projects",
                        "Create focused workspaces for your long-running projects.");
                    break;

                case "Codex":
                    WorkspaceHost.Content = new WorkspaceView(
                        "Codex",
                        "Build, debug, explain and work with your software projects.");
                    break;

                case "More":
                    WorkspaceHost.Content = new WorkspaceView(
                        "More",
                        "Manage settings, account options, help and additional Ask Bofah features.");
                    break;

                default:
                    ShowSection("Chat");
                    break;
            }
        }
    }
}
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace AskBofah.ViewModels
{
    public partial class MainViewModel : ObservableObject
    {
        [ObservableProperty]
        private string activeSection = "Chat";

        [ObservableProperty]
        private string pageTitle = "Ask Bofah";

        [ObservableProperty]
        private string pageSubtitle =
            "Your intelligent workspace";

        [ObservableProperty]
        private bool isSidebarExpanded = true;

        [RelayCommand]
        private void Navigate(string? section)
        {
            if (string.IsNullOrWhiteSpace(section))
                return;

            ActiveSection = section;

            switch (section)
            {
                case "Chat":

                    PageTitle = "Ask Bofah";
                    PageSubtitle =
                        "Your intelligent workspace";

                    break;

                case "Images":

                    PageTitle = "Images";
                    PageSubtitle =
                        "Create and manage visual ideas";

                    break;

                case "Library":

                    PageTitle = "Library";
                    PageSubtitle =
                        "Your saved knowledge and files";

                    break;

                case "Scheduled":

                    PageTitle = "Scheduled";
                    PageSubtitle =
                        "Automations and planned tasks";

                    break;

                case "Plugins":

                    PageTitle = "Plugins";
                    PageSubtitle =
                        "Extend what Ask Bofah can do";

                    break;

                case "Projects":

                    PageTitle = "Projects";
                    PageSubtitle =
                        "Keep your work organized";

                    break;

                case "Codex":

                    PageTitle = "Codex";
                    PageSubtitle =
                        "Build, debug and understand code";

                    break;

                case "More":

                    PageTitle = "More";
                    PageSubtitle =
                        "More tools and account features";

                    break;

                default:

                    PageTitle = section;
                    PageSubtitle =
                        "Ask Bofah workspace";

                    break;
            }
        }

        [RelayCommand]
        private void ToggleSidebar()
        {
            IsSidebarExpanded =
                !IsSidebarExpanded;
        }
    }
}
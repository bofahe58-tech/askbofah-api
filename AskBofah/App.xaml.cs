using AskBofah.Services;

namespace AskBofah
{
    public partial class App : Application
    {
        public App(AuthService auth)
        {
            InitializeComponent();

            // Try to restore session in background
            _ = Task.Run(async () =>
            {
                await auth.TryRestoreSessionAsync();
            });
        }

        protected override Window CreateWindow(IActivationState? activationState)
            => new Window(new AppShell());
    }
}
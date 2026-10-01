using AskBofah.Views;

namespace AskBofah
{
    public partial class AppShell : Shell
    {
        public AppShell()
        {
            InitializeComponent();

            // ==================== REGISTER ROUTES ====================
            Routing.RegisterRoute("register", typeof(RegisterPage));
            Routing.RegisterRoute("forgotpassword", typeof(ForgotPasswordPage));
            Routing.RegisterRoute("chat", typeof(ChatView));
            Routing.RegisterRoute("main", typeof(MainPage));
        }
    }
}
using AskBofah.ViewModels;

namespace AskBofah.Views
{
    public partial class ForgotPasswordPage : ContentPage
    {
        public ForgotPasswordPage(ForgotPasswordViewModel vm)
        {
            InitializeComponent();
            BindingContext = vm;
        }
    }
}
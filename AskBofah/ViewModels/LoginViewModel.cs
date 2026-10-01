using AskBofah.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace AskBofah.ViewModels
{
    public partial class LoginViewModel : ObservableObject
    {
        private readonly AuthService _auth;

        public LoginViewModel(AuthService auth)
        {
            _auth = auth;
        }

        [ObservableProperty]
        private string email = string.Empty;

        [ObservableProperty]
        private string password = string.Empty;

        [ObservableProperty]
        private string errorMessage = string.Empty;

        [ObservableProperty]
        private bool hasError;

        [ObservableProperty]
        private bool isBusy;

        [ObservableProperty]
        private bool isPasswordHidden = true;

        public string LoginButtonText =>
            IsBusy ? "Signing in..." : "Sign in";

        partial void OnIsBusyChanged(bool value)
        {
            OnPropertyChanged(nameof(LoginButtonText));
        }

        [RelayCommand]
        private void TogglePassword()
        {
            IsPasswordHidden = !IsPasswordHidden;
        }

        [RelayCommand]
        private async Task LoginAsync()
        {
            if (IsBusy)
                return;

            HasError = false;
            ErrorMessage = string.Empty;

            if (string.IsNullOrWhiteSpace(Email))
            {
                ShowError("Please enter your email address.");
                return;
            }

            if (!IsValidEmail(Email))
            {
                ShowError("Please enter a valid email address.");
                return;
            }

            if (string.IsNullOrWhiteSpace(Password))
            {
                ShowError("Please enter your password.");
                return;
            }

            IsBusy = true;

            try
            {
                var (ok, error) = await _auth.LoginAsync(
                    Email.Trim(),
                    Password);

                if (!ok)
                {
                    ShowError(
                        string.IsNullOrWhiteSpace(error)
                            ? "We couldn't sign you in. Please check your details and try again."
                            : error);

                    return;
                }

                Email = string.Empty;
                Password = string.Empty;

                await Shell.Current.GoToAsync("//main");
            }
            catch
            {
                ShowError(
                    "Something went wrong while signing you in. Please try again.");
            }
            finally
            {
                IsBusy = false;
            }
        }

        [RelayCommand]
        private async Task GoToRegisterAsync()
        {
            if (IsBusy)
                return;

            await Shell.Current.GoToAsync("register");
        }

        [RelayCommand]
        private async Task GoToForgotPasswordAsync()
        {
            if (IsBusy)
                return;

            await Shell.Current.GoToAsync("forgotpassword");
        }

        [RelayCommand]
        private async Task GoogleLoginAsync()
        {
            if (IsBusy)
                return;

            await Shell.Current.DisplayAlert(
                "Google Sign-In",
                "Google authentication is being wired up. It will be available shortly.",
                "OK");
        }

        private void ShowError(string message)
        {
            ErrorMessage = message;
            HasError = true;
        }

        private static bool IsValidEmail(string email)
        {
            return email.Contains("@") &&
                   email.Contains(".") &&
                   email.IndexOf("@") > 0 &&
                   email.IndexOf("@") < email.Length - 1;
        }
    }
}
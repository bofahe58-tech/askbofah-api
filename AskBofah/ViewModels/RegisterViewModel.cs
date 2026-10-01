using AskBofah.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace AskBofah.ViewModels
{
    public partial class RegisterViewModel : ObservableObject
    {
        private readonly AuthService _auth;

        public RegisterViewModel(AuthService auth)
        {
            _auth = auth;
        }

        [ObservableProperty]
        private string fullName = string.Empty;

        [ObservableProperty]
        private string email = string.Empty;

        [ObservableProperty]
        private string password = string.Empty;

        [ObservableProperty]
        private string confirmPassword = string.Empty;

        [ObservableProperty]
        private string errorMessage = string.Empty;

        [ObservableProperty]
        private bool hasError;

        [ObservableProperty]
        private bool isBusy;

        [ObservableProperty]
        private bool isPasswordHidden = true;

        [ObservableProperty]
        private bool isConfirmPasswordHidden = true;

        public string RegisterButtonText =>
            IsBusy ? "Creating your account..." : "Create account";

        partial void OnIsBusyChanged(bool value)
        {
            OnPropertyChanged(nameof(RegisterButtonText));
        }

        [RelayCommand]
        private void TogglePassword()
        {
            IsPasswordHidden = !IsPasswordHidden;
        }

        [RelayCommand]
        private void ToggleConfirmPassword()
        {
            IsConfirmPasswordHidden = !IsConfirmPasswordHidden;
        }

        [RelayCommand]
        private async Task RegisterAsync()
        {
            if (IsBusy)
                return;

            HasError = false;
            ErrorMessage = string.Empty;

            if (string.IsNullOrWhiteSpace(FullName))
            {
                ShowError("Please enter your full name.");
                return;
            }

            if (FullName.Trim().Length < 2)
            {
                ShowError("Please enter your full name.");
                return;
            }

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
                ShowError("Please create a password.");
                return;
            }

            if (Password.Length < 6)
            {
                ShowError("Your password must contain at least 6 characters.");
                return;
            }

            if (Password != ConfirmPassword)
            {
                ShowError("Your passwords do not match.");
                return;
            }

            IsBusy = true;

            try
            {
                var (ok, error) = await _auth.RegisterAsync(
                    FullName.Trim(),
                    Email.Trim(),
                    Password);

                if (!ok)
                {
                    ShowError(
                        string.IsNullOrWhiteSpace(error)
                            ? "We couldn't create your account. Please try again."
                            : error);

                    return;
                }

                await Shell.Current.DisplayAlert(
                    "Welcome to Ask Bofah",
                    "Your account is ready. Let's get started.",
                    "Continue");

                await Shell.Current.GoToAsync("//main");
            }
            catch
            {
                ShowError(
                    "Something went wrong while creating your account. Please try again.");
            }
            finally
            {
                IsBusy = false;
            }
        }

        [RelayCommand]
        private async Task GoToLoginAsync()
        {
            if (IsBusy)
                return;

            await Shell.Current.GoToAsync("..");
        }

        [RelayCommand]
        private async Task GoogleRegisterAsync()
        {
            if (IsBusy)
                return;

            await Shell.Current.DisplayAlert(
                "Google Sign-Up",
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
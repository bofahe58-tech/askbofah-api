using AskBofah.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace AskBofah.ViewModels
{
    public partial class ForgotPasswordViewModel : ObservableObject
    {
        private readonly AuthService _auth;

        public ForgotPasswordViewModel(AuthService auth)
        {
            _auth = auth;
        }

        [ObservableProperty]
        private string email = string.Empty;

        [ObservableProperty]
        private string errorMessage = string.Empty;

        [ObservableProperty]
        private string successMessage = string.Empty;

        [ObservableProperty]
        private bool hasError;

        [ObservableProperty]
        private bool isSent;

        [ObservableProperty]
        private bool isBusy;

        public bool IsNotSent => !IsSent;

        public string ButtonText =>
            IsBusy ? "Sending..." : "Send reset link";

        partial void OnIsSentChanged(bool value)
        {
            OnPropertyChanged(nameof(IsNotSent));
        }

        partial void OnIsBusyChanged(bool value)
        {
            OnPropertyChanged(nameof(ButtonText));
        }

        [RelayCommand]
        private async Task SendResetLinkAsync()
        {
            if (IsBusy)
                return;

            HasError = false;
            ErrorMessage = string.Empty;
            SuccessMessage = string.Empty;

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

            IsBusy = true;

            try
            {
                var (ok, error) =
                    await _auth.SendPasswordResetAsync(Email.Trim());

                if (!ok)
                {
                    ShowError(
                        string.IsNullOrWhiteSpace(error)
                            ? "We couldn't send the reset link. Please try again."
                            : error);

                    return;
                }

                SuccessMessage =
                    $"We've sent a password reset link to {Email.Trim()}. " +
                    "Check your inbox and spam folder.";

                IsSent = true;
            }
            catch
            {
                ShowError(
                    "Something went wrong while sending the reset link.");
            }
            finally
            {
                IsBusy = false;
            }
        }

        [RelayCommand]
        private async Task ResendAsync()
        {
            IsSent = false;
            await Task.Delay(150);
            await SendResetLinkAsync();
        }

        [RelayCommand]
        private async Task GoBackAsync()
        {
            await Shell.Current.GoToAsync("..");
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
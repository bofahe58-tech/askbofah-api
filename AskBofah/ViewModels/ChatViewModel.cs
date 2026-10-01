using System.Collections.ObjectModel;
using AskBofah.Helpers;
using AskBofah.Models;
using AskBofah.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace AskBofah.ViewModels
{
    public partial class ChatViewModel : ObservableObject
    {
        private readonly OpenRouterService _ai;
        private readonly AuthService _auth;
        private readonly SecureStorageService _secure;

        private string _lastUserPrompt = string.Empty;

        public ChatViewModel(
            OpenRouterService ai,
            AuthService auth,
            SecureStorageService secure)
        {
            _ai = ai;
            _auth = auth;
            _secure = secure;

            var user = _auth.CurrentUser;

            var firstName = "there";

            if (!string.IsNullOrWhiteSpace(user?.Name))
            {
                firstName =
                    user.Name
                        .Split(
                            ' ',
                            StringSplitOptions.RemoveEmptyEntries)
                        .FirstOrDefault()
                    ?? "there";
            }

            Greeting =
                $"Hi {firstName}, how can I help you today?";

            Messages.Add(
                new ChatMessage
                {
                    Text =
                        "Hello! I'm Ask Bofah AI. " +
                        "Ask me anything — code, essays, " +
                        "explanations, ideas, or questions.",
                    IsUser = false,
                    IsStreaming = false
                });
        }

        // =========================================================
        // CHAT MESSAGES
        // =========================================================

        public ObservableCollection<ChatMessage> Messages
        {
            get;
        } = new();

        // =========================================================
        // INPUT
        // =========================================================

        [ObservableProperty]
        private string inputText = string.Empty;

        // =========================================================
        // GREETING
        // =========================================================

        [ObservableProperty]
        private string greeting = string.Empty;

        // =========================================================
        // BUSY STATE
        // =========================================================

        [ObservableProperty]
        private bool isBusy;

        // =========================================================
        // SEND MESSAGE
        // =========================================================

        [RelayCommand]
        private async Task SendAsync()
        {
            if (IsBusy)
                return;

            var text =
                InputText?.Trim();

            if (string.IsNullOrWhiteSpace(text))
                return;

            // Clear input immediately.
            InputText = string.Empty;

            // Remember the latest prompt.
            _lastUserPrompt = text;

            // Add user message.
            Messages.Add(
                new ChatMessage
                {
                    Text = text,
                    IsUser = true,
                    IsStreaming = false,
                    Timestamp = DateTime.Now
                });

            // Create streaming placeholder.
            var placeholder =
                new ChatMessage
                {
                    Text = "▋",
                    IsUser = false,
                    IsStreaming = true,
                    Timestamp = DateTime.Now
                };

            Messages.Add(placeholder);

            IsBusy = true;

            try
            {
                // =================================================
                // BUILD CONVERSATION HISTORY
                // =================================================

                var history =
                    new List<(string role, string content)>();

                foreach (var message in Messages)
                {
                    if (message.IsStreaming)
                        continue;

                    if (string.IsNullOrWhiteSpace(message.Text))
                        continue;

                    history.Add(
                        (
                            message.IsUser
                                ? "user"
                                : "assistant",
                            message.Text
                        ));
                }

                // =================================================
                // STREAM AI RESPONSE
                // =================================================

                var fullText = string.Empty;

                await foreach (
                    var token in
                    _ai.StreamChatAsync(history))
                {
                    if (string.IsNullOrEmpty(token))
                        continue;

                    fullText += token;

                    var index =
                        Messages.IndexOf(placeholder);

                    if (index < 0)
                        break;

                    var updated =
                        new ChatMessage
                        {
                            Text = fullText,
                            IsUser = false,
                            IsStreaming = true,
                            Timestamp =
                                placeholder.Timestamp
                        };

                    Messages[index] = updated;

                    placeholder = updated;
                }

                // =================================================
                // FINAL AI MESSAGE
                // =================================================

                var finalIndex =
                    Messages.IndexOf(placeholder);

                if (finalIndex >= 0)
                {
                    Messages[finalIndex] =
                        new ChatMessage
                        {
                            Text =
                                string.IsNullOrWhiteSpace(fullText)
                                    ? "I couldn't generate a response."
                                    : fullText,
                            IsUser = false,
                            IsStreaming = false,
                            Timestamp =
                                placeholder.Timestamp
                        };
                }
            }
            catch (Exception ex)
            {
                // =================================================
                // ERROR
                // =================================================

                var errorIndex =
                    Messages.IndexOf(placeholder);

                var errorMessage =
                    $"Something went wrong.\n\n{ex.Message}";

                if (errorIndex >= 0)
                {
                    Messages[errorIndex] =
                        new ChatMessage
                        {
                            Text = errorMessage,
                            IsUser = false,
                            IsStreaming = false,
                            Timestamp = DateTime.Now
                        };
                }
                else
                {
                    Messages.Add(
                        new ChatMessage
                        {
                            Text = errorMessage,
                            IsUser = false,
                            IsStreaming = false,
                            Timestamp = DateTime.Now
                        });
                }
            }
            finally
            {
                IsBusy = false;
            }
        }

        // =========================================================
        // REGENERATE
        // =========================================================

        [RelayCommand]
        private async Task RegenerateAsync()
        {
            if (IsBusy)
                return;

            if (string.IsNullOrWhiteSpace(
                    _lastUserPrompt))
            {
                return;
            }

            // Remove the latest assistant response.
            if (Messages.Count > 0 &&
                !Messages.Last().IsUser)
            {
                Messages.RemoveAt(
                    Messages.Count - 1);
            }

            InputText = _lastUserPrompt;

            await SendAsync();
        }

        // =========================================================
        // CLEAR CHAT
        // =========================================================

        [RelayCommand]
        private async Task ClearChatAsync()
        {
            if (IsBusy)
                return;

            var confirm =
                await Shell.Current.DisplayAlert(
                    "Clear conversation",
                    "Are you sure you want to delete this conversation?",
                    "Clear",
                    "Cancel");

            if (!confirm)
                return;

            Messages.Clear();

            _lastUserPrompt =
                string.Empty;

            Messages.Add(
                new ChatMessage
                {
                    Text =
                        "Chat cleared. " +
                        "What would you like to ask?",
                    IsUser = false,
                    IsStreaming = false,
                    Timestamp = DateTime.Now
                });
        }

        // =========================================================
        // COPY MESSAGE
        // =========================================================

        [RelayCommand]
        private async Task CopyMessageAsync(
            ChatMessage? message)
        {
            if (message is null)
                return;

            if (string.IsNullOrWhiteSpace(message.Text))
                return;

            try
            {
                await Clipboard.Default.SetTextAsync(
                    message.Text);

                await Shell.Current.DisplayAlert(
                    "Copied",
                    "The response has been copied to your clipboard.",
                    "OK");
            }
            catch (Exception ex)
            {
                await Shell.Current.DisplayAlert(
                    "Copy failed",
                    ex.Message,
                    "OK");
            }
        }

        // =========================================================
        // SHARE MESSAGE
        // =========================================================

        [RelayCommand]
        private async Task ShareMessageAsync(
            ChatMessage? message)
        {
            if (message is null)
                return;

            if (string.IsNullOrWhiteSpace(message.Text))
                return;

            try
            {
                await Share.Default.RequestAsync(
                    new ShareTextRequest
                    {
                        Text = message.Text,
                        Title = "Share from Ask Bofah"
                    });
            }
            catch (Exception ex)
            {
                await Shell.Current.DisplayAlert(
                    "Share failed",
                    ex.Message,
                    "OK");
            }
        }

        // =========================================================
        // DOWNLOAD / SAVE MESSAGE
        // =========================================================

        [RelayCommand]
        private async Task DownloadMessageAsync(
            ChatMessage? message)
        {
            if (message is null)
                return;

            if (string.IsNullOrWhiteSpace(message.Text))
                return;

            try
            {
                var fileName =
                    $"AskBofah_{DateTime.Now:yyyyMMdd_HHmmss}.txt";

                var path =
                    Path.Combine(
                        FileSystem.CacheDirectory,
                        fileName);

                await File.WriteAllTextAsync(
                    path,
                    message.Text);

                await Share.Default.RequestAsync(
                    new ShareFileRequest
                    {
                        Title =
                            "Save Ask Bofah response",
                        File =
                            new ShareFile(path)
                    });
            }
            catch (Exception ex)
            {
                await Shell.Current.DisplayAlert(
                    "Save failed",
                    ex.Message,
                    "OK");
            }
        }

        // =========================================================
        // LOGOUT
        // =========================================================

        [RelayCommand]
        private async Task LogoutAsync()
        {
            if (IsBusy)
                return;

            var confirm =
                await Shell.Current.DisplayAlert(
                    "Log out",
                    "Are you sure you want to log out of Ask Bofah?",
                    "Log out",
                    "Cancel");

            if (!confirm)
                return;

            try
            {
                await _auth.LogoutAsync();

                await Shell.Current.GoToAsync(
                    "//login");
            }
            catch (Exception ex)
            {
                await Shell.Current.DisplayAlert(
                    "Logout failed",
                    ex.Message,
                    "OK");
            }
        }

        // =========================================================
        // NEW CHAT
        // =========================================================

        [RelayCommand]
        private void NewChat()
        {
            if (IsBusy)
                return;

            Messages.Clear();

            InputText =
                string.Empty;

            _lastUserPrompt =
                string.Empty;

            Messages.Add(
                new ChatMessage
                {
                    Text =
                        "New conversation started. " +
                        "What would you like to work on?",
                    IsUser = false,
                    IsStreaming = false,
                    Timestamp = DateTime.Now
                });
        }
    }
}
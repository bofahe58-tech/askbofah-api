using AskBofah.Models;
using AskBofah.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;

namespace AskBofah.ViewModels
{
    public partial class ChatViewModel : ObservableObject
    {
        private readonly ChatService _chat;
        private readonly AuthService _auth;
        private readonly SecureStorageService _secure;

        private string _lastUserPrompt = string.Empty;
        private Guid? _currentConversationId;

        public ChatViewModel(
            ChatService chat,
            AuthService auth,
            SecureStorageService secure)
        {
            _chat = chat;
            _auth = auth;
            _secure = secure;

            // Read user info immediately
            RefreshUserInfo();

            // Welcome message
            Messages.Add(new ChatMessage
            {
                Text = "Hello! I'm Ask Bofah AI. Ask me anything — code, essays, explanations, ideas, or questions.",
                IsUser = false,
                IsStreaming = false
            });

            // Async init
            _ = InitializeAsync();
        }

        // ============================================================
        // INIT — restore session if needed, then refresh UI
        // ============================================================
        private async Task InitializeAsync()
        {
            try
            {
                if (_auth.CurrentUser is null)
                {
                    await _auth.TryRestoreSessionAsync();
                }

                await MainThread.InvokeOnMainThreadAsync(RefreshUserInfo);

                await LoadConversationsAsync();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[ChatViewModel] Init error: {ex.Message}");
            }
        }

        // ============================================================
        // REFRESH USER INFO — with email fallback so it never says "Guest"
        // ============================================================
        private void RefreshUserInfo()
        {
            var user = _auth.CurrentUser;

            string displayName = "Guest";

            // Prefer full name; fall back to email prefix
            if (!string.IsNullOrWhiteSpace(user?.Name))
            {
                displayName = user.Name.Trim();
            }
            else if (!string.IsNullOrWhiteSpace(user?.Email))
            {
                var prefix = user.Email.Split('@')[0];
                displayName = prefix.Length > 0
                    ? char.ToUpperInvariant(prefix[0]) + prefix.Substring(1)
                    : "Guest";
            }

            UserName = displayName;

            Initial = string.IsNullOrWhiteSpace(UserName)
                ? "U"
                : UserName.Substring(0, 1).ToUpperInvariant();

            var firstName = "there";
            var parts = UserName.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length > 0)
                firstName = parts[0];

            Greeting = $"Hi {firstName}, how can I help you today?";

            PlanTier = user?.PlanTier ?? "Free";
            PlanLabel = PlanTier == "Free"
                ? "Free plan · Upgrade for more"
                : $"{PlanTier} plan";
            IsFreePlan = PlanTier == "Free";
        }

        // ============================================================
        // USER INFO — bound to sidebar
        // ============================================================
        [ObservableProperty]
        private string userName = "Guest";

        [ObservableProperty]
        private string initial = "U";

        // ============================================================
        // MESSAGES
        // ============================================================
        public ObservableCollection<ChatMessage> Messages { get; } = new();

        // ============================================================
        // SIDEBAR: CHAT HISTORY
        // ============================================================
        public ObservableCollection<ConversationSummary> Conversations { get; } = new();

        [ObservableProperty]
        private bool isLoadingHistory;

        // ============================================================
        // INPUT
        // ============================================================
        [ObservableProperty]
        private string inputText = string.Empty;

        // ============================================================
        // GREETING
        // ============================================================
        [ObservableProperty]
        private string greeting = string.Empty;

        // ============================================================
        // BUSY
        // ============================================================
        [ObservableProperty]
        private bool isBusy;

        // ============================================================
        // PLAN
        // ============================================================
        [ObservableProperty]
        private string planTier = "Free";

        [ObservableProperty]
        private string planLabel = "Free plan · Upgrade for more";

        [ObservableProperty]
        private bool isFreePlan = true;

        // ============================================================
        // LOAD CHAT HISTORY
        // ============================================================
        [RelayCommand]
        private async Task LoadConversationsAsync()
        {
            try
            {
                IsLoadingHistory = true;

                var list = await _chat.GetConversationsAsync();
                Conversations.Clear();

                foreach (var c in list)
                    Conversations.Add(c);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[ChatViewModel] LoadConversations error: {ex.Message}");
            }
            finally
            {
                IsLoadingHistory = false;
            }
        }

        // ============================================================
        // OPEN CONVERSATION
        // ============================================================
        [RelayCommand]
        private async Task OpenConversationAsync(ConversationSummary? summary)
        {
            if (summary is null || IsBusy) return;

            try
            {
                IsBusy = true;

                var detail = await _chat.GetConversationAsync(summary.Id);
                if (detail is null) return;

                _currentConversationId = detail.Id;

                Messages.Clear();
                foreach (var m in detail.Messages)
                {
                    Messages.Add(new ChatMessage
                    {
                        Text = m.Content,
                        IsUser = m.Role == "user",
                        Timestamp = m.CreatedAt,
                        IsStreaming = false
                    });
                }
            }
            catch (Exception ex)
            {
                await Shell.Current.DisplayAlert("Couldn't load conversation", ex.Message, "OK");
            }
            finally { IsBusy = false; }
        }

        // ============================================================
        // DELETE CONVERSATION
        // ============================================================
        [RelayCommand]
        private async Task DeleteConversationAsync(ConversationSummary? summary)
        {
            if (summary is null) return;

            var confirm = await Shell.Current.DisplayAlert(
                "Delete conversation",
                $"Delete \"{summary.Title}\"?",
                "Delete", "Cancel");

            if (!confirm) return;

            try
            {
                await _chat.DeleteConversationAsync(summary.Id);
                Conversations.Remove(summary);

                if (_currentConversationId == summary.Id)
                {
                    Messages.Clear();
                    _currentConversationId = null;
                    Messages.Add(new ChatMessage
                    {
                        Text = "New conversation started. What would you like to work on?",
                        IsUser = false,
                        Timestamp = DateTime.Now
                    });
                }
            }
            catch (Exception ex)
            {
                await Shell.Current.DisplayAlert("Delete failed", ex.Message, "OK");
            }
        }

        // ============================================================
        // SEND MESSAGE
        // ============================================================
        [RelayCommand]
        private async Task SendAsync()
        {
            if (IsBusy) return;

            var text = InputText?.Trim();
            if (string.IsNullOrWhiteSpace(text)) return;

            InputText = string.Empty;
            _lastUserPrompt = text;

            Messages.Add(new ChatMessage
            {
                Text = text,
                IsUser = true,
                IsStreaming = false,
                Timestamp = DateTime.Now
            });

            var placeholder = new ChatMessage
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
                var fullText = string.Empty;

                await foreach (var token in _chat.StreamChatAsync(
                    text, _currentConversationId, id => _currentConversationId = id))
                {
                    if (string.IsNullOrEmpty(token)) continue;
                    fullText += token;

                    var index = Messages.IndexOf(placeholder);
                    if (index < 0) break;

                    var updated = new ChatMessage
                    {
                        Text = fullText,
                        IsUser = false,
                        IsStreaming = true,
                        Timestamp = placeholder.Timestamp
                    };

                    Messages[index] = updated;
                    placeholder = updated;
                }

                var finalIndex = Messages.IndexOf(placeholder);
                if (finalIndex >= 0)
                {
                    Messages[finalIndex] = new ChatMessage
                    {
                        Text = string.IsNullOrWhiteSpace(fullText)
                            ? "I couldn't generate a response."
                            : fullText,
                        IsUser = false,
                        IsStreaming = false,
                        Timestamp = placeholder.Timestamp
                    };
                }

                _ = LoadConversationsAsync();
            }
            catch (Exception ex)
            {
                var errorIndex = Messages.IndexOf(placeholder);
                var errorMessage = $"Something went wrong.\n\n{ex.Message}";

                if (errorIndex >= 0)
                {
                    Messages[errorIndex] = new ChatMessage
                    {
                        Text = errorMessage,
                        IsUser = false,
                        Timestamp = DateTime.Now
                    };
                }
                else
                {
                    Messages.Add(new ChatMessage
                    {
                        Text = errorMessage,
                        IsUser = false,
                        Timestamp = DateTime.Now
                    });
                }
            }
            finally { IsBusy = false; }
        }

        // ============================================================
        // REGENERATE
        // ============================================================
        [RelayCommand]
        private async Task RegenerateAsync()
        {
            if (IsBusy) return;
            if (string.IsNullOrWhiteSpace(_lastUserPrompt)) return;

            if (Messages.Count > 0 && !Messages.Last().IsUser)
                Messages.RemoveAt(Messages.Count - 1);

            InputText = _lastUserPrompt;
            await SendAsync();
        }

        // ============================================================
        // COPY / SHARE / DOWNLOAD
        // ============================================================
        [RelayCommand]
        private async Task CopyMessageAsync(ChatMessage? message)
        {
            if (message is null || string.IsNullOrWhiteSpace(message.Text)) return;
            try
            {
                await Clipboard.Default.SetTextAsync(message.Text);
                await Shell.Current.DisplayAlert("Copied", "Response copied.", "OK");
            }
            catch (Exception ex)
            {
                await Shell.Current.DisplayAlert("Copy failed", ex.Message, "OK");
            }
        }

        [RelayCommand]
        private async Task ShareMessageAsync(ChatMessage? message)
        {
            if (message is null || string.IsNullOrWhiteSpace(message.Text)) return;
            try
            {
                await Share.Default.RequestAsync(new ShareTextRequest
                {
                    Text = message.Text,
                    Title = "Share from Ask Bofah"
                });
            }
            catch (Exception ex)
            {
                await Shell.Current.DisplayAlert("Share failed", ex.Message, "OK");
            }
        }

        [RelayCommand]
        private async Task DownloadMessageAsync(ChatMessage? message)
        {
            if (message is null || string.IsNullOrWhiteSpace(message.Text)) return;
            try
            {
                var fileName = $"AskBofah_{DateTime.Now:yyyyMMdd_HHmmss}.txt";
                var path = Path.Combine(FileSystem.CacheDirectory, fileName);
                await File.WriteAllTextAsync(path, message.Text);

                await Share.Default.RequestAsync(new ShareFileRequest
                {
                    Title = "Save Ask Bofah response",
                    File = new ShareFile(path)
                });
            }
            catch (Exception ex)
            {
                await Shell.Current.DisplayAlert("Save failed", ex.Message, "OK");
            }
        }

        // ============================================================
        // LOGOUT
        // ============================================================
        [RelayCommand]
        private async Task LogoutAsync()
        {
            if (IsBusy) return;

            var confirm = await Shell.Current.DisplayAlert(
                "Log out",
                "Are you sure you want to log out?",
                "Log out", "Cancel");

            if (!confirm) return;

            try
            {
                await _auth.LogoutAsync();
                await Shell.Current.GoToAsync("//login");
            }
            catch (Exception ex)
            {
                await Shell.Current.DisplayAlert("Logout failed", ex.Message, "OK");
            }
        }

        // ============================================================
        // NEW CHAT
        // ============================================================
        [RelayCommand]
        private void NewChat()
        {
            if (IsBusy) return;

            Messages.Clear();
            InputText = string.Empty;
            _lastUserPrompt = string.Empty;
            _currentConversationId = null;

            Messages.Add(new ChatMessage
            {
                Text = "New conversation started. What would you like to work on?",
                IsUser = false,
                IsStreaming = false,
                Timestamp = DateTime.Now
            });
        }

        // ============================================================
        // UPGRADE
        // ============================================================
        [RelayCommand]
        private async Task UpgradePlanAsync()
        {
            await Shell.Current.DisplayAlert(
                "Upgrade to Pro",
                "Pro plan unlocks unlimited messages, priority AI, and more.\n\nPayments will be available in the next release.",
                "Got it");
        }
    }
}
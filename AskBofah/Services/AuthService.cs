using AskBofah.Models;
using Newtonsoft.Json;

namespace AskBofah.Services
{
    public class AuthService
    {
        private const string UsersKey = "askbofah_local_users";
        private const string ResetTokensKey = "askbofah_reset_tokens";
        private readonly SecureStorageService _secure;

        public AuthService(SecureStorageService secure)
        {
            _secure = secure;
        }

        public User? CurrentUser { get; private set; }
        public bool IsLoggedIn => CurrentUser is not null;

        // ==================== REGISTER ====================
        public async Task<(bool ok, string error)> RegisterAsync(string name, string email, string password)
        {
            if (string.IsNullOrWhiteSpace(name)) return (false, "Name is required.");
            if (string.IsNullOrWhiteSpace(email)) return (false, "Email is required.");
            if (password.Length < 6) return (false, "Password must be at least 6 characters.");

            var users = LoadUsers();

            if (users.Any(u => u.Email.Equals(email.Trim(), StringComparison.OrdinalIgnoreCase)))
                return (false, "An account with this email already exists.");

            var user = new User
            {
                Id = Guid.NewGuid().ToString(),
                Name = name.Trim(),
                Email = email.Trim().ToLower(),
                Role = "student",
                CreatedAt = DateTime.Now
            };

            users.Add(user);
            SaveUsers(users);
            SavePassword(user.Email, password);

            CurrentUser = user;
            await _secure.SaveUserAsync(user, GenerateToken(user));
            return (true, string.Empty);
        }

        // ==================== LOGIN ====================
        public async Task<(bool ok, string error)> LoginAsync(string email, string password)
        {
            if (string.IsNullOrWhiteSpace(email)) return (false, "Email is required.");
            if (string.IsNullOrWhiteSpace(password)) return (false, "Password is required.");

            var users = LoadUsers();
            var user = users.FirstOrDefault(u =>
                u.Email.Equals(email.Trim(), StringComparison.OrdinalIgnoreCase));

            if (user is null) return (false, "No account found with this email.");

            var storedPassword = GetPassword(user.Email);
            if (storedPassword != password) return (false, "Incorrect password.");

            CurrentUser = user;
            await _secure.SaveUserAsync(user, GenerateToken(user));
            return (true, string.Empty);
        }

        // ==================== FORGOT PASSWORD ====================
        public Task<(bool ok, string error)> SendPasswordResetAsync(string email)
        {
            if (string.IsNullOrWhiteSpace(email))
                return Task.FromResult<(bool, string)>((false, "Email is required."));

            if (!email.Contains("@") || !email.Contains("."))
                return Task.FromResult<(bool, string)>((false, "Please enter a valid email address."));

            var users = LoadUsers();
            var user = users.FirstOrDefault(u =>
                u.Email.Equals(email.Trim(), StringComparison.OrdinalIgnoreCase));

            // Security best practice: don't reveal whether the email exists.
            // Simulate sending the reset link regardless.
            //
            // In a real backend, this would:
            // 1. Generate a reset token
            // 2. Store it with an expiry time
            // 3. Send an email with the reset URL
            //
            // For local auth, we store the token so we can support
            // the "reset password" step in the next iteration.

            if (user is not null)
            {
                var token = Guid.NewGuid().ToString("N");
                SaveResetToken(user.Email, token);
            }

            return Task.FromResult<(bool, string)>((true, string.Empty));
        }

        // ==================== SESSION ====================
        public async Task<bool> TryRestoreSessionAsync()
        {
            var user = await _secure.GetUserAsync();
            if (user is null) return false;
            CurrentUser = user;
            return true;
        }

        public Task LogoutAsync()
        {
            CurrentUser = null;
            _secure.ClearAll();
            return Task.CompletedTask;
        }

        // ==================== LOCAL STORAGE: USERS ====================
        private List<User> LoadUsers()
        {
            var json = Preferences.Default.Get(UsersKey, "[]");
            return JsonConvert.DeserializeObject<List<User>>(json) ?? new List<User>();
        }

        private void SaveUsers(List<User> users)
        {
            Preferences.Default.Set(UsersKey, JsonConvert.SerializeObject(users));
        }

        // ==================== LOCAL STORAGE: PASSWORDS ====================
        private void SavePassword(string email, string password)
        {
            var key = $"askbofah_pwd_{email.ToLower()}";
            Preferences.Default.Set(key, password);
        }

        private string GetPassword(string email)
        {
            var key = $"askbofah_pwd_{email.ToLower()}";
            return Preferences.Default.Get(key, string.Empty);
        }

        // ==================== LOCAL STORAGE: RESET TOKENS ====================
        private void SaveResetToken(string email, string token)
        {
            var tokens = LoadResetTokens();
            tokens[email.ToLower()] = new ResetToken
            {
                Token = token,
                ExpiresAt = DateTime.UtcNow.AddMinutes(15)
            };
            Preferences.Default.Set(ResetTokensKey, JsonConvert.SerializeObject(tokens));
        }

        public (bool ok, string error) ValidateResetToken(string email, string token)
        {
            var tokens = LoadResetTokens();
            if (!tokens.TryGetValue(email.ToLower(), out var stored))
                return (false, "No reset request found for this email.");

            if (stored.ExpiresAt < DateTime.UtcNow)
                return (false, "This reset link has expired. Please request a new one.");

            if (stored.Token != token)
                return (false, "Invalid reset token.");

            return (true, string.Empty);
        }

        public (bool ok, string error) ResetPassword(string email, string token, string newPassword)
        {
            var validation = ValidateResetToken(email, token);
            if (!validation.ok) return validation;

            if (string.IsNullOrWhiteSpace(newPassword) || newPassword.Length < 6)
                return (false, "Password must be at least 6 characters.");

            SavePassword(email.ToLower(), newPassword);

            // Invalidate the token after use
            var tokens = LoadResetTokens();
            tokens.Remove(email.ToLower());
            Preferences.Default.Set(ResetTokensKey, JsonConvert.SerializeObject(tokens));

            return (true, string.Empty);
        }

        private Dictionary<string, ResetToken> LoadResetTokens()
        {
            var json = Preferences.Default.Get(ResetTokensKey, "{}");
            return JsonConvert.DeserializeObject<Dictionary<string, ResetToken>>(json)
                   ?? new Dictionary<string, ResetToken>();
        }

        // ==================== TOKEN GENERATION ====================
        private static string GenerateToken(User user)
        {
            // Simple token for local auth — replace with real JWT when backend is added
            return Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(
                $"{user.Id}:{user.Email}:{DateTime.UtcNow.Ticks}"));
        }
    }

    public class ResetToken
    {
        public string Token { get; set; } = string.Empty;
        public DateTime ExpiresAt { get; set; }
    }
}
using AskBofah.Models;
using System.Text.Json.Serialization;

namespace AskBofah.Services
{
    public class AuthService
    {
        private readonly ApiClient _api;
        private readonly SecureStorageService _secure;

        public AuthService(ApiClient api, SecureStorageService secure)
        {
            _api = api;
            _secure = secure;
        }

        public User? CurrentUser { get; private set; }
        public bool IsLoggedIn => CurrentUser is not null;

        // ============================================================
        // REGISTER
        // ============================================================
        public async Task<(bool ok, string error)> RegisterAsync(
            string name,
            string email,
            string password)
        {
            try
            {
                var response = await _api.PostAsync<AuthResponse>(
                    "auth/register",
                    new
                    {
                        fullName = name.Trim(),
                        email = email.Trim().ToLowerInvariant(),
                        password = password
                    });

                if (response is null || string.IsNullOrEmpty(response.Token))
                    return (false, "Registration failed. Please try again.");

                CurrentUser = response.User;
                await _secure.SaveUserAsync(response.User, response.Token);
                return (true, string.Empty);
            }
            catch (ApiException ex)
            {
                return (false, ex.Message);
            }
            catch (Exception ex)
            {
                return (false, $"Connection error: {ex.Message}");
            }
        }

        // ============================================================
        // LOGIN
        // ============================================================
        public async Task<(bool ok, string error)> LoginAsync(
            string email,
            string password)
        {
            try
            {
                var response = await _api.PostAsync<AuthResponse>(
                    "auth/login",
                    new
                    {
                        email = email.Trim().ToLowerInvariant(),
                        password = password
                    });

                if (response is null || string.IsNullOrEmpty(response.Token))
                    return (false, "Login failed. Please try again.");

                CurrentUser = response.User;
                await _secure.SaveUserAsync(response.User, response.Token);
                return (true, string.Empty);
            }
            catch (ApiException ex)
            {
                return (false, ex.Message);
            }
            catch (Exception ex)
            {
                return (false, $"Connection error: {ex.Message}");
            }
        }

        // ============================================================
        // RESTORE SESSION
        // ============================================================
        public async Task<bool> TryRestoreSessionAsync()
        {
            var token = await _secure.GetTokenAsync();
            if (string.IsNullOrEmpty(token))
                return false;

            try
            {
                var user = await _api.GetAsync<User>("auth/me");
                if (user is null)
                {
                    await LogoutAsync();
                    return false;
                }

                CurrentUser = user;
                return true;
            }
            catch (ApiException ex) when (ex.StatusCode == 401)
            {
                await LogoutAsync();
                return false;
            }
            catch
            {
                return false;
            }
        }

        // ============================================================
        // FORGOT PASSWORD — placeholder
        // ============================================================
        public Task<(bool ok, string error)> SendPasswordResetAsync(string email)
        {
            return Task.FromResult<(bool, string)>((true, string.Empty));
        }

        // ============================================================
        // LOGOUT
        // ============================================================
        public Task LogoutAsync()
        {
            CurrentUser = null;
            _secure.ClearAll();
            return Task.CompletedTask;
        }

        // ============================================================
        // DTO — now using System.Text.Json attributes
        // ============================================================
        private class AuthResponse
        {
            [JsonPropertyName("token")]
            public string Token { get; set; } = string.Empty;

            [JsonPropertyName("user")]
            public User User { get; set; } = new();
        }
    }
}
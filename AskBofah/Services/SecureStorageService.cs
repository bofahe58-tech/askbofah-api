using AskBofah.Helpers;
using AskBofah.Models;

namespace AskBofah.Services
{
    public class SecureStorageService
    {
        public async Task SaveUserAsync(User user, string token)
        {
            try
            {
                await SecureStorage.Default.SetAsync(AppConstants.SecureKeyToken, token);
                await SecureStorage.Default.SetAsync(AppConstants.SecureKeyUserId, user.Id);
                await SecureStorage.Default.SetAsync(AppConstants.SecureKeyUserEmail, user.Email);
                await SecureStorage.Default.SetAsync(AppConstants.SecureKeyUserName, user.Name);
            }
            catch { }
        }

        public async Task<User?> GetUserAsync()
        {
            try
            {
                var id = await SecureStorage.Default.GetAsync(AppConstants.SecureKeyUserId);
                var email = await SecureStorage.Default.GetAsync(AppConstants.SecureKeyUserEmail);
                var name = await SecureStorage.Default.GetAsync(AppConstants.SecureKeyUserName);

                if (string.IsNullOrEmpty(id) || string.IsNullOrEmpty(email))
                    return null;

                return new User
                {
                    Id = id,
                    Email = email,
                    Name = name ?? "User"
                };
            }
            catch { return null; }
        }

        public async Task<string?> GetTokenAsync()
        {
            try { return await SecureStorage.Default.GetAsync(AppConstants.SecureKeyToken); }
            catch { return null; }
        }

        public bool HasToken()
        {
            try
            {
                var token = SecureStorage.Default.GetAsync(AppConstants.SecureKeyToken).Result;
                return !string.IsNullOrEmpty(token);
            }
            catch { return false; }
        }

        public void ClearAll()
        {
            try
            {
                SecureStorage.Default.Remove(AppConstants.SecureKeyToken);
                SecureStorage.Default.Remove(AppConstants.SecureKeyUserId);
                SecureStorage.Default.Remove(AppConstants.SecureKeyUserEmail);
                SecureStorage.Default.Remove(AppConstants.SecureKeyUserName);
            }
            catch { }
        }

        public void SaveChatHistory(string json)
        {
            Preferences.Default.Set(AppConstants.PrefKeyConversationHistory, json);
        }

        public string GetChatHistory()
        {
            return Preferences.Default.Get(AppConstants.PrefKeyConversationHistory, string.Empty);
        }

        public void ClearChatHistory()
        {
            Preferences.Default.Remove(AppConstants.PrefKeyConversationHistory);
        }
    }
}
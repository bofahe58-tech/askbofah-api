using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using AskBofah.Helpers;

namespace AskBofah.Services
{
    public class ApiClient
    {
        private readonly HttpClient _http;
        private readonly SecureStorageService _secure;

        private static readonly JsonSerializerOptions JsonOptions = new()
        {
            PropertyNameCaseInsensitive = true,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };

        public ApiClient(SecureStorageService secure)
        {
            _secure = secure;

            _http = new HttpClient
            {
                BaseAddress = new Uri(AppConstants.ApiBaseUrl.TrimEnd('/') + "/"),
                Timeout = TimeSpan.FromMinutes(5)
            };
        }

        private async Task ApplyAuthHeaderAsync()
        {
            var token = await _secure.GetTokenAsync();
            _http.DefaultRequestHeaders.Authorization = string.IsNullOrEmpty(token)
                ? null
                : new AuthenticationHeaderValue("Bearer", token);
        }

        public async Task<T?> GetAsync<T>(string endpoint)
        {
            await ApplyAuthHeaderAsync();
            var response = await _http.GetAsync(endpoint);
            return await HandleResponseAsync<T>(response);
        }

        public async Task<T?> PostAsync<T>(string endpoint, object? body = null)
        {
            await ApplyAuthHeaderAsync();
            var content = body is null
                ? null
                : new StringContent(JsonSerializer.Serialize(body, JsonOptions),
                    Encoding.UTF8, "application/json");

            var response = await _http.PostAsync(endpoint, content);
            return await HandleResponseAsync<T>(response);
        }

        public async Task<T?> PutAsync<T>(string endpoint, object? body = null)
        {
            await ApplyAuthHeaderAsync();
            var content = body is null
                ? null
                : new StringContent(JsonSerializer.Serialize(body, JsonOptions),
                    Encoding.UTF8, "application/json");

            var response = await _http.PutAsync(endpoint, content);
            return await HandleResponseAsync<T>(response);
        }

        public async Task DeleteAsync(string endpoint)
        {
            await ApplyAuthHeaderAsync();
            var response = await _http.DeleteAsync(endpoint);

            if (!response.IsSuccessStatusCode)
            {
                var error = await response.Content.ReadAsStringAsync();
                throw new ApiException((int)response.StatusCode, error);
            }
        }

        public async Task<Stream> PostStreamAsync(string endpoint, object body)
        {
            await ApplyAuthHeaderAsync();

            var content = new StringContent(
                JsonSerializer.Serialize(body, JsonOptions),
                Encoding.UTF8,
                "application/json");

            var request = new HttpRequestMessage(HttpMethod.Post, endpoint)
            {
                Content = content
            };

            var response = await _http.SendAsync(
                request,
                HttpCompletionOption.ResponseHeadersRead);

            if (!response.IsSuccessStatusCode)
            {
                var error = await response.Content.ReadAsStringAsync();
                throw new ApiException((int)response.StatusCode, error);
            }

            return await response.Content.ReadAsStreamAsync();
        }

        private static async Task<T?> HandleResponseAsync<T>(HttpResponseMessage response)
        {
            var raw = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
            {
                string message = $"HTTP {(int)response.StatusCode}";
                try
                {
                    using var doc = JsonDocument.Parse(raw);
                    if (doc.RootElement.TryGetProperty("message", out var msg))
                        message = msg.GetString() ?? message;
                }
                catch { }
                throw new ApiException((int)response.StatusCode, message);
            }

            if (string.IsNullOrWhiteSpace(raw))
                return default;

            return JsonSerializer.Deserialize<T>(raw, JsonOptions);
        }
    }

    public class ApiException : Exception
    {
        public int StatusCode { get; }

        public ApiException(int statusCode, string message) : base(message)
        {
            StatusCode = statusCode;
        }
    }
}
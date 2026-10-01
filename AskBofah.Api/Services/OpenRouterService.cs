using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

namespace AskBofah.Api.Services
{
    public class OpenRouterService
    {
        private readonly HttpClient _http;
        private readonly IConfiguration _config;
        private readonly string _baseUrl;

        public OpenRouterService(HttpClient http, IConfiguration config)
        {
            _http = http;
            _config = config;

            var apiKey = _config["OpenRouter:ApiKey"]
                ?? throw new InvalidOperationException("OpenRouter API key not configured.");

            // Store base URL separately, don't set BaseAddress
            // (avoids the trailing-slash concatenation bug)
            _baseUrl = (_config["OpenRouter:BaseUrl"] ?? "https://openrouter.ai/api/v1")
                .TrimEnd('/');

            _http.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", apiKey);
            _http.DefaultRequestHeaders.Add("HTTP-Referer", "https://askbofah.app");
            _http.DefaultRequestHeaders.Add("X-Title", "Ask Bofah");
            _http.Timeout = TimeSpan.FromMinutes(5);
        }

        public async IAsyncEnumerable<string> StreamChatAsync(
            List<(string role, string content)> messages,
            string? model = null)
        {
            var chosenModel = model
                ?? _config["OpenRouter:DefaultModel"]
                ?? "openai/gpt-4o-mini";

            var body = new
            {
                model = chosenModel,
                messages = messages
                    .Select(m => new { role = m.role, content = m.content })
                    .ToArray(),
                stream = true,
                temperature = 0.7
            };

            var json = JsonSerializer.Serialize(body);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            // Build the FULL URL here
            var fullUrl = $"{_baseUrl}/chat/completions";

            var request = new HttpRequestMessage(HttpMethod.Post, fullUrl)
            {
                Content = content
            };

            var response = await _http.SendAsync(
                request,
                HttpCompletionOption.ResponseHeadersRead);

            if (!response.IsSuccessStatusCode)
            {
                var error = await response.Content.ReadAsStringAsync();
                throw new Exception($"OpenRouter error {(int)response.StatusCode}: {error}");
            }

            var stream = await response.Content.ReadAsStreamAsync();
            using var reader = new StreamReader(stream);

            while (!reader.EndOfStream)
            {
                var line = await reader.ReadLineAsync();
                if (string.IsNullOrWhiteSpace(line)) continue;
                if (!line.StartsWith("data: ")) continue;

                var data = line.Substring(6).Trim();
                if (data == "[DONE]") yield break;

                string? token = null;
                try
                {
                    using var doc = JsonDocument.Parse(data);
                    var choices = doc.RootElement.GetProperty("choices");
                    if (choices.GetArrayLength() > 0)
                    {
                        var delta = choices[0].GetProperty("delta");
                        if (delta.TryGetProperty("content", out var contentEl))
                        {
                            token = contentEl.GetString();
                        }
                    }
                }
                catch { continue; }

                if (!string.IsNullOrEmpty(token))
                    yield return token;
            }
        }
    }
}
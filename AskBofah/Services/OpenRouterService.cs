using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using AskBofah.Helpers;

namespace AskBofah.Services
{
    public class OpenRouterService
    {
        private readonly HttpClient _http;

        public OpenRouterService()
        {
            _http = new HttpClient();
            _http.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", AppConstants.OpenRouterApiKey);
            _http.DefaultRequestHeaders.Add("HTTP-Referer", AppConstants.AppReferer);
            _http.DefaultRequestHeaders.Add("X-Title", AppConstants.AppName);
            _http.Timeout = TimeSpan.FromMinutes(5);
        }

        public async IAsyncEnumerable<string> StreamChatAsync(
            List<(string role, string content)> messages,
            string model = AppConstants.DefaultModel)
        {
            var body = new
            {
                model,
                messages = messages.Select(m => new { role = m.role, content = m.content }).ToArray(),
                stream = true,
                temperature = 0.7
            };

            var json = JsonSerializer.Serialize(body);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var request = new HttpRequestMessage(
                HttpMethod.Post,
                $"{AppConstants.OpenRouterBaseUrl}/chat/completions")
            {
                Content = content
            };

            var response = await _http.SendAsync(request, HttpCompletionOption.ResponseHeadersRead);

            if (!response.IsSuccessStatusCode)
            {
                var err = await response.Content.ReadAsStringAsync();
                throw new Exception($"OpenRouter error {(int)response.StatusCode}: {err}");
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
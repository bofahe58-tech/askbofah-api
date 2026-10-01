using System.Text.Json;
using AskBofah.Models;

namespace AskBofah.Services
{
    public class ChatService
    {
        private readonly ApiClient _api;

        public ChatService(ApiClient api)
        {
            _api = api;
        }

        // ============================================================
        // STREAM CHAT — streams tokens from /api/chat/send
        // ============================================================
        public async IAsyncEnumerable<string> StreamChatAsync(
            string content,
            Guid? conversationId,
            Action<Guid>? onConversationIdReceived = null)
        {
            var body = new
            {
                content = content,
                conversationId = conversationId
            };

            var stream = await _api.PostStreamAsync("chat/send", body);
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

                    // First SSE message carries the conversation ID
                    if (doc.RootElement.TryGetProperty("conversationId", out var convId))
                    {
                        if (Guid.TryParse(convId.GetString(), out var id))
                            onConversationIdReceived?.Invoke(id);
                        continue;
                    }

                    // Streaming tokens
                    if (doc.RootElement.TryGetProperty("token", out var tokenEl))
                    {
                        token = tokenEl.GetString();
                    }

                    // Errors
                    if (doc.RootElement.TryGetProperty("error", out var errEl))
                    {
                        throw new Exception(errEl.GetString() ?? "Unknown error from AI");
                    }
                }
                catch (JsonException) { continue; }

                if (!string.IsNullOrEmpty(token))
                    yield return token;
            }
        }

        // ============================================================
        // GET ALL CONVERSATIONS (sidebar history)
        // ============================================================
        public async Task<List<ConversationSummary>> GetConversationsAsync()
        {
            var list = await _api.GetAsync<List<ConversationSummary>>("conversations");
            return list ?? new List<ConversationSummary>();
        }

        // ============================================================
        // GET ONE CONVERSATION (with full message list)
        // ============================================================
        public async Task<ConversationDetail?> GetConversationAsync(Guid id)
        {
            return await _api.GetAsync<ConversationDetail>($"conversations/{id}");
        }

        // ============================================================
        // DELETE A CONVERSATION
        // ============================================================
        public async Task DeleteConversationAsync(Guid id)
        {
            await _api.DeleteAsync($"conversations/{id}");
        }
    }
}
using AskBofah.Api.Data;
using AskBofah.Api.DTOs;
using AskBofah.Api.Models;
using AskBofah.Api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AskBofah.Api.Controllers
{
    [ApiController]
    [Route("api/chat")]
    [Authorize]
    public class ChatController : ControllerBase
    {
        private readonly AppDbContext _db;
        private readonly OpenRouterService _ai;

        public ChatController(AppDbContext db, OpenRouterService ai)
        {
            _db = db;
            _ai = ai;
        }

        // ============================================================
        // POST /api/chat/send
        // Streams the AI response as Server-Sent Events (SSE).
        // ============================================================
        [HttpPost("send")]
        public async Task Send([FromBody] SendMessageRequest request)
        {
            var userId = (Guid)HttpContext.Items["UserId"]!;

            var user = await _db.Users.FindAsync(userId);
            if (user is null)
            {
                Response.StatusCode = 401;
                await Response.WriteAsync("User not found");
                return;
            }

            // ============================================================
            // PLAN LIMIT ENFORCEMENT
            // ============================================================
            var today = DateTime.UtcNow.Date;
            if (user.LastMessageDate < today)
            {
                user.MessagesSentToday = 0;
                user.LastMessageDate = today;
            }

            var dailyLimit = AuthController.GetDailyLimit(user.PlanTier);
            if (user.MessagesSentToday >= dailyLimit)
            {
                Response.StatusCode = 429;
                await Response.WriteAsync(
                    $"Daily limit reached. You've sent {dailyLimit} messages today on the {user.PlanTier} plan. Upgrade to send more.");
                return;
            }

            // ============================================================
            // GET OR CREATE CONVERSATION
            // ============================================================
            Conversation? conversation = null;

            if (request.ConversationId.HasValue)
            {
                conversation = await _db.Conversations
                    .Include(c => c.Messages)
                    .FirstOrDefaultAsync(c => c.Id == request.ConversationId.Value
                                           && c.UserId == userId);
            }

            if (conversation is null)
            {
                conversation = new Conversation
                {
                    UserId = userId,
                    Title = GenerateTitle(request.Content),
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };
                _db.Conversations.Add(conversation);
                await _db.SaveChangesAsync();
            }

            // ============================================================
            // SAVE USER MESSAGE
            // ============================================================
            var userMessage = new Message
            {
                ConversationId = conversation.Id,
                Role = "user",
                Content = request.Content,
                CreatedAt = DateTime.UtcNow
            };
            _db.Messages.Add(userMessage);

            user.MessagesSentToday++;
            conversation.UpdatedAt = DateTime.UtcNow;

            await _db.SaveChangesAsync();

            // ============================================================
            // BUILD HISTORY FOR AI
            // ============================================================
            var history = conversation.Messages
                .OrderBy(m => m.CreatedAt)
                .Select(m => (m.Role, m.Content))
                .ToList();

            history.Add(("user", request.Content));

            // ============================================================
            // STREAM THE RESPONSE (SSE)
            // ============================================================
            Response.Headers.Append("Content-Type", "text/event-stream");
            Response.Headers.Append("Cache-Control", "no-cache");
            Response.Headers.Append("X-Accel-Buffering", "no");

            // First, tell the client the conversation ID
            await Response.WriteAsync($"data: {{\"conversationId\":\"{conversation.Id}\"}}\n\n");
            await Response.Body.FlushAsync();

            var assistantResponse = new System.Text.StringBuilder();

            try
            {
                await foreach (var token in _ai.StreamChatAsync(history))
                {
                    assistantResponse.Append(token);
                    var escaped = token
                        .Replace("\\", "\\\\")
                        .Replace("\"", "\\\"")
                        .Replace("\n", "\\n")
                        .Replace("\r", "\\r");

                    await Response.WriteAsync($"data: {{\"token\":\"{escaped}\"}}\n\n");
                    await Response.Body.FlushAsync();
                }
            }
            catch (Exception ex)
            {
                await Response.WriteAsync($"data: {{\"error\":\"{ex.Message}\"}}\n\n");
                await Response.Body.FlushAsync();
            }

            // ============================================================
            // SAVE ASSISTANT MESSAGE
            // ============================================================
            var assistantMessage = new Message
            {
                ConversationId = conversation.Id,
                Role = "assistant",
                Content = assistantResponse.ToString(),
                CreatedAt = DateTime.UtcNow
            };
            _db.Messages.Add(assistantMessage);
            conversation.UpdatedAt = DateTime.UtcNow;
            await _db.SaveChangesAsync();

            await Response.WriteAsync("data: [DONE]\n\n");
            await Response.Body.FlushAsync();
        }

        // ============================================================
        // Helper
        // ============================================================
        private static string GenerateTitle(string firstMessage)
        {
            var trimmed = firstMessage.Trim();
            if (trimmed.Length <= 60)
                return trimmed;
            return trimmed.Substring(0, 57) + "...";
        }
    }
}
using AskBofah.Api.Data;
using AskBofah.Api.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AskBofah.Api.Controllers
{
    [ApiController]
    [Route("api/conversations")]
    [Authorize]
    public class ConversationsController : ControllerBase
    {
        private readonly AppDbContext _db;

        public ConversationsController(AppDbContext db)
        {
            _db = db;
        }

        // ============================================================
        // GET /api/conversations
        // ============================================================
        [HttpGet]
        public async Task<IActionResult> List()
        {
            var userId = (Guid)HttpContext.Items["UserId"]!;

            var conversations = await _db.Conversations
                .Where(c => c.UserId == userId && !c.IsArchived)
                .OrderByDescending(c => c.UpdatedAt)
                .Select(c => new ConversationDto
                {
                    Id = c.Id,
                    Title = c.Title,
                    CreatedAt = c.CreatedAt,
                    UpdatedAt = c.UpdatedAt,
                    MessageCount = c.Messages.Count
                })
                .ToListAsync();

            return Ok(conversations);
        }

        // ============================================================
        // GET /api/conversations/{id}
        // ============================================================
        [HttpGet("{id}")]
        public async Task<IActionResult> Get(Guid id)
        {
            var userId = (Guid)HttpContext.Items["UserId"]!;

            var conversation = await _db.Conversations
                .Include(c => c.Messages.OrderBy(m => m.CreatedAt))
                .FirstOrDefaultAsync(c => c.Id == id && c.UserId == userId);

            if (conversation is null)
                return NotFound(new { message = "Conversation not found." });

            return Ok(new ConversationDetailDto
            {
                Id = conversation.Id,
                Title = conversation.Title,
                CreatedAt = conversation.CreatedAt,
                Messages = conversation.Messages.Select(m => new MessageDto
                {
                    Id = m.Id,
                    Role = m.Role,
                    Content = m.Content,
                    CreatedAt = m.CreatedAt
                }).ToList()
            });
        }

        // ============================================================
        // DELETE /api/conversations/{id}
        // ============================================================
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            var userId = (Guid)HttpContext.Items["UserId"]!;

            var conversation = await _db.Conversations
                .FirstOrDefaultAsync(c => c.Id == id && c.UserId == userId);

            if (conversation is null)
                return NotFound(new { message = "Conversation not found." });

            _db.Conversations.Remove(conversation);
            await _db.SaveChangesAsync();

            return Ok(new { message = "Conversation deleted." });
        }
    }
}
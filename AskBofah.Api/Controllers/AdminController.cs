using AskBofah.Api.Data;
using AskBofah.Api.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AskBofah.Api.Controllers
{
    [ApiController]
    [Route("api/admin")]
    [Authorize]
    public class AdminController : ControllerBase
    {
        private readonly AppDbContext _db;

        public AdminController(AppDbContext db)
        {
            _db = db;
        }

        // ============================================================
        // GET /api/admin/users — list all users (admin only)
        // ============================================================
        [HttpGet("users")]
        public async Task<IActionResult> GetAllUsers()
        {
            var currentUserId = (Guid)HttpContext.Items["UserId"]!;

            var caller = await _db.Users.FindAsync(currentUserId);
            if (caller is null || !caller.IsAdmin)
                return Forbid();

            var users = await _db.Users
                .OrderByDescending(u => u.CreatedAt)
                .Select(u => new
                {
                    id = u.Id,
                    fullName = u.FullName,
                    email = u.Email,
                    planTier = u.PlanTier,
                    isAdmin = u.IsAdmin,
                    messagesSentToday = u.MessagesSentToday,
                    createdAt = u.CreatedAt,
                    lastLoginAt = u.LastLoginAt,
                    subscriptionExpiresAt = u.SubscriptionExpiresAt
                })
                .ToListAsync();

            return Ok(users);
        }

        // ============================================================
        // PATCH /api/admin/users/{id}/role — promote/demote (admin only)
        // ============================================================
        [HttpPatch("users/{id}/role")]
        public async Task<IActionResult> UpdateRole(Guid id, [FromBody] UpdateRoleRequest req)
        {
            var currentUserId = (Guid)HttpContext.Items["UserId"]!;
            var caller = await _db.Users.FindAsync(currentUserId);
            if (caller is null || !caller.IsAdmin) return Forbid();

            if (id == currentUserId)
                return BadRequest(new { message = "You cannot change your own role." });

            var user = await _db.Users.FindAsync(id);
            if (user is null) return NotFound(new { message = "User not found." });

            user.IsAdmin = req.IsAdmin;
            await _db.SaveChangesAsync();

            return Ok(new { message = "Role updated.", isAdmin = user.IsAdmin });
        }

        // ============================================================
        // DELETE /api/admin/users/{id} — delete user (admin only)
        // ============================================================
        [HttpDelete("users/{id}")]
        public async Task<IActionResult> DeleteUser(Guid id)
        {
            var currentUserId = (Guid)HttpContext.Items["UserId"]!;
            var caller = await _db.Users.FindAsync(currentUserId);
            if (caller is null || !caller.IsAdmin) return Forbid();

            if (id == currentUserId)
                return BadRequest(new { message = "You cannot delete yourself." });

            var user = await _db.Users.FindAsync(id);
            if (user is null) return NotFound(new { message = "User not found." });

            _db.Users.Remove(user);
            await _db.SaveChangesAsync();

            return Ok(new { message = "User deleted." });
        }

        // ============================================================
        // POST /api/admin/become-admin
        // TEMP: self-promote to admin. Remove before final submission.
        // ============================================================
        [HttpPost("become-admin")]
        [AllowAnonymous]
        public async Task<IActionResult> BecomeAdmin()
        {
            // Check we have a valid JWT-derived UserId
            var userIdObj = HttpContext.Items["UserId"];
            if (userIdObj is not Guid userId)
                return Unauthorized(new { message = "You must be logged in." });

            var user = await _db.Users.FindAsync(userId);
            if (user is null)
                return NotFound(new { message = "User not found." });

            user.IsAdmin = true;
            await _db.SaveChangesAsync();

            return Ok(new
            {
                message = "You are now an admin.",
                email = user.Email
            });
        }
    }
}
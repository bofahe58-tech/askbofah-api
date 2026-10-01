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
    [Route("api/auth")]
    public class AuthController : ControllerBase
    {
        private readonly AppDbContext _db;
        private readonly JwtService _jwt;
        private readonly PasswordService _password;

        public AuthController(AppDbContext db, JwtService jwt, PasswordService password)
        {
            _db = db;
            _jwt = jwt;
            _password = password;
        }

        // ============================================================
        // POST /api/auth/register
        // ============================================================
        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterRequest request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var email = request.Email.Trim().ToLowerInvariant();

            var exists = await _db.Users.AnyAsync(u => u.Email == email);
            if (exists)
                return Conflict(new { message = "An account with this email already exists." });

            var user = new User
            {
                FullName = request.FullName.Trim(),
                Email = email,
                PasswordHash = _password.Hash(request.Password),
                PlanTier = "Free",
                CreatedAt = DateTime.UtcNow,
                LastLoginAt = DateTime.UtcNow
            };

            _db.Users.Add(user);
            await _db.SaveChangesAsync();

            var token = _jwt.GenerateToken(user);

            return Ok(new AuthResponse
            {
                Token = token,
                User = MapToDto(user)
            });
        }

        // ============================================================
        // POST /api/auth/login
        // ============================================================
        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequest request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var email = request.Email.Trim().ToLowerInvariant();

            var user = await _db.Users.FirstOrDefaultAsync(u => u.Email == email);
            if (user is null)
                return Unauthorized(new { message = "Incorrect email or password." });

            var ok = _password.Verify(request.Password, user.PasswordHash);
            if (!ok)
                return Unauthorized(new { message = "Incorrect email or password." });

            user.LastLoginAt = DateTime.UtcNow;
            await _db.SaveChangesAsync();

            var token = _jwt.GenerateToken(user);

            return Ok(new AuthResponse
            {
                Token = token,
                User = MapToDto(user)
            });
        }

        // ============================================================
        // GET /api/auth/me  (requires JWT)
        // ============================================================
        [Authorize]
        [HttpGet("me")]
        public async Task<IActionResult> Me()
        {
            var userId = HttpContext.Items["UserId"] as Guid?;
            if (userId is null)
                return Unauthorized(new { message = "Invalid token." });

            var user = await _db.Users.FindAsync(userId.Value);
            if (user is null)
                return Unauthorized(new { message = "User not found." });

            return Ok(MapToDto(user));
        }

        // ============================================================
        // Helper
        // ============================================================
        private static UserDto MapToDto(User user) => new()
        {
            Id = user.Id,
            FullName = user.FullName,
            Email = user.Email,
            PlanTier = user.PlanTier,
            SubscriptionExpiresAt = user.SubscriptionExpiresAt,
            MessagesSentToday = user.MessagesSentToday,
            DailyMessageLimit = GetDailyLimit(user.PlanTier)
        };

        public static int GetDailyLimit(string planTier) => planTier switch
        {
            "Unlimited" => 1000,
            "Pro" => 200,
            _ => 50 // Free
        };
    }
}
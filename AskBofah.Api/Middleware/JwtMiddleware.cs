using AskBofah.Api.Services;

namespace AskBofah.Api.Middleware
{
    public class JwtMiddleware
    {
        private readonly RequestDelegate _next;

        public JwtMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task Invoke(HttpContext context, JwtService jwtService)
        {
            var token = context.Request.Headers["Authorization"]
                .FirstOrDefault()?.Split(" ").Last();

            if (!string.IsNullOrEmpty(token))
            {
                var userId = jwtService.GetUserIdFromToken(token);
                if (userId.HasValue)
                {
                    context.Items["UserId"] = userId.Value;
                }
            }

            await _next(context);
        }
    }
}
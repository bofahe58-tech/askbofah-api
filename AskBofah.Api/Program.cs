using AskBofah.Api.Data;
using AskBofah.Api.Middleware;
using AskBofah.Api.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// ============================================================
// PORT BINDING (Render provides PORT env var)
// ============================================================
var port = Environment.GetEnvironmentVariable("PORT");
if (!string.IsNullOrEmpty(port))
{
    builder.WebHost.UseUrls($"http://+:{port}");
}

// ============================================================
// DATABASE — PostgreSQL via EF Core
// ============================================================
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
    ?? throw new InvalidOperationException("Connection string not configured.");

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(connectionString));

// ============================================================
// SERVICES
// ============================================================
builder.Services.AddSingleton<JwtService>();
builder.Services.AddSingleton<PasswordService>();
builder.Services.AddHttpClient<OpenRouterService>();

// ============================================================
// AUTHENTICATION — JWT
// ============================================================
var jwtSecret = builder.Configuration["Jwt:Secret"]
    ?? throw new InvalidOperationException("JWT secret not configured.");

builder.Services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = builder.Configuration["Jwt:Issuer"],
            ValidAudience = builder.Configuration["Jwt:Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(jwtSecret))
        };
    });

builder.Services.AddAuthorization();

// ============================================================
// CORS — allow the MAUI app to call us
// ============================================================
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

// ============================================================
// CONTROLLERS + JSON
// ============================================================
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.PropertyNamingPolicy =
            System.Text.Json.JsonNamingPolicy.CamelCase;
    });

var app = builder.Build();

// ============================================================
// PIPELINE
// ============================================================
app.UseCors("AllowAll");

app.UseAuthentication();
app.UseAuthorization();

// Custom middleware: extracts UserId from JWT into context.Items["UserId"]
app.UseMiddleware<JwtMiddleware>();

app.MapControllers();

// ============================================================
// HEALTH CHECK
// ============================================================
app.MapGet("/", () => new
{
    status = "OK",
    service = "Ask Bofah API",
    version = "1.0",
    environment = app.Environment.EnvironmentName,
    timestamp = DateTime.UtcNow
});

// ============================================================
// AUTO-MIGRATE on startup
// ============================================================
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    try
    {
        db.Database.Migrate();
        Console.WriteLine("✅ Database migrations applied.");
    }
    catch (Exception ex)
    {
        Console.WriteLine($"⚠️ Migration failed: {ex.Message}");
        // Print full stack so we can debug Render deploy issues
        Console.WriteLine(ex.ToString());
    }
}

app.Run();
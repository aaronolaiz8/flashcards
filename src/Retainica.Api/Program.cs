using System.Text;
using Retainica.Api.Data;
using Retainica.Api.Infrastructure;
using Retainica.Api.Jobs;
using Retainica.Api.Middleware;
using Retainica.Api.Services;
using Retainica.Api.Services.Ai;
using Retainica.Api.Services.Interfaces;
using Hangfire;
using Hangfire.PostgreSql;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

var builder = WebApplication.CreateBuilder(args);
var config = builder.Configuration;

// Bind to the host-provided PORT (Render/containers) when present.
var port = Environment.GetEnvironmentVariable("PORT");
if (!string.IsNullOrWhiteSpace(port))
    builder.WebHost.UseUrls($"http://0.0.0.0:{port}");

// Database — Postgres everywhere (Neon in prod, local Postgres in dev).
builder.Services.AddDbContext<AppDbContext>(opt =>
    opt.UseNpgsql(config.GetConnectionString("DefaultConnection")));

// Hangfire is gated off for the MVP (reminders deferred). Enable via Features:Hangfire=true.
var hangfireEnabled = config.GetValue<bool>("Features:Hangfire");
if (hangfireEnabled)
{
    builder.Services.AddHangfire(hf =>
        hf.UsePostgreSqlStorage(config.GetConnectionString("DefaultConnection")));
    builder.Services.AddHangfireServer();
}

// JWT Authentication
var jwtKey = config["Jwt:Key"] ?? throw new InvalidOperationException("Jwt:Key not configured");
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(opt =>
    {
        opt.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey)),
            ValidateIssuer = true,
            ValidIssuer = config["Jwt:Issuer"],
            ValidateAudience = true,
            ValidAudience = config["Jwt:Audience"],
            ValidateLifetime = true,
            ClockSkew = TimeSpan.Zero
        };
    });

builder.Services.AddAuthorization();
builder.Services.AddControllers();

// CORS — allow frontend in dev
builder.Services.AddCors(opt =>
{
    opt.AddPolicy("Frontend", policy =>
    {
        var origins = config.GetSection("Cors:AllowedOrigins").Get<string[]>() ?? [];
        policy.WithOrigins(origins).AllowAnyMethod().AllowAnyHeader().AllowCredentials();
    });
});

// Services
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IDeckService, DeckService>();
builder.Services.AddScoped<ICardService, CardService>();
builder.Services.AddScoped<IStudyService, StudyService>();
builder.Services.AddScoped<IGoalService, GoalService>();
builder.Services.AddScoped<IReminderService, ReminderService>();
builder.Services.AddScoped<IAnalyticsService, AnalyticsService>();
builder.Services.AddScoped<IAiService, AiService>();
builder.Services.AddScoped<IAiSettingsService, AiSettingsService>();
builder.Services.AddScoped<IFsrsService, FsrsService>();
builder.Services.AddScoped<IEmailService, EmailService>();
builder.Services.AddScoped<ReminderJob>();

// AI card generation: at-rest key encryption + provider HTTP clients.
builder.Services.AddSingleton<IEncryptionService, AesGcmEncryptionService>();
builder.Services.AddHttpClient("ai", c => c.Timeout = TimeSpan.FromSeconds(100));
builder.Services.AddScoped<IAiProvider, AnthropicProvider>();
builder.Services.AddScoped<IAiProvider, OpenAiProvider>();
builder.Services.AddScoped<IAiProviderFactory, AiProviderFactory>();

var app = builder.Build();

app.UseMiddleware<ExceptionMiddleware>();
app.UseCors("Frontend");
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

if (hangfireEnabled)
{
    app.UseHangfireDashboard("/hangfire", new DashboardOptions
    {
        Authorization = [new HangfireAuthFilter()]
    });
}

// Apply migrations on startup
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    db.Database.Migrate();
}

app.Run();

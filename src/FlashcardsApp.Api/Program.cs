using System.Text;
using FlashcardsApp.Api.Data;
using FlashcardsApp.Api.Infrastructure;
using FlashcardsApp.Api.Jobs;
using FlashcardsApp.Api.Middleware;
using FlashcardsApp.Api.Services;
using FlashcardsApp.Api.Services.Interfaces;
using Hangfire;
using Hangfire.PostgreSql;
using Hangfire.SqlServer;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

var builder = WebApplication.CreateBuilder(args);
var config = builder.Configuration;
var isProduction = builder.Environment.IsProduction();

// Database — SQL Server locally, Postgres in production
if (isProduction)
{
    builder.Services.AddDbContext<AppDbContext>(opt =>
        opt.UseNpgsql(config.GetConnectionString("DefaultConnection")));
}
else
{
    builder.Services.AddDbContext<AppDbContext>(opt =>
        opt.UseSqlServer(config.GetConnectionString("DefaultConnection")));
}

// Hangfire — SQL Server locally, Postgres in production
builder.Services.AddHangfire(hf =>
{
    if (isProduction)
        hf.UsePostgreSqlStorage(c => c.UseConnectionString(config.GetConnectionString("DefaultConnection")));
    else
        hf.UseSqlServerStorage(config.GetConnectionString("DefaultConnection"));
});
builder.Services.AddHangfireServer();

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

var app = builder.Build();

app.UseMiddleware<ExceptionMiddleware>();
app.UseCors("Frontend");
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

app.UseHangfireDashboard("/hangfire", new DashboardOptions
{
    Authorization = [new HangfireAuthFilter()]
});

// Apply migrations on startup
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    db.Database.Migrate();
}

app.Run();

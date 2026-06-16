using Memora.Api.Services.Interfaces;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

namespace Memora.Api.Services;

public class EmailService(IConfiguration config, ILogger<EmailService> logger) : IEmailService
{
    private readonly string _apiKey = config["Resend:ApiKey"] ?? throw new InvalidOperationException("Resend:ApiKey not configured");
    private readonly string _fromAddress = config["Resend:FromAddress"] ?? "noreply@flashcards.app";

    public async Task SendPasswordResetAsync(string email, string displayName, string resetLink)
    {
        string subject = "Reset your Flashcards password";
        string body = $"""
            Hi {displayName},<br><br>
            Click the link below to reset your password. This link expires in 1 hour.<br><br>
            <a href="{resetLink}">Reset Password</a><br><br>
            If you didn't request this, you can safely ignore this email.
            """;

        await SendAsync(email, subject, body);
    }

    public async Task SendEmailVerificationAsync(string email, string displayName, string verifyLink)
    {
        string subject = "Verify your Flashcards email";
        string body = $"""
            Hi {displayName},<br><br>
            Click the link below to verify your email address.<br><br>
            <a href="{verifyLink}">Verify Email</a>
            """;

        await SendAsync(email, subject, body);
    }

    public async Task SendReminderAsync(string email, string displayName, string subject, string body)
    {
        await SendAsync(email, subject, body);
    }

    private async Task SendAsync(string to, string subject, string htmlBody)
    {
        using var client = new HttpClient();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _apiKey);

        var payload = new
        {
            from = _fromAddress,
            to = new[] { to },
            subject,
            html = htmlBody
        };

        var content = new StringContent(JsonSerializer.Serialize(payload), Encoding.UTF8, "application/json");
        var response = await client.PostAsync("https://api.resend.com/emails", content);

        if (!response.IsSuccessStatusCode)
        {
            var error = await response.Content.ReadAsStringAsync();
            logger.LogError("Resend API error {StatusCode}: {Error}", response.StatusCode, error);
            throw new Exception($"Email send failed: {response.StatusCode}");
        }
    }
}

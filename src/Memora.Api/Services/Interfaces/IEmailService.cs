namespace Memora.Api.Services.Interfaces;

public interface IEmailService
{
    Task SendPasswordResetAsync(string email, string displayName, string resetLink);
    Task SendEmailVerificationAsync(string email, string displayName, string verifyLink);
    Task SendReminderAsync(string email, string displayName, string subject, string body);
}

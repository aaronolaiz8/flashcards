namespace Retainica.Api.Models;

public class PasswordResetToken
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public string TokenHash { get; set; } = null!;
    public DateTime ExpiresAt { get; set; }
    public DateTime? UsedAt { get; set; }

    public bool IsValid => UsedAt == null && DateTime.UtcNow < ExpiresAt;

    public User User { get; set; } = null!;
}

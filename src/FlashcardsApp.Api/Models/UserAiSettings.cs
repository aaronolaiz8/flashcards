namespace FlashcardsApp.Api.Models;

public enum AiProvider { OpenAI, Anthropic }

public class UserAiSettings
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public AiProvider Provider { get; set; }
    public string EncryptedApiKey { get; set; } = null!;
    public bool IsConfigured { get; set; }
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    public User User { get; set; } = null!;
}

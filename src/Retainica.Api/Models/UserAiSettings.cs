namespace Retainica.Api.Models;

public enum AiProvider { OpenAI, Anthropic }

public class UserAiSettings
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public AiProvider Provider { get; set; }
    public string EncryptedApiKey { get; set; } = null!;

    /// <summary>Provider model id (e.g. "claude-haiku-4-5", "gpt-4o-mini"). Null falls back to the provider default.</summary>
    public string? Model { get; set; }

    public bool IsConfigured { get; set; }
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    public User User { get; set; } = null!;
}

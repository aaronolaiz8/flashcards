namespace Retainica.Api.Models;

public enum AiRequestStatus { Pending, Completed, Failed }

public class AiGenerationRequest
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public int? DeckId { get; set; }
    public string PromptSummary { get; set; } = null!;
    public int CardCount { get; set; }
    public AiRequestStatus Status { get; set; } = AiRequestStatus.Pending;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}

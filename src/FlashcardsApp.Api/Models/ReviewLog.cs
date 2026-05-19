namespace FlashcardsApp.Api.Models;

public class ReviewLog
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public int CardId { get; set; }
    public int SessionId { get; set; }
    public int Rating { get; set; }
    public float? StabilityBefore { get; set; }
    public float? StabilityAfter { get; set; }
    public float? Retrievability { get; set; }
    public int? ResponseTimeMs { get; set; }
    public DateTime ReviewedAt { get; set; } = DateTime.UtcNow;

    public Card Card { get; set; } = null!;
    public StudySession Session { get; set; } = null!;
}

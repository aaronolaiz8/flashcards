namespace Memora.Api.Models;

public enum StudyMode { Spaced, Free, Goal }

public class StudySession
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public int? GoalId { get; set; }
    public int? DeckId { get; set; }
    public StudyMode Mode { get; set; }
    public DateTime StartedAt { get; set; } = DateTime.UtcNow;
    public DateTime? EndedAt { get; set; }
    public int CardsStudied { get; set; }
    public int CorrectCount { get; set; }

    public User User { get; set; } = null!;
    public Goal? Goal { get; set; }
    public ICollection<ReviewLog> ReviewLogs { get; set; } = [];
}

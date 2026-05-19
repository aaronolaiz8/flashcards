namespace FlashcardsApp.Api.Models;

public enum GoalStatus { Active, Completed, Abandoned, Extended }

public class Goal
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public string? Label { get; set; }
    public DateTime DeadlineDate { get; set; }
    public int MasteryThresholdPct { get; set; } = 100;
    public int RecallTargetPct { get; set; } = 90;
    public int DailyNewCardBudget { get; set; }
    public int DailyReviewBudget { get; set; }
    public GoalStatus Status { get; set; } = GoalStatus.Active;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? CompletedAt { get; set; }
    public string? FinalMasterySnapshot { get; set; }

    public User User { get; set; } = null!;
    public ICollection<GoalDeck> GoalDecks { get; set; } = [];
    public ICollection<GoalProgressLog> ProgressLogs { get; set; } = [];
    public ICollection<StudySession> StudySessions { get; set; } = [];
}

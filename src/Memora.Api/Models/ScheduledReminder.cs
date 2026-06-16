namespace Memora.Api.Models;

public enum ReminderType { DailyStudy, BehindPace, DeadlineApproaching, StreakAtRisk, PostReviewNudge }
public enum ReminderStatus { Pending, Sent, Snoozed, Suppressed }

public class ScheduledReminder
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public ReminderType Type { get; set; }
    public int? GoalId { get; set; }
    public DateTime ScheduledFor { get; set; }
    public DateTime? SentAt { get; set; }
    public DateTime? SnoozedUntil { get; set; }
    public ReminderStatus Status { get; set; } = ReminderStatus.Pending;
}

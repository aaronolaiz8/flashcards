namespace Retainica.Api.Models;

public enum NotificationFrequency { Daily, EveryOtherDay, WeeklyDigest }
public enum NotificationChannel { Email, Push, Both }

public class NotificationPreferences
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public bool DailyStudyEnabled { get; set; } = true;
    public bool BehindPaceEnabled { get; set; } = true;
    public bool DeadlineApproachEnabled { get; set; } = true;
    public bool StreakAtRiskEnabled { get; set; } = true;
    public bool PostReviewNudgeEnabled { get; set; } = true;
    public NotificationFrequency FrequencyMode { get; set; } = NotificationFrequency.Daily;
    public NotificationChannel Channel { get; set; } = NotificationChannel.Email;

    public User User { get; set; } = null!;
}

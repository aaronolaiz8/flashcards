namespace Retainica.Api.DTOs.Reminders;

public record NotificationPreferencesDto(bool DailyStudyEnabled, bool BehindPaceEnabled,
    bool DeadlineApproachEnabled, bool StreakAtRiskEnabled, bool PostReviewNudgeEnabled,
    string FrequencyMode, string Channel);

public record UpdatePreferencesRequest(bool? DailyStudyEnabled, bool? BehindPaceEnabled,
    bool? DeadlineApproachEnabled, bool? StreakAtRiskEnabled, bool? PostReviewNudgeEnabled,
    string? FrequencyMode, string? Channel);

public record ScheduledReminderDto(int Id, string Type, int? GoalId,
    DateTime ScheduledFor, string Status);

public record SnoozeRequest(int Hours);

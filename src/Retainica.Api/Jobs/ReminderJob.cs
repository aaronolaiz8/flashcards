using Retainica.Api.Data;
using Retainica.Api.Models;
using Retainica.Api.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Retainica.Api.Jobs;

public class ReminderJob(AppDbContext db, IEmailService emailService, ILogger<ReminderJob> logger)
{
    private static readonly TimeSpan StalenessLimit = TimeSpan.FromHours(2);

    public async Task SendReminderAsync(int reminderId)
    {
        var reminder = await db.ScheduledReminders.FindAsync(reminderId);
        if (reminder == null || reminder.Status != ReminderStatus.Pending)
            return;

        if (DateTime.UtcNow - reminder.ScheduledFor > StalenessLimit)
        {
            reminder.Status = ReminderStatus.Suppressed;
            await db.SaveChangesAsync();
            logger.LogInformation("Reminder {Id} suppressed — too stale", reminderId);
            return;
        }

        var user = await db.Users.FindAsync(reminder.UserId);
        if (user == null) return;

        var prefs = await db.NotificationPreferences.FirstOrDefaultAsync(p => p.UserId == reminder.UserId);
        if (prefs == null) return;

        if (!ShouldSend(prefs, reminder.Type))
        {
            reminder.Status = ReminderStatus.Suppressed;
            await db.SaveChangesAsync();
            return;
        }

        var (subject, body) = BuildMessage(reminder);

        try
        {
            await emailService.SendReminderAsync(user.Email, user.DisplayName, subject, body);
            reminder.Status = ReminderStatus.Sent;
            reminder.SentAt = DateTime.UtcNow;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to send reminder {Id}", reminderId);
            throw;
        }
        finally
        {
            await db.SaveChangesAsync();
        }
    }

    private static bool ShouldSend(NotificationPreferences prefs, ReminderType type) => type switch
    {
        ReminderType.DailyStudy => prefs.DailyStudyEnabled,
        ReminderType.BehindPace => prefs.BehindPaceEnabled,
        ReminderType.DeadlineApproaching => prefs.DeadlineApproachEnabled,
        ReminderType.StreakAtRisk => prefs.StreakAtRiskEnabled,
        ReminderType.PostReviewNudge => prefs.PostReviewNudgeEnabled,
        _ => false
    };

    private static (string subject, string body) BuildMessage(ScheduledReminder reminder) => reminder.Type switch
    {
        ReminderType.DailyStudy => ("Cards are waiting for you", "You have cards due today. Keep your streak alive!"),
        ReminderType.BehindPace => ("You're falling behind on your goal", "You're behind pace on your study goal. A quick session now can catch you up."),
        ReminderType.DeadlineApproaching => ("Your goal deadline is approaching", "Your study goal deadline is coming up soon. Check your progress."),
        ReminderType.StreakAtRisk => ("Don't break your streak!", "You still have cards due today. Just a few minutes keeps your streak alive."),
        ReminderType.PostReviewNudge => ("Cards are due for review", "Some cards are due for review. Best time to review is now."),
        _ => ("Flashcards reminder", "You have a pending reminder.")
    };
}

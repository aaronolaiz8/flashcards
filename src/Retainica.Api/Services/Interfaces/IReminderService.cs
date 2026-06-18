using Retainica.Api.DTOs.Reminders;

namespace Retainica.Api.Services.Interfaces;

public interface IReminderService
{
    Task<NotificationPreferencesDto> GetPreferencesAsync(int userId);
    Task<NotificationPreferencesDto> UpdatePreferencesAsync(int userId, UpdatePreferencesRequest request);
    Task<List<ScheduledReminderDto>> GetScheduledAsync(int userId);
    Task SnoozeAsync(int reminderId, int userId, SnoozeRequest request);
}

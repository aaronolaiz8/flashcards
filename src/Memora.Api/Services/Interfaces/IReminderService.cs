using Memora.Api.DTOs.Reminders;

namespace Memora.Api.Services.Interfaces;

public interface IReminderService
{
    Task<NotificationPreferencesDto> GetPreferencesAsync(int userId);
    Task<NotificationPreferencesDto> UpdatePreferencesAsync(int userId, UpdatePreferencesRequest request);
    Task<List<ScheduledReminderDto>> GetScheduledAsync(int userId);
    Task SnoozeAsync(int reminderId, int userId, SnoozeRequest request);
}

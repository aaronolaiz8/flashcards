using Retainica.Api.Data;
using Retainica.Api.DTOs.Reminders;
using Retainica.Api.Services.Interfaces;

namespace Retainica.Api.Services;

public class ReminderService(AppDbContext db) : IReminderService
{
    public Task<NotificationPreferencesDto> GetPreferencesAsync(int userId) => throw new NotImplementedException();
    public Task<NotificationPreferencesDto> UpdatePreferencesAsync(int userId, UpdatePreferencesRequest request) => throw new NotImplementedException();
    public Task<List<ScheduledReminderDto>> GetScheduledAsync(int userId) => throw new NotImplementedException();
    public Task SnoozeAsync(int reminderId, int userId, SnoozeRequest request) => throw new NotImplementedException();
}

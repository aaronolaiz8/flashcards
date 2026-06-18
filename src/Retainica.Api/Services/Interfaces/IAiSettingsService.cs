using Retainica.Api.DTOs.Settings;

namespace Retainica.Api.Services.Interfaces;

public interface IAiSettingsService
{
    Task<AiSettingsDto> GetAsync(int userId);
    Task<AiSettingsDto> SaveAsync(int userId, SaveAiSettingsRequest request);
    Task DeleteAsync(int userId);
}

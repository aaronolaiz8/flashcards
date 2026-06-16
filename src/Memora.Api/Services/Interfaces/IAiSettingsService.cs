using Memora.Api.DTOs.Settings;

namespace Memora.Api.Services.Interfaces;

public interface IAiSettingsService
{
    Task<AiSettingsDto> GetAsync(int userId);
    Task<AiSettingsDto> SaveAsync(int userId, SaveAiSettingsRequest request);
    Task DeleteAsync(int userId);
}

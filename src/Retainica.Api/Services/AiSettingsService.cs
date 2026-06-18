using Retainica.Api.Data;
using Retainica.Api.DTOs.Settings;
using Retainica.Api.Services.Interfaces;

namespace Retainica.Api.Services;

public class AiSettingsService(AppDbContext db) : IAiSettingsService
{
    public Task<AiSettingsDto> GetAsync(int userId) => throw new NotImplementedException();

    public Task<AiSettingsDto> SaveAsync(int userId, SaveAiSettingsRequest request) => throw new NotImplementedException();

    public Task DeleteAsync(int userId) => throw new NotImplementedException();
}

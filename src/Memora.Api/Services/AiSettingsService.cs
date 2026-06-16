using Memora.Api.Data;
using Memora.Api.DTOs.Settings;
using Memora.Api.Services.Interfaces;

namespace Memora.Api.Services;

public class AiSettingsService(AppDbContext db) : IAiSettingsService
{
    public Task<AiSettingsDto> GetAsync(int userId) => throw new NotImplementedException();

    public Task<AiSettingsDto> SaveAsync(int userId, SaveAiSettingsRequest request) => throw new NotImplementedException();

    public Task DeleteAsync(int userId) => throw new NotImplementedException();
}

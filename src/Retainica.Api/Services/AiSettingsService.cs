using Retainica.Api.Data;
using Retainica.Api.DTOs.Settings;
using Retainica.Api.Infrastructure;
using Retainica.Api.Models;
using Retainica.Api.Services.Ai;
using Retainica.Api.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Retainica.Api.Services;

public class AiSettingsService(
    AppDbContext db,
    IEncryptionService encryption,
    IAiProviderFactory providers) : IAiSettingsService
{
    public async Task<AiSettingsDto> GetAsync(int userId)
    {
        var settings = await db.UserAiSettings.FirstOrDefaultAsync(s => s.UserId == userId);
        return settings is null
            ? new AiSettingsDto(null, null, false)
            : new AiSettingsDto(settings.Provider.ToString(), settings.Model, settings.IsConfigured);
    }

    public async Task<AiSettingsDto> SaveAsync(int userId, SaveAiSettingsRequest request)
    {
        if (!Enum.TryParse<AiProvider>(request.Provider, ignoreCase: true, out var provider))
            throw new ArgumentException($"Unknown AI provider: {request.Provider}");
        if (string.IsNullOrWhiteSpace(request.ApiKey))
            throw new ArgumentException("API key is required");

        var apiKey = request.ApiKey.Trim();
        var impl = providers.Get(provider);
        var model = string.IsNullOrWhiteSpace(request.Model) ? impl.DefaultModel : request.Model.Trim();

        // Confirm the key + model actually work before persisting.
        await impl.ValidateAsync(apiKey, model, CancellationToken.None);

        var settings = await db.UserAiSettings.FirstOrDefaultAsync(s => s.UserId == userId);
        if (settings is null)
        {
            settings = new UserAiSettings { UserId = userId };
            db.UserAiSettings.Add(settings);
        }

        settings.Provider = provider;
        settings.Model = model;
        settings.EncryptedApiKey = encryption.Encrypt(apiKey);
        settings.IsConfigured = true;
        settings.UpdatedAt = DateTime.UtcNow;

        await db.SaveChangesAsync();
        return new AiSettingsDto(settings.Provider.ToString(), settings.Model, settings.IsConfigured);
    }

    public async Task DeleteAsync(int userId)
    {
        var settings = await db.UserAiSettings.FirstOrDefaultAsync(s => s.UserId == userId);
        if (settings is null) return;

        db.UserAiSettings.Remove(settings);
        await db.SaveChangesAsync();
    }
}

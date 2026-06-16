namespace Memora.Api.DTOs.Settings;

public record AiSettingsDto(string? Provider, bool IsConfigured);
public record SaveAiSettingsRequest(string Provider, string ApiKey);

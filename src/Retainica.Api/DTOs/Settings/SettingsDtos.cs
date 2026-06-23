namespace Retainica.Api.DTOs.Settings;

public record AiSettingsDto(string? Provider, string? Model, bool IsConfigured);
public record SaveAiSettingsRequest(string Provider, string ApiKey, string? Model);

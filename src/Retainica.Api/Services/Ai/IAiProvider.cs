using Retainica.Api.DTOs.Ai;
using Retainica.Api.Models;

namespace Retainica.Api.Services.Ai;

/// <summary>One LLM provider behind the card-generation feature (OpenAI or Anthropic).</summary>
public interface IAiProvider
{
    AiProvider Provider { get; }

    /// <summary>Model id used when the user hasn't chosen one.</summary>
    string DefaultModel { get; }

    /// <summary>Generates cards from a fully-built user prompt. Returns raw (unsanitized) candidates.</summary>
    Task<IReadOnlyList<GeneratedCardDto>> GenerateAsync(
        string userPrompt, int count, string model, string apiKey, CancellationToken ct);

    /// <summary>Cheap call to confirm the key + model work. Throws ArgumentException on rejection.</summary>
    Task ValidateAsync(string apiKey, string model, CancellationToken ct);
}

public interface IAiProviderFactory
{
    IAiProvider Get(AiProvider provider);
}

public sealed class AiProviderFactory(IEnumerable<IAiProvider> providers) : IAiProviderFactory
{
    public IAiProvider Get(AiProvider provider) =>
        providers.FirstOrDefault(p => p.Provider == provider)
            ?? throw new ArgumentException($"Unsupported AI provider: {provider}");
}

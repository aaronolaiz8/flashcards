using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Retainica.Api.DTOs.Ai;
using Retainica.Api.Models;

namespace Retainica.Api.Services.Ai;

/// <summary>Anthropic Messages API (POST /v1/messages) via raw HTTP.</summary>
public sealed class AnthropicProvider(IHttpClientFactory httpFactory) : IAiProvider
{
    private const string Endpoint = "https://api.anthropic.com/v1/messages";
    private const string ApiVersion = "2023-06-01";

    public AiProvider Provider => AiProvider.Anthropic;
    public string DefaultModel => "claude-haiku-4-5";

    public async Task<IReadOnlyList<GeneratedCardDto>> GenerateAsync(
        string userPrompt, int count, string model, string apiKey, CancellationToken ct)
    {
        var text = await SendAsync(model, apiKey, userPrompt, MaxTokensFor(count), ct);
        return AiPrompt.Parse(text);
    }

    public async Task ValidateAsync(string apiKey, string model, CancellationToken ct) =>
        await SendAsync(model, apiKey, "Reply with the single word OK.", 8, ct);

    private async Task<string> SendAsync(string model, string apiKey, string userContent, int maxTokens, CancellationToken ct)
    {
        var body = new
        {
            model,
            max_tokens = maxTokens,
            system = AiPrompt.System,
            messages = new[] { new { role = "user", content = userContent } }
        };

        using var req = new HttpRequestMessage(HttpMethod.Post, Endpoint);
        req.Headers.Add("x-api-key", apiKey);
        req.Headers.Add("anthropic-version", ApiVersion);
        req.Content = JsonContent.Create(body);

        var client = httpFactory.CreateClient("ai");
        using var res = await client.SendAsync(req, ct);
        var payload = await res.Content.ReadAsStringAsync(ct);

        if (!res.IsSuccessStatusCode)
            throw AiHttpErrors.Map("Anthropic", res.StatusCode, model, payload);

        return ExtractText(payload);
    }

    private static string ExtractText(string payload)
    {
        // { "content": [ { "type": "text", "text": "..." }, ... ] }
        using var doc = JsonDocument.Parse(payload);
        if (!doc.RootElement.TryGetProperty("content", out var content) || content.ValueKind != JsonValueKind.Array)
            return "";

        var parts = content.EnumerateArray()
            .Where(b => b.TryGetProperty("type", out var t) && t.GetString() == "text")
            .Select(b => b.TryGetProperty("text", out var txt) ? txt.GetString() : null)
            .Where(s => s is not null);
        return string.Concat(parts);
    }

    private static int MaxTokensFor(int count) => Math.Clamp(count * 140 + 400, 1024, 8000);
}

internal static class AiHttpErrors
{
    /// <summary>Maps a provider HTTP failure to an ArgumentException (surfaces as 400 with a clear message).</summary>
    public static Exception Map(string provider, HttpStatusCode status, string model, string payload) => status switch
    {
        HttpStatusCode.Unauthorized or HttpStatusCode.Forbidden =>
            new ArgumentException($"Your {provider} API key was rejected. Check the key in Settings → AI."),
        HttpStatusCode.NotFound =>
            new ArgumentException($"{provider} did not recognize the model \"{model}\". Pick a different model."),
        HttpStatusCode.TooManyRequests =>
            new ArgumentException($"{provider} rate limit reached. Wait a moment and try again."),
        _ => new ArgumentException($"{provider} request failed ({(int)status}). {Summarize(payload)}".Trim())
    };

    private static string Summarize(string payload)
    {
        if (string.IsNullOrWhiteSpace(payload)) return "";
        try
        {
            using var doc = JsonDocument.Parse(payload);
            if (doc.RootElement.TryGetProperty("error", out var err))
            {
                if (err.ValueKind == JsonValueKind.Object && err.TryGetProperty("message", out var msg))
                    return msg.GetString() ?? "";
                if (err.ValueKind == JsonValueKind.String)
                    return err.GetString() ?? "";
            }
        }
        catch (JsonException) { /* fall through */ }
        return "";
    }
}

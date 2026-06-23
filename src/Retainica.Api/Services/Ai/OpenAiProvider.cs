using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using Retainica.Api.DTOs.Ai;
using Retainica.Api.Models;

namespace Retainica.Api.Services.Ai;

/// <summary>OpenAI Chat Completions API (POST /v1/chat/completions) via raw HTTP.</summary>
public sealed class OpenAiProvider(IHttpClientFactory httpFactory) : IAiProvider
{
    private const string Endpoint = "https://api.openai.com/v1/chat/completions";

    public AiProvider Provider => AiProvider.OpenAI;
    public string DefaultModel => "gpt-4o-mini";

    public async Task<IReadOnlyList<GeneratedCardDto>> GenerateAsync(
        string userPrompt, int count, string model, string apiKey, CancellationToken ct)
    {
        var text = await SendAsync(model, apiKey, userPrompt, MaxTokensFor(count), jsonMode: true, ct);
        return AiPrompt.Parse(text);
    }

    public async Task ValidateAsync(string apiKey, string model, CancellationToken ct) =>
        await SendAsync(model, apiKey, "Reply with the single word OK.", 8, jsonMode: false, ct);

    private async Task<string> SendAsync(
        string model, string apiKey, string userContent, int maxTokens, bool jsonMode, CancellationToken ct)
    {
        object body = jsonMode
            ? new
            {
                model,
                max_tokens = maxTokens,
                response_format = new { type = "json_object" },
                messages = new[]
                {
                    new { role = "system", content = AiPrompt.System },
                    new { role = "user", content = userContent }
                }
            }
            : new
            {
                model,
                max_tokens = maxTokens,
                messages = new[]
                {
                    new { role = "system", content = AiPrompt.System },
                    new { role = "user", content = userContent }
                }
            };

        using var req = new HttpRequestMessage(HttpMethod.Post, Endpoint);
        req.Headers.Authorization = new AuthenticationHeaderValue("Bearer", apiKey);
        req.Content = JsonContent.Create(body);

        var client = httpFactory.CreateClient("ai");
        using var res = await client.SendAsync(req, ct);
        var payload = await res.Content.ReadAsStringAsync(ct);

        if (!res.IsSuccessStatusCode)
            throw AiHttpErrors.Map("OpenAI", res.StatusCode, model, payload);

        return ExtractText(payload);
    }

    private static string ExtractText(string payload)
    {
        // { "choices": [ { "message": { "content": "..." } } ] }
        using var doc = JsonDocument.Parse(payload);
        if (doc.RootElement.TryGetProperty("choices", out var choices)
            && choices.ValueKind == JsonValueKind.Array
            && choices.GetArrayLength() > 0
            && choices[0].TryGetProperty("message", out var msg)
            && msg.TryGetProperty("content", out var content))
            return content.GetString() ?? "";
        return "";
    }

    private static int MaxTokensFor(int count) => Math.Clamp(count * 140 + 400, 1024, 8000);
}

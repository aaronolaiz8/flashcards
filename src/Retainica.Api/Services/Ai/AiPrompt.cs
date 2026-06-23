using System.Text.Json;
using Retainica.Api.DTOs.Ai;

namespace Retainica.Api.Services.Ai;

/// <summary>Shared prompt contract and tolerant response parsing for all providers.</summary>
public static class AiPrompt
{
    public const string System =
        "You are a flashcard generation assistant. You create clear, accurate study flashcards. " +
        "Respond with ONLY a JSON object of the form {\"cards\":[{\"front\":\"...\",\"back\":\"...\"}]}. " +
        "Each card has a concise question or prompt on the front and the answer on the back. " +
        "No markdown, no code fences, no commentary — only the JSON object.";

    public static string BuildUserPrompt(int count, string source) =>
        $"Generate {count} flashcards as JSON about the following topic or source material. " +
        $"Return exactly the JSON object described, with a \"cards\" array of {count} items.\n\n{source}";

    /// <summary>
    /// Parses a model response into cards. Accepts a bare JSON array, an object with a "cards"
    /// (or first array-valued) property, and tolerates ```json fences / surrounding prose by
    /// extracting the first balanced JSON value. Reads front/back case-insensitively.
    /// </summary>
    public static IReadOnlyList<GeneratedCardDto> Parse(string? text)
    {
        if (string.IsNullOrWhiteSpace(text)) return [];

        var json = ExtractJson(text);
        if (json is null) return [];

        try
        {
            using var doc = JsonDocument.Parse(json);
            var array = doc.RootElement.ValueKind switch
            {
                JsonValueKind.Array => doc.RootElement,
                JsonValueKind.Object => FindCardArray(doc.RootElement),
                _ => (JsonElement?)null
            };
            if (array is null) return [];

            var cards = new List<GeneratedCardDto>();
            foreach (var item in array.Value.EnumerateArray())
            {
                if (item.ValueKind != JsonValueKind.Object) continue;
                var front = ReadProp(item, "front");
                var back = ReadProp(item, "back");
                if (!string.IsNullOrWhiteSpace(front) && !string.IsNullOrWhiteSpace(back))
                    cards.Add(new GeneratedCardDto(front!.Trim(), back!.Trim()));
            }
            return cards;
        }
        catch (JsonException)
        {
            return [];
        }
    }

    private static JsonElement? FindCardArray(JsonElement obj)
    {
        if (obj.TryGetProperty("cards", out var cards) && cards.ValueKind == JsonValueKind.Array)
            return cards;
        foreach (var prop in obj.EnumerateObject())
            if (prop.Value.ValueKind == JsonValueKind.Array)
                return prop.Value;
        return null;
    }

    private static string? ReadProp(JsonElement obj, string name)
    {
        foreach (var prop in obj.EnumerateObject())
            if (string.Equals(prop.Name, name, StringComparison.OrdinalIgnoreCase))
                return prop.Value.ValueKind == JsonValueKind.String
                    ? prop.Value.GetString()
                    : prop.Value.ToString();
        return null;
    }

    /// <summary>Returns the first balanced { } or [ ] JSON value in the text, or null.</summary>
    private static string? ExtractJson(string text)
    {
        var trimmed = text.Trim();
        var start = trimmed.IndexOfAny(['{', '[']);
        if (start < 0) return null;

        var open = trimmed[start];
        var close = open == '{' ? '}' : ']';
        var depth = 0;
        var inString = false;
        var escape = false;

        for (var i = start; i < trimmed.Length; i++)
        {
            var c = trimmed[i];
            if (inString)
            {
                if (escape) escape = false;
                else if (c == '\\') escape = true;
                else if (c == '"') inString = false;
                continue;
            }

            if (c == '"') inString = true;
            else if (c == open) depth++;
            else if (c == close && --depth == 0)
                return trimmed.Substring(start, i - start + 1);
        }
        return null;
    }
}

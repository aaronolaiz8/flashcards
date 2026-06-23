using Retainica.Api.Data;
using Retainica.Api.DTOs.Ai;
using Retainica.Api.Infrastructure;
using Retainica.Api.Models;
using Retainica.Api.Services.Ai;
using Retainica.Api.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Retainica.Api.Services;

public class AiService(
    AppDbContext db,
    IEncryptionService encryption,
    IAiProviderFactory providers) : IAiService
{
    private const int MinCount = 5;
    private const int MaxCount = 50;
    private const int MaxSourceChars = 8000;

    public async Task<List<GeneratedCardDto>> GenerateCardsAsync(int userId, GenerateCardsRequest request)
    {
        var settings = await db.UserAiSettings.FirstOrDefaultAsync(s => s.UserId == userId);
        if (settings is null || !settings.IsConfigured)
            throw new ArgumentException("AI is not set up. Add an API key under Settings → AI first.");

        var source = (request.Text ?? request.Topic ?? "").Trim();
        if (string.IsNullOrWhiteSpace(source))
            throw new ArgumentException("Provide a topic or some text to generate cards from.");
        if (source.Length > MaxSourceChars)
            throw new ArgumentException($"Source text is too long (max {MaxSourceChars} characters).");

        var count = Math.Clamp(request.Count, MinCount, MaxCount);
        var apiKey = encryption.Decrypt(settings.EncryptedApiKey);
        var provider = providers.Get(settings.Provider);
        var model = string.IsNullOrWhiteSpace(settings.Model) ? provider.DefaultModel : settings.Model!;

        var log = new AiGenerationRequest
        {
            UserId = userId,
            DeckId = request.DeckId,
            PromptSummary = source.Length > 200 ? source[..200] : source,
            CardCount = count,
            Status = AiRequestStatus.Pending
        };
        db.AiGenerationRequests.Add(log);

        try
        {
            var prompt = AiPrompt.BuildUserPrompt(count, source);
            var raw = await provider.GenerateAsync(prompt, count, model, apiKey, CancellationToken.None);

            var cards = raw
                .Select(c => new GeneratedCardDto(c.Front.Trim(), c.Back.Trim()))
                .Where(c => c.Front.Length > 0 && c.Back.Length > 0)
                .Take(count)
                .ToList();

            if (cards.Count == 0)
                throw new ArgumentException("The model didn't return any usable cards. Try rephrasing your topic.");

            log.Status = AiRequestStatus.Completed;
            log.CardCount = cards.Count;
            await db.SaveChangesAsync();
            return cards;
        }
        catch
        {
            log.Status = AiRequestStatus.Failed;
            await db.SaveChangesAsync();
            throw;
        }
    }
}

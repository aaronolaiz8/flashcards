using System.Text;
using System.Text.Json;
using Retainica.Api.Data;
using Retainica.Api.DTOs.Decks;
using Retainica.Api.Models;
using Retainica.Api.Services.Interfaces;
using Retainica.Api.Utilities;
using Microsoft.EntityFrameworkCore;

namespace Retainica.Api.Services;

public class DeckService(AppDbContext db) : IDeckService
{
    public async Task<List<DeckSummaryDto>> GetUserDecksAsync(int userId)
    {
        return await db.Decks
            .Where(d => d.UserId == userId)
            .OrderByDescending(d => d.UpdatedAt)
            .Select(d => new DeckSummaryDto(d.Id, d.Title, d.Description, d.Tags,
                d.Visibility.ToString(), d.Cards.Count, d.CreatedAt, d.UpdatedAt))
            .ToListAsync();
    }

    public async Task<DeckDetailDto> GetDeckAsync(int deckId, int userId)
    {
        var deck = await db.Decks
            .Include(d => d.Cards)
            .FirstOrDefaultAsync(d => d.Id == deckId)
            ?? throw new KeyNotFoundException("Deck not found");

        if (deck.UserId != userId && deck.Visibility == DeckVisibility.Private)
            throw new UnauthorizedAccessException("You do not have access to this deck");

        return ToDetail(deck, deck.Cards.Count);
    }

    public async Task<DeckDetailDto> CreateDeckAsync(int userId, CreateDeckRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Title))
            throw new ArgumentException("Title is required");

        var deck = new Deck
        {
            UserId = userId,
            Title = request.Title.Trim(),
            Description = request.Description,
            Tags = request.Tags ?? [],
            Visibility = ParseVisibility(request.Visibility)
        };

        db.Decks.Add(deck);
        await db.SaveChangesAsync();
        return ToDetail(deck, 0);
    }

    public async Task<DeckDetailDto> UpdateDeckAsync(int deckId, int userId, UpdateDeckRequest request)
    {
        var deck = await GetOwnedDeckAsync(deckId, userId);

        if (request.Title is not null)
        {
            if (string.IsNullOrWhiteSpace(request.Title))
                throw new ArgumentException("Title cannot be empty");
            deck.Title = request.Title.Trim();
        }
        if (request.Description is not null) deck.Description = request.Description;
        if (request.Tags is not null) deck.Tags = request.Tags;
        if (request.Visibility is not null) deck.Visibility = ParseVisibility(request.Visibility);

        deck.UpdatedAt = DateTime.UtcNow;
        await db.SaveChangesAsync();

        var cardCount = await db.Cards.CountAsync(c => c.DeckId == deck.Id);
        return ToDetail(deck, cardCount);
    }

    public async Task DeleteDeckAsync(int deckId, int userId)
    {
        var deck = await GetOwnedDeckAsync(deckId, userId);
        db.Decks.Remove(deck);
        await db.SaveChangesAsync();
    }

    public async Task<DeckDetailDto> ForkDeckAsync(int deckId, int userId)
    {
        var source = await db.Decks
            .Include(d => d.Cards)
            .FirstOrDefaultAsync(d => d.Id == deckId)
            ?? throw new KeyNotFoundException("Deck not found");

        if (source.UserId != userId && source.Visibility == DeckVisibility.Private)
            throw new UnauthorizedAccessException("You cannot fork this deck");

        var fork = new Deck
        {
            UserId = userId,
            Title = source.Title,
            Description = source.Description,
            Tags = source.Tags,
            Visibility = DeckVisibility.Private,
            ForkedFromDeckId = source.Id,
            Cards = source.Cards.Select(c => new Card
            {
                Front = c.Front,
                Back = c.Back,
                Tags = c.Tags
            }).ToList()
        };

        db.Decks.Add(fork);
        await db.SaveChangesAsync();
        return ToDetail(fork, fork.Cards.Count);
    }

    public async Task<PagedResult<DeckSummaryDto>> SearchPublicDecksAsync(string? query, string[]? tags, int page, int pageSize)
    {
        if (page < 1) page = 1;
        if (pageSize is < 1 or > 100) pageSize = 20;

        var q = db.Decks.Where(d => d.Visibility == DeckVisibility.Public);

        if (!string.IsNullOrWhiteSpace(query))
        {
            var term = query.Trim();
            q = q.Where(d => EF.Functions.Like(d.Title, $"%{term}%")
                || (d.Description != null && EF.Functions.Like(d.Description, $"%{term}%")));
        }

        if (tags is { Length: > 0 })
            q = q.Where(d => tags.All(t => d.Tags.Contains(t)));

        var total = await q.CountAsync();
        var items = await q
            .OrderByDescending(d => d.UpdatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(d => new DeckSummaryDto(d.Id, d.Title, d.Description, d.Tags,
                d.Visibility.ToString(), d.Cards.Count, d.CreatedAt, d.UpdatedAt))
            .ToListAsync();

        return new PagedResult<DeckSummaryDto>(items, total, page, pageSize);
    }

    public async Task<byte[]> ExportDeckAsync(int deckId, int userId, string format)
    {
        var deck = await db.Decks
            .Include(d => d.Cards)
            .FirstOrDefaultAsync(d => d.Id == deckId)
            ?? throw new KeyNotFoundException("Deck not found");

        if (deck.UserId != userId && deck.Visibility == DeckVisibility.Private)
            throw new UnauthorizedAccessException("You do not have access to this deck");

        return format.ToLowerInvariant() switch
        {
            "csv" => Encoding.UTF8.GetBytes(ExportCsv(deck)),
            "json" => Encoding.UTF8.GetBytes(ExportJson(deck)),
            _ => throw new ArgumentException($"Unsupported export format: {format}")
        };
    }

    public async Task<DeckDetailDto> ImportDeckAsync(int userId, ImportDeckRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Content))
            throw new ArgumentException("Import content is empty");

        var deck = request.Format.ToLowerInvariant() switch
        {
            "csv" => ImportCsv(userId, request.Content),
            "json" => ImportJson(userId, request.Content),
            _ => throw new ArgumentException($"Unsupported import format: {request.Format}")
        };

        // Caller-supplied fields win over the CSV defaults / the JSON file's own values.
        if (!string.IsNullOrWhiteSpace(request.Title))
            deck.Title = request.Title.Trim();
        if (!string.IsNullOrWhiteSpace(request.Description))
            deck.Description = request.Description.Trim();
        if (request.Tags is { Length: > 0 })
            deck.Tags = request.Tags;

        db.Decks.Add(deck);
        await db.SaveChangesAsync();
        return ToDetail(deck, deck.Cards.Count);
    }

    // --- helpers ---

    private async Task<Deck> GetOwnedDeckAsync(int deckId, int userId)
    {
        var deck = await db.Decks.FirstOrDefaultAsync(d => d.Id == deckId)
            ?? throw new KeyNotFoundException("Deck not found");
        if (deck.UserId != userId)
            throw new UnauthorizedAccessException("You do not own this deck");
        return deck;
    }

    private static DeckDetailDto ToDetail(Deck d, int cardCount) =>
        new(d.Id, d.Title, d.Description, d.Tags, d.Visibility.ToString(),
            d.ForkedFromDeckId, cardCount, d.CreatedAt, d.UpdatedAt);

    private static DeckVisibility ParseVisibility(string? value)
    {
        if (string.IsNullOrWhiteSpace(value)) return DeckVisibility.Private;
        return Enum.TryParse<DeckVisibility>(value, ignoreCase: true, out var v)
            ? v
            : throw new ArgumentException($"Invalid visibility: {value}");
    }

    private static string ExportCsv(Deck deck)
    {
        var sb = new StringBuilder();
        sb.AppendLine("Front,Back,Tags");
        foreach (var c in deck.Cards)
        {
            var tags = string.Join(";", c.Tags);
            sb.AppendLine($"{Csv.Escape(c.Front)},{Csv.Escape(c.Back)},{Csv.Escape(tags)}");
        }
        return sb.ToString();
    }

    private static string ExportJson(Deck deck)
    {
        var payload = new
        {
            deck.Title,
            deck.Description,
            deck.Tags,
            Cards = deck.Cards.Select(c => new { c.Front, c.Back, c.Tags })
        };
        return JsonSerializer.Serialize(payload, new JsonSerializerOptions { WriteIndented = true });
    }

    private static Deck ImportCsv(int userId, string content)
    {
        var rows = Csv.Parse(content);
        if (rows.Count == 0) throw new ArgumentException("CSV has no rows");

        // Skip header if it looks like one.
        var start = rows[0].Count >= 2
            && rows[0][0].Trim().Equals("Front", StringComparison.OrdinalIgnoreCase)
            ? 1 : 0;

        var cards = new List<Card>();
        for (var i = start; i < rows.Count; i++)
        {
            var row = rows[i];
            if (row.Count < 2 || (string.IsNullOrWhiteSpace(row[0]) && string.IsNullOrWhiteSpace(row[1])))
                continue;
            cards.Add(new Card
            {
                Front = row[0],
                Back = row[1],
                Tags = row.Count > 2 && !string.IsNullOrWhiteSpace(row[2])
                    ? row[2].Split(';', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                    : []
            });
        }

        if (cards.Count == 0) throw new ArgumentException("CSV contained no usable cards");

        return new Deck
        {
            UserId = userId,
            Title = "Imported Deck",
            Visibility = DeckVisibility.Private,
            Cards = cards
        };
    }

    private static Deck ImportJson(int userId, string content)
    {
        ImportedDeck? parsed;
        try
        {
            parsed = JsonSerializer.Deserialize<ImportedDeck>(content,
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
        }
        catch (JsonException ex)
        {
            throw new ArgumentException($"Invalid JSON: {ex.Message}");
        }

        if (parsed is null || string.IsNullOrWhiteSpace(parsed.Title))
            throw new ArgumentException("JSON must include a deck title");

        return new Deck
        {
            UserId = userId,
            Title = parsed.Title.Trim(),
            Description = parsed.Description,
            Tags = parsed.Tags ?? [],
            Visibility = DeckVisibility.Private,
            Cards = (parsed.Cards ?? [])
                .Where(c => !string.IsNullOrWhiteSpace(c.Front))
                .Select(c => new Card { Front = c.Front, Back = c.Back ?? "", Tags = c.Tags ?? [] })
                .ToList()
        };
    }

    private record ImportedDeck(string Title, string? Description, string[]? Tags, List<ImportedCard>? Cards);
    private record ImportedCard(string Front, string? Back, string[]? Tags);
}

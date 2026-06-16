using Memora.Api.Data;
using Memora.Api.DTOs.Cards;
using Memora.Api.Models;
using Memora.Api.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Memora.Api.Services;

public class CardService(AppDbContext db) : ICardService
{
    public async Task<List<CardDto>> GetCardsAsync(int deckId, int userId)
    {
        await EnsureDeckOwnedAsync(deckId, userId);
        return await db.Cards
            .Where(c => c.DeckId == deckId)
            .OrderBy(c => c.CreatedAt)
            .Select(c => new CardDto(c.Id, c.DeckId, c.Front, c.Back, c.Tags, c.CreatedAt, c.UpdatedAt))
            .ToListAsync();
    }

    public async Task<CardDto> CreateCardAsync(int deckId, int userId, CreateCardRequest request)
    {
        await EnsureDeckOwnedAsync(deckId, userId);
        Validate(request.Front, request.Back);

        var card = new Card
        {
            DeckId = deckId,
            Front = request.Front.Trim(),
            Back = request.Back.Trim(),
            Tags = request.Tags ?? []
        };

        db.Cards.Add(card);
        await TouchDeckAsync(deckId);
        await db.SaveChangesAsync();
        return ToDto(card);
    }

    public async Task<CardDto> UpdateCardAsync(int deckId, int cardId, int userId, UpdateCardRequest request)
    {
        await EnsureDeckOwnedAsync(deckId, userId);

        var card = await db.Cards.FirstOrDefaultAsync(c => c.Id == cardId && c.DeckId == deckId)
            ?? throw new KeyNotFoundException("Card not found");

        if (request.Front is not null)
        {
            if (string.IsNullOrWhiteSpace(request.Front))
                throw new ArgumentException("Front cannot be empty");
            card.Front = request.Front.Trim();
        }
        if (request.Back is not null)
        {
            if (string.IsNullOrWhiteSpace(request.Back))
                throw new ArgumentException("Back cannot be empty");
            card.Back = request.Back.Trim();
        }
        if (request.Tags is not null) card.Tags = request.Tags;

        card.UpdatedAt = DateTime.UtcNow;
        await TouchDeckAsync(deckId);
        await db.SaveChangesAsync();
        return ToDto(card);
    }

    public async Task DeleteCardAsync(int deckId, int cardId, int userId)
    {
        await EnsureDeckOwnedAsync(deckId, userId);

        var card = await db.Cards.FirstOrDefaultAsync(c => c.Id == cardId && c.DeckId == deckId)
            ?? throw new KeyNotFoundException("Card not found");

        db.Cards.Remove(card);
        await TouchDeckAsync(deckId);
        await db.SaveChangesAsync();
    }

    public async Task<List<CardDto>> BulkCreateCardsAsync(int deckId, int userId, List<CreateCardRequest> cards)
    {
        await EnsureDeckOwnedAsync(deckId, userId);

        if (cards is null || cards.Count == 0)
            throw new ArgumentException("No cards provided");

        var entities = new List<Card>(cards.Count);
        foreach (var c in cards)
        {
            Validate(c.Front, c.Back);
            entities.Add(new Card
            {
                DeckId = deckId,
                Front = c.Front.Trim(),
                Back = c.Back.Trim(),
                Tags = c.Tags ?? []
            });
        }

        db.Cards.AddRange(entities);
        await TouchDeckAsync(deckId);
        await db.SaveChangesAsync();
        return entities.Select(ToDto).ToList();
    }

    // --- helpers ---

    private async Task EnsureDeckOwnedAsync(int deckId, int userId)
    {
        var ownerId = await db.Decks
            .Where(d => d.Id == deckId)
            .Select(d => (int?)d.UserId)
            .FirstOrDefaultAsync()
            ?? throw new KeyNotFoundException("Deck not found");

        if (ownerId != userId)
            throw new UnauthorizedAccessException("You do not own this deck");
    }

    private async Task TouchDeckAsync(int deckId)
    {
        var deck = await db.Decks.FindAsync(deckId);
        if (deck is not null) deck.UpdatedAt = DateTime.UtcNow;
    }

    private static void Validate(string? front, string? back)
    {
        if (string.IsNullOrWhiteSpace(front))
            throw new ArgumentException("Card front is required");
        if (string.IsNullOrWhiteSpace(back))
            throw new ArgumentException("Card back is required");
    }

    private static CardDto ToDto(Card c) =>
        new(c.Id, c.DeckId, c.Front, c.Back, c.Tags, c.CreatedAt, c.UpdatedAt);
}

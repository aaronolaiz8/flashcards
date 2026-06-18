using Retainica.Api.Data;
using Retainica.Api.DTOs.Study;
using Retainica.Api.Models;
using Retainica.Api.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Retainica.Api.Services;

public class StudyService(AppDbContext db, IFsrsService fsrs) : IStudyService
{
    public async Task<SessionDto> StartSessionAsync(int userId, StartSessionRequest request)
    {
        var deckId = request.DeckId;
        var deck = await db.Decks.FirstOrDefaultAsync(d => d.Id == deckId)
            ?? throw new KeyNotFoundException("Deck not found");
        if (deck.UserId != userId)
            throw new UnauthorizedAccessException("You do not own this deck");

        var mode = ParseMode(request.Mode);
        var now = DateTime.UtcNow;

        // Auto-close any abandoned (still-open) sessions for this user before starting a new one.
        var open = await db.StudySessions
            .Where(s => s.UserId == userId && s.EndedAt == null)
            .ToListAsync();
        foreach (var s in open) s.EndedAt = now;

        var cards = await db.Cards.Where(c => c.DeckId == deckId).ToListAsync();
        var cardById = cards.ToDictionary(c => c.Id);

        List<SessionCardDto> sessionCards;

        if (mode == StudyMode.Free)
        {
            // Free Study: no scheduling — just present the cards (optionally shuffled/capped).
            var ordered = ApplyShuffleAndCap(cards.Select(c => c.Id).ToList(), request);
            sessionCards = ordered
                .Select(id => cardById[id])
                .Select(c => new SessionCardDto(c.Id, c.Front, c.Back, "Free", null))
                .ToList();
        }
        else
        {
            var cardIds = cards.Select(c => c.Id).ToList();
            var existing = await db.CardMemoryStates
                .Where(s => s.UserId == userId && cardIds.Contains(s.CardId))
                .ToDictionaryAsync(s => s.CardId);

            // Cards without a memory state yet are treated as New (transient — persisted at review time).
            var working = cards
                .Select(c => existing.TryGetValue(c.Id, out var st)
                    ? st
                    : new CardMemoryState { UserId = userId, CardId = c.Id, State = CardState.New })
                .ToList();

            var due = fsrs.GetDueCards(working, request.GoalId, now);

            if (request.NewCardsLimit is int newLimit)
            {
                var seenNew = 0;
                due = due.Where(s => s.State != CardState.New || seenNew++ < newLimit).ToList();
            }

            var dueIds = due.Select(s => s.CardId).ToList();
            dueIds = ApplyShuffleAndCap(dueIds, request);

            var stateByCard = due.ToDictionary(s => s.CardId);
            sessionCards = dueIds
                .Select(id => new SessionCardDto(
                    id,
                    cardById[id].Front,
                    cardById[id].Back,
                    stateByCard[id].State.ToString(),
                    stateByCard[id].NextReviewDate))
                .ToList();
        }

        var session = new StudySession
        {
            UserId = userId,
            DeckId = deckId,
            GoalId = request.GoalId,
            Mode = mode,
            StartedAt = now
        };
        db.StudySessions.Add(session);
        await db.SaveChangesAsync();

        return new SessionDto(session.Id, deckId, mode.ToString(), session.StartedAt, sessionCards, null);
    }

    public async Task<ReviewResultDto> SubmitReviewAsync(int sessionId, int userId, SubmitReviewRequest request)
    {
        if (request.Rating is < 1 or > 4)
            throw new ArgumentException("Rating must be between 1 (Again) and 4 (Easy)");

        var session = await db.StudySessions.FirstOrDefaultAsync(s => s.Id == sessionId)
            ?? throw new KeyNotFoundException("Session not found");
        if (session.UserId != userId)
            throw new UnauthorizedAccessException("You do not own this session");
        if (session.EndedAt != null)
            throw new ArgumentException("Session has already ended");

        var now = DateTime.UtcNow;

        // Free Study logs the session only — no ReviewLogs, no memory-state updates.
        if (session.Mode == StudyMode.Free)
        {
            session.CardsStudied++;
            await db.SaveChangesAsync();
            return new ReviewResultDto(request.CardId, "Free", null, null);
        }

        var card = await db.Cards.FirstOrDefaultAsync(c => c.Id == request.CardId)
            ?? throw new KeyNotFoundException("Card not found");
        // Card must belong to the session's deck (and therefore the user's deck).
        if (session.DeckId != null && card.DeckId != session.DeckId)
            throw new ArgumentException("Card does not belong to this session's deck");

        var state = await db.CardMemoryStates
            .FirstOrDefaultAsync(s => s.UserId == userId && s.CardId == request.CardId);
        var isNewState = state is null;
        state ??= new CardMemoryState { UserId = userId, CardId = request.CardId, State = CardState.New };

        var stabilityBefore = state.Stability;
        var retrievability = state.State == CardState.Review
            ? fsrs.ComputeRetrievability(state, now)
            : (float?)null;

        fsrs.ProcessReview(state, request.Rating, now);

        if (isNewState) db.CardMemoryStates.Add(state);

        db.ReviewLogs.Add(new ReviewLog
        {
            UserId = userId,
            CardId = request.CardId,
            SessionId = sessionId,
            Rating = request.Rating,
            StabilityBefore = stabilityBefore,
            StabilityAfter = state.Stability,
            Retrievability = retrievability,
            ResponseTimeMs = request.ResponseTimeMs,
            ReviewedAt = now
        });

        session.CardsStudied++;
        if (request.Rating >= 3) session.CorrectCount++;

        await db.SaveChangesAsync();

        return new ReviewResultDto(request.CardId, state.State.ToString(), state.NextReviewDate, retrievability);
    }

    public async Task<SessionDto> EndSessionAsync(int sessionId, int userId)
    {
        var session = await db.StudySessions.FirstOrDefaultAsync(s => s.Id == sessionId)
            ?? throw new KeyNotFoundException("Session not found");
        if (session.UserId != userId)
            throw new UnauthorizedAccessException("You do not own this session");

        session.EndedAt ??= DateTime.UtcNow;
        await db.SaveChangesAsync();

        return new SessionDto(session.Id, session.DeckId ?? 0, session.Mode.ToString(),
            session.StartedAt, [], null);
    }

    // --- helpers ---

    private static StudyMode ParseMode(string? value)
    {
        if (string.IsNullOrWhiteSpace(value)) return StudyMode.Spaced;
        return Enum.TryParse<StudyMode>(value, ignoreCase: true, out var m)
            ? m
            : throw new ArgumentException($"Invalid study mode: {value}");
    }

    private static List<int> ApplyShuffleAndCap(List<int> ids, StartSessionRequest request)
    {
        if (request.Shuffle)
            ids = ids.OrderBy(_ => Guid.NewGuid()).ToList();
        if (request.CardCountCap is int cap && cap >= 0)
            ids = ids.Take(cap).ToList();
        return ids;
    }
}

using Retainica.Api.Data;
using Retainica.Api.DTOs.Goals;
using Retainica.Api.Models;
using Retainica.Api.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Retainica.Api.Services;

public class GoalService(AppDbContext db) : IGoalService
{
    // Default daily review target when the client doesn't supply one (Anki-norm ~20/day).
    private const int DefaultDailyTarget = 20;
    // How far back we look when computing a goal's streak.
    private const int StreakWindowDays = 180;

    public async Task<List<GoalDto>> GetGoalsAsync(int userId)
    {
        var goals = await db.Goals
            .Where(g => g.UserId == userId)
            .Include(g => g.GoalDecks).ThenInclude(gd => gd.Deck)
            .OrderByDescending(g => g.CreatedAt)
            .ToListAsync();

        if (goals.Count == 0) return [];

        var now = DateTime.UtcNow;
        var today = DateOnly.FromDateTime(now);
        var since = now.AddDays(-StreakWindowDays);

        // Pull the user's recent reviews once (review time + owning deck) and reuse per goal.
        var reviews = await db.ReviewLogs
            .Where(r => r.UserId == userId && r.ReviewedAt >= since)
            .Select(r => new { r.ReviewedAt, DeckId = r.Card.DeckId })
            .ToListAsync();

        return goals.Select(g => ToDto(g, reviews.Select(r => (r.ReviewedAt, r.DeckId)), today)).ToList();
    }

    public async Task<GoalDto> GetGoalAsync(int goalId, int userId)
    {
        var goal = await db.Goals
            .Include(g => g.GoalDecks).ThenInclude(gd => gd.Deck)
            .FirstOrDefaultAsync(g => g.Id == goalId)
            ?? throw new KeyNotFoundException("Goal not found");
        if (goal.UserId != userId)
            throw new UnauthorizedAccessException("You do not own this goal");

        var now = DateTime.UtcNow;
        var since = now.AddDays(-StreakWindowDays);
        var reviews = await db.ReviewLogs
            .Where(r => r.UserId == userId && r.ReviewedAt >= since)
            .Select(r => new { r.ReviewedAt, DeckId = r.Card.DeckId })
            .ToListAsync();

        return ToDto(goal, reviews.Select(r => (r.ReviewedAt, r.DeckId)), DateOnly.FromDateTime(now));
    }

    public async Task<GoalDto> CreateGoalAsync(int userId, CreateGoalRequest request)
    {
        var target = request.DailyReviewTarget > 0 ? request.DailyReviewTarget : DefaultDailyTarget;
        var deckIds = (request.DeckIds ?? []).Distinct().ToList();

        if (deckIds.Count > 0)
        {
            var owned = await db.Decks
                .Where(d => d.UserId == userId && deckIds.Contains(d.Id))
                .Select(d => d.Id)
                .ToListAsync();
            var missing = deckIds.Except(owned).ToList();
            if (missing.Count > 0)
                throw new ArgumentException($"Deck(s) not found or not owned: {string.Join(", ", missing)}");
        }

        var goal = new Goal
        {
            UserId = userId,
            Label = string.IsNullOrWhiteSpace(request.Label) ? null : request.Label.Trim(),
            // Deadline is optional in lite mode; default 30 days out to satisfy the schema.
            DeadlineDate = request.DeadlineDate ?? DateTime.UtcNow.Date.AddDays(30),
            DailyReviewBudget = target,
            DailyNewCardBudget = 0,
            Status = GoalStatus.Active,
            GoalDecks = deckIds.Select(id => new GoalDeck { DeckId = id }).ToList()
        };

        db.Goals.Add(goal);
        await db.SaveChangesAsync();

        // Reload deck titles for the response.
        await db.Entry(goal).Collection(g => g.GoalDecks).Query().Include(gd => gd.Deck).LoadAsync();
        return ToDto(goal, [], DateOnly.FromDateTime(DateTime.UtcNow));
    }

    public async Task DeleteGoalAsync(int goalId, int userId)
    {
        var goal = await db.Goals.FirstOrDefaultAsync(g => g.Id == goalId)
            ?? throw new KeyNotFoundException("Goal not found");
        if (goal.UserId != userId)
            throw new UnauthorizedAccessException("You do not own this goal");

        db.Goals.Remove(goal);
        await db.SaveChangesAsync();
    }

    // --- Full-featured path (Phase 2): deadline budgets, schedule preview, at-risk cards, extend. ---
    public Task<GoalDto> UpdateGoalAsync(int goalId, int userId, UpdateGoalRequest request) => throw new NotImplementedException();
    public Task<GoalScheduleDto> GetScheduleAsync(int goalId, int userId) => throw new NotImplementedException();
    public Task<List<AtRiskCardDto>> GetAtRiskCardsAsync(int goalId, int userId) => throw new NotImplementedException();
    public Task<GoalDto> ExtendGoalAsync(int goalId, int userId, ExtendGoalRequest request) => throw new NotImplementedException();

    // --- helpers ---

    private static GoalDto ToDto(Goal g, IEnumerable<(DateTime ReviewedAt, int DeckId)> reviews, DateOnly today)
    {
        var deckIds = g.GoalDecks.Select(gd => gd.DeckId).ToHashSet();

        // A goal with no deck scope counts reviews across all decks.
        var scoped = deckIds.Count == 0
            ? reviews
            : reviews.Where(r => deckIds.Contains(r.DeckId));

        var perDay = scoped
            .GroupBy(r => DateOnly.FromDateTime(r.ReviewedAt))
            .ToDictionary(grp => grp.Key, grp => grp.Count());

        var target = g.DailyReviewBudget > 0 ? g.DailyReviewBudget : DefaultDailyTarget;
        var reviewsToday = perDay.GetValueOrDefault(today, 0);
        var qualifyingDays = perDay.Where(kv => kv.Value >= target).Select(kv => kv.Key).ToHashSet();
        var streak = CurrentStreak(qualifyingDays, today);
        var progressPct = Math.Min(100d, Math.Round(reviewsToday / (double)target * 100d, 0));

        var deckDtos = g.GoalDecks
            .Where(gd => gd.Deck != null)
            .Select(gd => new GoalDeckDto(gd.DeckId, gd.Deck.Title, gd.Deck.Cards?.Count ?? 0))
            .ToList();

        return new GoalDto(g.Id, g.Label, g.DeadlineDate, g.MasteryThresholdPct, g.RecallTargetPct,
            g.DailyNewCardBudget, g.DailyReviewBudget, g.Status.ToString(), deckDtos, g.CreatedAt,
            reviewsToday, streak, progressPct);
    }

    // Consecutive qualifying days ending today (or yesterday if today isn't met yet — the
    // streak isn't broken until a full day passes without hitting the target).
    private static int CurrentStreak(HashSet<DateOnly> days, DateOnly today)
    {
        var cursor = days.Contains(today) ? today : today.AddDays(-1);
        var n = 0;
        while (days.Contains(cursor))
        {
            n++;
            cursor = cursor.AddDays(-1);
        }
        return n;
    }
}

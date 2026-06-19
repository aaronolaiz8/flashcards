using Retainica.Api.Data;
using Retainica.Api.DTOs.Analytics;
using Retainica.Api.Models;
using Retainica.Api.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Retainica.Api.Services;

public class AnalyticsService(AppDbContext db, IGoalService goalService) : IAnalyticsService
{
    private const int ForecastDays = 7;
    private const int ActivityDays = 14;
    private const int MaxDeckUsageRows = 6;

    public async Task<DashboardOverviewDto> GetOverviewAsync(int userId)
    {
        var now = DateTime.UtcNow;
        var today = DateOnly.FromDateTime(now);

        var decks = await db.Decks
            .Where(d => d.UserId == userId)
            .Select(d => new { d.Id, d.Title, CardCount = d.Cards.Count })
            .ToListAsync();
        var totalDecks = decks.Count;
        var totalCards = decks.Sum(d => d.CardCount);

        // Per-card memory states (with owning deck) — drives due counts and the forecast.
        var states = await db.CardMemoryStates
            .Where(s => s.UserId == userId)
            .Select(s => new { s.State, s.NextReviewDate, DeckId = s.Card.DeckId })
            .ToListAsync();

        // Cards that have never been reviewed have no state row → they're "New" and available.
        var statesPerDeck = states.GroupBy(s => s.DeckId).ToDictionary(g => g.Key, g => g.Count());
        var newAvailable = decks.Sum(d => Math.Max(0, d.CardCount - statesPerDeck.GetValueOrDefault(d.Id, 0)));

        DateOnly? DueDate(DateTime? d) => d.HasValue ? DateOnly.FromDateTime(d.Value) : null;
        var dueScheduledToday = states.Count(s => DueDate(s.NextReviewDate) is { } dd && dd <= today);
        var newStateRows = states.Count(s => s.State == CardState.New && s.NextReviewDate == null);
        var cardsDueToday = newAvailable + newStateRows + dueScheduledToday;

        // 7-day due forecast (today's bucket = everything due now incl. new + overdue).
        var dueForecast = new List<DueForecastDto>(ForecastDays);
        for (var i = 0; i < ForecastDays; i++)
        {
            var date = today.AddDays(i);
            var count = i == 0
                ? cardsDueToday
                : states.Count(s => DueDate(s.NextReviewDate) is { } dd && dd == date);
            dueForecast.Add(new DueForecastDto(date, count));
        }

        // Most-due deck → powers the one-click "Study now" target.
        var mostDue = decks
            .Select(d => new
            {
                d.Id,
                d.Title,
                Due = Math.Max(0, d.CardCount - statesPerDeck.GetValueOrDefault(d.Id, 0))
                      + states.Count(s => s.DeckId == d.Id && DueDate(s.NextReviewDate) is { } dd && dd <= today)
            })
            .Where(d => d.Due > 0)
            .OrderByDescending(d => d.Due)
            .FirstOrDefault();

        // Study activity: pull raw timestamps and bucket client-side (avoids provider date-trunc quirks).
        var sessionStarts = await db.StudySessions
            .Where(s => s.UserId == userId)
            .Select(s => s.StartedAt)
            .ToListAsync();
        var sessionDays = sessionStarts.Select(DateOnly.FromDateTime).ToHashSet();
        var currentStreak = CurrentStreak(sessionDays, today);
        var longestStreak = LongestStreak(sessionDays);

        var activitySince = now.AddDays(-(ActivityDays - 1)).Date;
        var recentReviews = await db.ReviewLogs
            .Where(r => r.UserId == userId && r.ReviewedAt >= activitySince)
            .Select(r => r.ReviewedAt)
            .ToListAsync();
        var reviewsByDayMap = recentReviews
            .GroupBy(DateOnly.FromDateTime)
            .ToDictionary(g => g.Key, g => g.Count());
        var reviewsByDay = Enumerable.Range(0, ActivityDays)
            .Select(i => today.AddDays(-(ActivityDays - 1) + i))
            .Select(date => new DailyReviewsDto(date, reviewsByDayMap.GetValueOrDefault(date, 0)))
            .ToList();
        var reviewsToday = reviewsByDayMap.GetValueOrDefault(today, 0);

        // Deck usage (most-reviewed decks, all-time) → most-used-deck pie.
        var usageDeckIds = await db.ReviewLogs
            .Where(r => r.UserId == userId)
            .Select(r => r.Card.DeckId)
            .ToListAsync();
        var deckTitles = decks.ToDictionary(d => d.Id, d => d.Title);
        var deckUsage = usageDeckIds
            .GroupBy(id => id)
            .Select(g => new DeckUsageDto(g.Key, deckTitles.GetValueOrDefault(g.Key, "Deleted deck"), g.Count()))
            .OrderByDescending(u => u.Reviews)
            .Take(MaxDeckUsageRows)
            .ToList();

        // Goals (reuse the goal-streak/progress logic).
        var goals = await goalService.GetGoalsAsync(userId);
        var activeGoals = goals.Where(g => g.Status == nameof(GoalStatus.Active)).ToList();
        var goalDtos = activeGoals
            .Select(g => new DashboardGoalDto(g.Id, g.Label, g.DailyReviewBudget,
                g.ReviewsToday, g.CurrentStreak, g.ProgressPct))
            .ToList();

        var isEmpty = totalDecks == 0 && goals.Count == 0;

        return new DashboardOverviewDto(
            totalDecks, totalCards, cardsDueToday, reviewsToday,
            currentStreak, longestStreak, activeGoals.Count,
            mostDue?.Id, mostDue?.Title,
            dueForecast, deckUsage, reviewsByDay, goalDtos, isEmpty);
    }

    public Task<DashboardDto> GetDashboardAsync(int userId) => throw new NotImplementedException();
    public Task<DeckAnalyticsDto> GetDeckAnalyticsAsync(int deckId, int userId) => throw new NotImplementedException();
    public Task<CardHistoryDto> GetCardHistoryAsync(int cardId, int userId) => throw new NotImplementedException();

    // --- helpers ---

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

    private static int LongestStreak(HashSet<DateOnly> days)
    {
        if (days.Count == 0) return 0;
        var longest = 0;
        foreach (var day in days)
        {
            // Count a run only from its start (no earlier neighbour).
            if (days.Contains(day.AddDays(-1))) continue;
            var run = 0;
            var cursor = day;
            while (days.Contains(cursor))
            {
                run++;
                cursor = cursor.AddDays(1);
            }
            longest = Math.Max(longest, run);
        }
        return longest;
    }
}

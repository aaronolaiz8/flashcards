namespace Retainica.Api.DTOs.Analytics;

public record DashboardDto(int TotalCardsStudied, int CurrentStreak, int LongestStreak,
    double AverageStudyMinutesPerDay, List<HeatmapEntryDto> Heatmap);

public record HeatmapEntryDto(DateOnly Date, int Count);

public record DeckAnalyticsDto(int DeckId, string Title, int CardsDueToday,
    int NewCardsRemaining, double RetentionRateLast30Days, int Streak);

public record CardHistoryDto(int CardId, string Front, List<ReviewLogDto> Reviews);

public record ReviewLogDto(DateTime ReviewedAt, int Rating, float? StabilityAfter, float? Retrievability);

// --- Dashboard overview (home page) ---

public record DashboardOverviewDto(
    int TotalDecks,
    int TotalCards,
    int CardsDueToday,
    int ReviewsToday,
    int CurrentStreak,
    int LongestStreak,
    int ActiveGoals,
    int? NextDeckId,
    string? NextDeckTitle,
    List<DueForecastDto> DueForecast,
    List<DeckUsageDto> DeckUsage,
    List<DailyReviewsDto> ReviewsByDay,
    List<DashboardGoalDto> Goals,
    bool IsEmpty);

public record DueForecastDto(DateOnly Date, int Count);

public record DeckUsageDto(int DeckId, string Title, int Reviews);

public record DailyReviewsDto(DateOnly Date, int Count);

public record DashboardGoalDto(int Id, string? Label, int DailyReviewTarget,
    int ReviewsToday, int CurrentStreak, double ProgressPct);

namespace FlashcardsApp.Api.DTOs.Analytics;

public record DashboardDto(int TotalCardsStudied, int CurrentStreak, int LongestStreak,
    double AverageStudyMinutesPerDay, List<HeatmapEntryDto> Heatmap);

public record HeatmapEntryDto(DateOnly Date, int Count);

public record DeckAnalyticsDto(int DeckId, string Title, int CardsDueToday,
    int NewCardsRemaining, double RetentionRateLast30Days, int Streak);

public record CardHistoryDto(int CardId, string Front, List<ReviewLogDto> Reviews);

public record ReviewLogDto(DateTime ReviewedAt, int Rating, float? StabilityAfter, float? Retrievability);

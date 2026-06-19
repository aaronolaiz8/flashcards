namespace Retainica.Api.DTOs.Goals;

public record GoalDeckRequest(int DeckId, string[]? CardFilterTags);

// Lite create shape: a daily review target, optional deck scope, optional deadline.
// The full-featured path (deadline-budget recalculation, per-deck tag filters) layers
// onto the same Goal schema later — DeckIds maps to GoalDecks, DeadlineDate is preserved.
public record CreateGoalRequest(string? Label, int DailyReviewTarget,
    List<int>? DeckIds, DateTime? DeadlineDate);

public record UpdateGoalRequest(string? Label, DateTime? DeadlineDate, int? MasteryThresholdPct, int? RecallTargetPct);

public record ExtendGoalRequest(DateTime NewDeadlineDate);

public record GoalDeckDto(int DeckId, string DeckTitle, int CardCount);

public record GoalDto(int Id, string? Label, DateTime DeadlineDate, int MasteryThresholdPct,
    int RecallTargetPct, int DailyNewCardBudget, int DailyReviewBudget, string Status,
    List<GoalDeckDto> Decks, DateTime CreatedAt,
    int ReviewsToday, int CurrentStreak, double ProgressPct);

public record GoalScheduleDto(int GoalId, int DaysRemaining, List<DailyBudgetDto> DailyBreakdown);

public record DailyBudgetDto(DateOnly Date, int NewCards, int ReviewCards);

public record AtRiskCardDto(int CardId, string Front, float ProjectedRetrievability, int DeckId, string DeckTitle);

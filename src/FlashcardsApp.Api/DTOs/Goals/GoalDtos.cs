namespace FlashcardsApp.Api.DTOs.Goals;

public record GoalDeckRequest(int DeckId, string[]? CardFilterTags);

public record CreateGoalRequest(string? Label, DateTime DeadlineDate,
    List<GoalDeckRequest> Decks, int MasteryThresholdPct = 100, int RecallTargetPct = 90);

public record UpdateGoalRequest(string? Label, DateTime? DeadlineDate, int? MasteryThresholdPct, int? RecallTargetPct);

public record ExtendGoalRequest(DateTime NewDeadlineDate);

public record GoalDeckDto(int DeckId, string DeckTitle, int CardCount);

public record GoalDto(int Id, string? Label, DateTime DeadlineDate, int MasteryThresholdPct,
    int RecallTargetPct, int DailyNewCardBudget, int DailyReviewBudget, string Status,
    List<GoalDeckDto> Decks, DateTime CreatedAt);

public record GoalScheduleDto(int GoalId, int DaysRemaining, List<DailyBudgetDto> DailyBreakdown);

public record DailyBudgetDto(DateOnly Date, int NewCards, int ReviewCards);

public record AtRiskCardDto(int CardId, string Front, float ProjectedRetrievability, int DeckId, string DeckTitle);

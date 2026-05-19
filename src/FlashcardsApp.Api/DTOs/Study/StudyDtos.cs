namespace FlashcardsApp.Api.DTOs.Study;

public record StartSessionRequest(int DeckId, string Mode, int? GoalId, int? CardCountCap, int? NewCardsLimit, bool Shuffle = false);

public record SessionCardDto(int CardId, string Front, string Back, string State, DateTime? NextReviewDate);

public record SessionDto(int Id, int DeckId, string Mode, DateTime StartedAt,
    List<SessionCardDto> Cards, PaceInfoDto? PaceInfo);

public record PaceInfoDto(string Status, int CardsOnPace, int CardsBehind, int CardsAhead);

public record SubmitReviewRequest(int CardId, int Rating, int? ResponseTimeMs);

public record ReviewResultDto(int CardId, string NewState, DateTime? NextReviewDate, float? Retrievability);

namespace Retainica.Api.DTOs.Ai;

public record GenerateCardsRequest(string? Topic, string? Text, int Count, string CardType = "Basic", int? DeckId = null);
public record GeneratedCardDto(string Front, string Back);

namespace Memora.Api.DTOs.Ai;

public record GenerateCardsRequest(string? Topic, string? Text, int Count, string CardType = "Basic");
public record GeneratedCardDto(string Front, string Back);

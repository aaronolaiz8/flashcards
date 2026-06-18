namespace Retainica.Api.DTOs.Cards;

public record CardDto(int Id, int DeckId, string Front, string Back, string[] Tags,
    DateTime CreatedAt, DateTime UpdatedAt);

public record CreateCardRequest(string Front, string Back, string[]? Tags);
public record UpdateCardRequest(string? Front, string? Back, string[]? Tags);

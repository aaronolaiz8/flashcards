namespace Retainica.Api.DTOs.Decks;

public record DeckSummaryDto(int Id, string Title, string? Description, string[] Tags,
    string Visibility, int CardCount, DateTime CreatedAt, DateTime UpdatedAt);

public record DeckDetailDto(int Id, string Title, string? Description, string[] Tags,
    string Visibility, int? ForkedFromDeckId, int CardCount, DateTime CreatedAt, DateTime UpdatedAt);

public record CreateDeckRequest(string Title, string? Description, string[]? Tags, string Visibility = "Private");
public record UpdateDeckRequest(string? Title, string? Description, string[]? Tags, string? Visibility);
public record ImportDeckRequest(string Format, string Content, string? Title = null);
public record PagedResult<T>(List<T> Items, int TotalCount, int Page, int PageSize);

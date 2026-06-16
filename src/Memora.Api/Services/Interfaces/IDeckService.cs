using Memora.Api.DTOs.Decks;

namespace Memora.Api.Services.Interfaces;

public interface IDeckService
{
    Task<List<DeckSummaryDto>> GetUserDecksAsync(int userId);
    Task<DeckDetailDto> GetDeckAsync(int deckId, int userId);
    Task<DeckDetailDto> CreateDeckAsync(int userId, CreateDeckRequest request);
    Task<DeckDetailDto> UpdateDeckAsync(int deckId, int userId, UpdateDeckRequest request);
    Task DeleteDeckAsync(int deckId, int userId);
    Task<DeckDetailDto> ForkDeckAsync(int deckId, int userId);
    Task<PagedResult<DeckSummaryDto>> SearchPublicDecksAsync(string? query, string[]? tags, int page, int pageSize);
    Task<byte[]> ExportDeckAsync(int deckId, int userId, string format);
    Task<DeckDetailDto> ImportDeckAsync(int userId, ImportDeckRequest request);
}

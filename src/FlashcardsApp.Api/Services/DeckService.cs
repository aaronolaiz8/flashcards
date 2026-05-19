using FlashcardsApp.Api.Data;
using FlashcardsApp.Api.DTOs.Decks;
using FlashcardsApp.Api.Services.Interfaces;

namespace FlashcardsApp.Api.Services;

public class DeckService(AppDbContext db) : IDeckService
{
    public Task<List<DeckSummaryDto>> GetUserDecksAsync(int userId) => throw new NotImplementedException();
    public Task<DeckDetailDto> GetDeckAsync(int deckId, int userId) => throw new NotImplementedException();
    public Task<DeckDetailDto> CreateDeckAsync(int userId, CreateDeckRequest request) => throw new NotImplementedException();
    public Task<DeckDetailDto> UpdateDeckAsync(int deckId, int userId, UpdateDeckRequest request) => throw new NotImplementedException();
    public Task DeleteDeckAsync(int deckId, int userId) => throw new NotImplementedException();
    public Task<DeckDetailDto> ForkDeckAsync(int deckId, int userId) => throw new NotImplementedException();
    public Task<PagedResult<DeckSummaryDto>> SearchPublicDecksAsync(string? query, string[]? tags, int page, int pageSize) => throw new NotImplementedException();
    public Task<byte[]> ExportDeckAsync(int deckId, int userId, string format) => throw new NotImplementedException();
    public Task<DeckDetailDto> ImportDeckAsync(int userId, ImportDeckRequest request) => throw new NotImplementedException();
}

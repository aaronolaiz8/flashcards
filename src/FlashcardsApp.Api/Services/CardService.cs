using FlashcardsApp.Api.Data;
using FlashcardsApp.Api.DTOs.Cards;
using FlashcardsApp.Api.Services.Interfaces;

namespace FlashcardsApp.Api.Services;

public class CardService(AppDbContext db) : ICardService
{
    public Task<List<CardDto>> GetCardsAsync(int deckId, int userId) => throw new NotImplementedException();
    public Task<CardDto> CreateCardAsync(int deckId, int userId, CreateCardRequest request) => throw new NotImplementedException();
    public Task<CardDto> UpdateCardAsync(int deckId, int cardId, int userId, UpdateCardRequest request) => throw new NotImplementedException();
    public Task DeleteCardAsync(int deckId, int cardId, int userId) => throw new NotImplementedException();
    public Task<List<CardDto>> BulkCreateCardsAsync(int deckId, int userId, List<CreateCardRequest> cards) => throw new NotImplementedException();
}

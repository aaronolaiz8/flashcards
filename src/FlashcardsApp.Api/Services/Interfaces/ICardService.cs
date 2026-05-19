using FlashcardsApp.Api.DTOs.Cards;

namespace FlashcardsApp.Api.Services.Interfaces;

public interface ICardService
{
    Task<List<CardDto>> GetCardsAsync(int deckId, int userId);
    Task<CardDto> CreateCardAsync(int deckId, int userId, CreateCardRequest request);
    Task<CardDto> UpdateCardAsync(int deckId, int cardId, int userId, UpdateCardRequest request);
    Task DeleteCardAsync(int deckId, int cardId, int userId);
    Task<List<CardDto>> BulkCreateCardsAsync(int deckId, int userId, List<CreateCardRequest> cards);
}

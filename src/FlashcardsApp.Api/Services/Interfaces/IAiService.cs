using FlashcardsApp.Api.DTOs.Ai;

namespace FlashcardsApp.Api.Services.Interfaces;

public interface IAiService
{
    Task<List<GeneratedCardDto>> GenerateCardsAsync(int userId, GenerateCardsRequest request);
}

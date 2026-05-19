using FlashcardsApp.Api.Data;
using FlashcardsApp.Api.DTOs.Ai;
using FlashcardsApp.Api.Services.Interfaces;

namespace FlashcardsApp.Api.Services;

public class AiService(AppDbContext db) : IAiService
{
    public Task<List<GeneratedCardDto>> GenerateCardsAsync(int userId, GenerateCardsRequest request) => throw new NotImplementedException();
}

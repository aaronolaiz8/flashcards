using Memora.Api.DTOs.Ai;

namespace Memora.Api.Services.Interfaces;

public interface IAiService
{
    Task<List<GeneratedCardDto>> GenerateCardsAsync(int userId, GenerateCardsRequest request);
}

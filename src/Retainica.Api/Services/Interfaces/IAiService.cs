using Retainica.Api.DTOs.Ai;

namespace Retainica.Api.Services.Interfaces;

public interface IAiService
{
    Task<List<GeneratedCardDto>> GenerateCardsAsync(int userId, GenerateCardsRequest request);
}

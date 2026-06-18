using Retainica.Api.Data;
using Retainica.Api.DTOs.Ai;
using Retainica.Api.Services.Interfaces;

namespace Retainica.Api.Services;

public class AiService(AppDbContext db) : IAiService
{
    public Task<List<GeneratedCardDto>> GenerateCardsAsync(int userId, GenerateCardsRequest request) => throw new NotImplementedException();
}

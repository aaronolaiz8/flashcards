using Memora.Api.Data;
using Memora.Api.DTOs.Ai;
using Memora.Api.Services.Interfaces;

namespace Memora.Api.Services;

public class AiService(AppDbContext db) : IAiService
{
    public Task<List<GeneratedCardDto>> GenerateCardsAsync(int userId, GenerateCardsRequest request) => throw new NotImplementedException();
}

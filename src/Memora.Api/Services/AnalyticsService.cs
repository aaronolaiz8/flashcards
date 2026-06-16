using Memora.Api.Data;
using Memora.Api.DTOs.Analytics;
using Memora.Api.Services.Interfaces;

namespace Memora.Api.Services;

public class AnalyticsService(AppDbContext db) : IAnalyticsService
{
    public Task<DashboardDto> GetDashboardAsync(int userId) => throw new NotImplementedException();
    public Task<DeckAnalyticsDto> GetDeckAnalyticsAsync(int deckId, int userId) => throw new NotImplementedException();
    public Task<CardHistoryDto> GetCardHistoryAsync(int cardId, int userId) => throw new NotImplementedException();
}

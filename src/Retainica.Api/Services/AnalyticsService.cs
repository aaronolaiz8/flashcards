using Retainica.Api.Data;
using Retainica.Api.DTOs.Analytics;
using Retainica.Api.Services.Interfaces;

namespace Retainica.Api.Services;

public class AnalyticsService(AppDbContext db) : IAnalyticsService
{
    public Task<DashboardDto> GetDashboardAsync(int userId) => throw new NotImplementedException();
    public Task<DeckAnalyticsDto> GetDeckAnalyticsAsync(int deckId, int userId) => throw new NotImplementedException();
    public Task<CardHistoryDto> GetCardHistoryAsync(int cardId, int userId) => throw new NotImplementedException();
}

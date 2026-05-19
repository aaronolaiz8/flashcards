using FlashcardsApp.Api.Data;
using FlashcardsApp.Api.DTOs.Analytics;
using FlashcardsApp.Api.Services.Interfaces;

namespace FlashcardsApp.Api.Services;

public class AnalyticsService(AppDbContext db) : IAnalyticsService
{
    public Task<DashboardDto> GetDashboardAsync(int userId) => throw new NotImplementedException();
    public Task<DeckAnalyticsDto> GetDeckAnalyticsAsync(int deckId, int userId) => throw new NotImplementedException();
    public Task<CardHistoryDto> GetCardHistoryAsync(int cardId, int userId) => throw new NotImplementedException();
}

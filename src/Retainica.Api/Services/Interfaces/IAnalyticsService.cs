using Retainica.Api.DTOs.Analytics;

namespace Retainica.Api.Services.Interfaces;

public interface IAnalyticsService
{
    Task<DashboardDto> GetDashboardAsync(int userId);
    Task<DeckAnalyticsDto> GetDeckAnalyticsAsync(int deckId, int userId);
    Task<CardHistoryDto> GetCardHistoryAsync(int cardId, int userId);
}

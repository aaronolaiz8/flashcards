using Retainica.Api.DTOs.Analytics;
using Retainica.Api.Extensions;
using Retainica.Api.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Retainica.Api.Controllers;

[ApiController]
[Route("api/analytics")]
[Authorize]
public class AnalyticsController(IAnalyticsService analyticsService) : ControllerBase
{
    [HttpGet("dashboard")]
    public async Task<ActionResult<DashboardDto>> GetDashboard()
    {
        var result = await analyticsService.GetDashboardAsync(User.GetUserId());
        return Ok(result);
    }

    [HttpGet("decks/{deckId:int}")]
    public async Task<ActionResult<DeckAnalyticsDto>> GetDeckAnalytics(int deckId)
    {
        var result = await analyticsService.GetDeckAnalyticsAsync(deckId, User.GetUserId());
        return Ok(result);
    }

    [HttpGet("cards/{cardId:int}/history")]
    public async Task<ActionResult<CardHistoryDto>> GetCardHistory(int cardId)
    {
        var result = await analyticsService.GetCardHistoryAsync(cardId, User.GetUserId());
        return Ok(result);
    }
}

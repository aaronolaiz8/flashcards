using FlashcardsApp.Api.DTOs.Settings;
using FlashcardsApp.Api.Extensions;
using FlashcardsApp.Api.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FlashcardsApp.Api.Controllers;

[ApiController]
[Route("api/settings")]
[Authorize]
public class SettingsController(IAiSettingsService aiSettingsService) : ControllerBase
{
    [HttpGet("ai")]
    public async Task<ActionResult<AiSettingsDto>> GetAiSettings()
    {
        var result = await aiSettingsService.GetAsync(User.GetUserId());
        return Ok(result);
    }

    [HttpPut("ai")]
    public async Task<ActionResult<AiSettingsDto>> SaveAiSettings(SaveAiSettingsRequest request)
    {
        var result = await aiSettingsService.SaveAsync(User.GetUserId(), request);
        return Ok(result);
    }

    [HttpDelete("ai")]
    public async Task<IActionResult> DeleteAiSettings()
    {
        await aiSettingsService.DeleteAsync(User.GetUserId());
        return NoContent();
    }
}

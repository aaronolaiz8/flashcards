using Memora.Api.DTOs.Reminders;
using Memora.Api.Extensions;
using Memora.Api.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Memora.Api.Controllers;

[ApiController]
[Route("api/reminders")]
[Authorize]
public class RemindersController(IReminderService reminderService) : ControllerBase
{
    [HttpGet("preferences")]
    public async Task<ActionResult<NotificationPreferencesDto>> GetPreferences()
    {
        var result = await reminderService.GetPreferencesAsync(User.GetUserId());
        return Ok(result);
    }

    [HttpPut("preferences")]
    public async Task<ActionResult<NotificationPreferencesDto>> UpdatePreferences(UpdatePreferencesRequest request)
    {
        var result = await reminderService.UpdatePreferencesAsync(User.GetUserId(), request);
        return Ok(result);
    }

    [HttpGet("scheduled")]
    public async Task<ActionResult<List<ScheduledReminderDto>>> GetScheduled()
    {
        var result = await reminderService.GetScheduledAsync(User.GetUserId());
        return Ok(result);
    }

    [HttpPost("snooze/{id:int}")]
    public async Task<IActionResult> Snooze(int id, SnoozeRequest request)
    {
        await reminderService.SnoozeAsync(id, User.GetUserId(), request);
        return NoContent();
    }
}

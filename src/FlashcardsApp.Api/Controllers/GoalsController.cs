using FlashcardsApp.Api.DTOs.Goals;
using FlashcardsApp.Api.Extensions;
using FlashcardsApp.Api.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FlashcardsApp.Api.Controllers;

[ApiController]
[Route("api/goals")]
[Authorize]
public class GoalsController(IGoalService goalService) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<List<GoalDto>>> GetGoals()
    {
        var result = await goalService.GetGoalsAsync(User.GetUserId());
        return Ok(result);
    }

    [HttpPost]
    public async Task<ActionResult<GoalDto>> CreateGoal(CreateGoalRequest request)
    {
        var result = await goalService.CreateGoalAsync(User.GetUserId(), request);
        return CreatedAtAction(nameof(GetGoal), new { id = result.Id }, result);
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<GoalDto>> GetGoal(int id)
    {
        var result = await goalService.GetGoalAsync(id, User.GetUserId());
        return Ok(result);
    }

    [HttpPut("{id:int}")]
    public async Task<ActionResult<GoalDto>> UpdateGoal(int id, UpdateGoalRequest request)
    {
        var result = await goalService.UpdateGoalAsync(id, User.GetUserId(), request);
        return Ok(result);
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> DeleteGoal(int id)
    {
        await goalService.DeleteGoalAsync(id, User.GetUserId());
        return NoContent();
    }

    [HttpGet("{id:int}/schedule")]
    public async Task<ActionResult<GoalScheduleDto>> GetSchedule(int id)
    {
        var result = await goalService.GetScheduleAsync(id, User.GetUserId());
        return Ok(result);
    }

    [HttpGet("{id:int}/at-risk")]
    public async Task<ActionResult<List<AtRiskCardDto>>> GetAtRiskCards(int id)
    {
        var result = await goalService.GetAtRiskCardsAsync(id, User.GetUserId());
        return Ok(result);
    }

    [HttpPost("{id:int}/extend")]
    public async Task<ActionResult<GoalDto>> ExtendGoal(int id, ExtendGoalRequest request)
    {
        var result = await goalService.ExtendGoalAsync(id, User.GetUserId(), request);
        return Ok(result);
    }
}

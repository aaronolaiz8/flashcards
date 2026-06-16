using Memora.Api.DTOs.Study;
using Memora.Api.Extensions;
using Memora.Api.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Memora.Api.Controllers;

[ApiController]
[Route("api/study")]
[Authorize]
public class StudyController(IStudyService studyService) : ControllerBase
{
    [HttpPost("sessions")]
    public async Task<ActionResult<SessionDto>> StartSession(StartSessionRequest request)
    {
        var result = await studyService.StartSessionAsync(User.GetUserId(), request);
        return Ok(result);
    }

    [HttpPost("sessions/{sessionId:int}/review")]
    public async Task<ActionResult<ReviewResultDto>> SubmitReview(int sessionId, SubmitReviewRequest request)
    {
        var result = await studyService.SubmitReviewAsync(sessionId, User.GetUserId(), request);
        return Ok(result);
    }

    [HttpPost("sessions/{sessionId:int}/end")]
    public async Task<ActionResult<SessionDto>> EndSession(int sessionId)
    {
        var result = await studyService.EndSessionAsync(sessionId, User.GetUserId());
        return Ok(result);
    }
}

using Memora.Api.DTOs.Ai;
using Memora.Api.Extensions;
using Memora.Api.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Memora.Api.Controllers;

[ApiController]
[Route("api/ai")]
[Authorize]
public class AiController(IAiService aiService) : ControllerBase
{
    [HttpPost("generate")]
    public async Task<ActionResult<List<GeneratedCardDto>>> Generate(GenerateCardsRequest request)
    {
        var result = await aiService.GenerateCardsAsync(User.GetUserId(), request);
        return Ok(result);
    }
}

using Memora.Api.DTOs.Decks;
using Memora.Api.Extensions;
using Memora.Api.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Memora.Api.Controllers;

[ApiController]
[Route("api/decks")]
[Authorize]
public class DecksController(IDeckService deckService) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<List<DeckSummaryDto>>> GetDecks()
    {
        var result = await deckService.GetUserDecksAsync(User.GetUserId());
        return Ok(result);
    }

    [HttpPost]
    public async Task<ActionResult<DeckDetailDto>> CreateDeck(CreateDeckRequest request)
    {
        var result = await deckService.CreateDeckAsync(User.GetUserId(), request);
        return CreatedAtAction(nameof(GetDeck), new { id = result.Id }, result);
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<DeckDetailDto>> GetDeck(int id)
    {
        var result = await deckService.GetDeckAsync(id, User.GetUserId());
        return Ok(result);
    }

    [HttpPut("{id:int}")]
    public async Task<ActionResult<DeckDetailDto>> UpdateDeck(int id, UpdateDeckRequest request)
    {
        var result = await deckService.UpdateDeckAsync(id, User.GetUserId(), request);
        return Ok(result);
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> DeleteDeck(int id)
    {
        await deckService.DeleteDeckAsync(id, User.GetUserId());
        return NoContent();
    }

    [HttpPost("{id:int}/fork")]
    public async Task<ActionResult<DeckDetailDto>> ForkDeck(int id)
    {
        var result = await deckService.ForkDeckAsync(id, User.GetUserId());
        return Ok(result);
    }

    [HttpGet("public")]
    [AllowAnonymous]
    public async Task<ActionResult<PagedResult<DeckSummaryDto>>> SearchPublicDecks(
        [FromQuery] string? q, [FromQuery] string[]? tags,
        [FromQuery] int page = 1, [FromQuery] int pageSize = 20)
    {
        var result = await deckService.SearchPublicDecksAsync(q, tags, page, pageSize);
        return Ok(result);
    }

    [HttpGet("{id:int}/export")]
    public async Task<IActionResult> ExportDeck(int id, [FromQuery] string format = "json")
    {
        var content = await deckService.ExportDeckAsync(id, User.GetUserId(), format);
        var contentType = format == "csv" ? "text/csv" : "application/json";
        return File(content, contentType, $"deck-{id}.{format}");
    }

    [HttpPost("import")]
    public async Task<ActionResult<DeckDetailDto>> ImportDeck(ImportDeckRequest request)
    {
        var result = await deckService.ImportDeckAsync(User.GetUserId(), request);
        return Ok(result);
    }
}

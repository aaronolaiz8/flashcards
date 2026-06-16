using Memora.Api.DTOs.Cards;
using Memora.Api.Extensions;
using Memora.Api.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Memora.Api.Controllers;

[ApiController]
[Route("api/decks/{deckId:int}/cards")]
[Authorize]
public class CardsController(ICardService cardService) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<List<CardDto>>> GetCards(int deckId)
    {
        var result = await cardService.GetCardsAsync(deckId, User.GetUserId());
        return Ok(result);
    }

    [HttpPost]
    public async Task<ActionResult<CardDto>> CreateCard(int deckId, CreateCardRequest request)
    {
        var result = await cardService.CreateCardAsync(deckId, User.GetUserId(), request);
        return CreatedAtAction(nameof(GetCards), new { deckId }, result);
    }

    [HttpPut("{cardId:int}")]
    public async Task<ActionResult<CardDto>> UpdateCard(int deckId, int cardId, UpdateCardRequest request)
    {
        var result = await cardService.UpdateCardAsync(deckId, cardId, User.GetUserId(), request);
        return Ok(result);
    }

    [HttpDelete("{cardId:int}")]
    public async Task<IActionResult> DeleteCard(int deckId, int cardId)
    {
        await cardService.DeleteCardAsync(deckId, cardId, User.GetUserId());
        return NoContent();
    }

    [HttpPost("bulk")]
    public async Task<ActionResult<List<CardDto>>> BulkCreate(int deckId, List<CreateCardRequest> cards)
    {
        var result = await cardService.BulkCreateCardsAsync(deckId, User.GetUserId(), cards);
        return Ok(result);
    }
}

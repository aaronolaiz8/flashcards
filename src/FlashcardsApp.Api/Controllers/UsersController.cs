using FlashcardsApp.Api.DTOs.Auth;
using FlashcardsApp.Api.Extensions;
using FlashcardsApp.Api.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FlashcardsApp.Api.Controllers;

[ApiController]
[Route("api/users")]
[Authorize]
public class UsersController(IAuthService authService) : ControllerBase
{
    [HttpGet("me")]
    public async Task<ActionResult<UserDto>> GetProfile()
    {
        var result = await authService.GetProfileAsync(User.GetUserId());
        return Ok(result);
    }

    [HttpPut("me")]
    public async Task<ActionResult<UserDto>> UpdateProfile(UpdateProfileRequest request)
    {
        var result = await authService.UpdateProfileAsync(User.GetUserId(), request);
        return Ok(result);
    }

    [HttpPut("me/password")]
    public async Task<IActionResult> ChangePassword(ChangePasswordRequest request)
    {
        await authService.ChangePasswordAsync(User.GetUserId(), request);
        return NoContent();
    }

    [HttpDelete("me")]
    public async Task<IActionResult> DeleteAccount(DeleteAccountRequest request)
    {
        await authService.DeleteAccountAsync(User.GetUserId(), request.Password);
        return NoContent();
    }
}

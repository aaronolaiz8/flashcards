using System.Security.Claims;

namespace FlashcardsApp.Api.Extensions;

public static class ClaimsPrincipalExtensions
{
    public static int GetUserId(this ClaimsPrincipal principal)
    {
        var claim = principal.FindFirst(ClaimTypes.NameIdentifier)
            ?? throw new InvalidOperationException("User ID claim not found");
        return int.Parse(claim.Value);
    }
}

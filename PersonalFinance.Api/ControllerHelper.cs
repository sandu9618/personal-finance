using System.Security.Claims;

namespace PersonalFinance.Api;

public static class ControllerHelper
{
  public static Guid GetUserIdFromClaims(this ClaimsPrincipal user)
  {
    var userIdClaim = user.FindFirst(ClaimTypes.NameIdentifier)
      ?? user.FindFirst("sub");

    if (userIdClaim == null || !Guid.TryParse(userIdClaim.Value, out var userId))
    {
      throw new InvalidOperationException("User ID claim is missing or invalid.");
    }

    return userId;
  }
}

using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using PersonalFinance.Web.Models;

namespace PersonalFinance.Web.Services;

public static class AuthCookie
{
    public const string AccessTokenClaim = "access_token";

    public static async Task SignInAsync(HttpContext httpContext, AuthResponse auth)
    {
        var claims = new List<Claim>
        {
            new(ClaimTypes.Name, auth.Email),
            new(AccessTokenClaim, auth.Token)
        };

        var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
        await httpContext.SignInAsync(
            CookieAuthenticationDefaults.AuthenticationScheme,
            new ClaimsPrincipal(identity));
    }
}

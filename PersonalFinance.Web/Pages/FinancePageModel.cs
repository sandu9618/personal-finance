using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using PersonalFinance.Web.Services;

namespace PersonalFinance.Web.Pages;

public abstract class FinancePageModel : PageModel
{
    protected FinancePageModel(ApiClient api)
    {
        Api = api;
    }

    protected ApiClient Api { get; }

    public string? Error { get; private set; }

    protected void LoadError()
    {
        if (TempData["Error"] is string message)
        {
            Error = message;
        }
    }

    protected void SaveError(string message) => TempData["Error"] = message;

    protected IActionResult? InvalidForm(Guid? id = null)
    {
        if (ModelState.IsValid)
        {
            return null;
        }

        var messages = ModelState.Values
            .SelectMany(entry => entry.Errors)
            .Select(error => error.ErrorMessage)
            .Where(message => !string.IsNullOrWhiteSpace(message));
        SaveError(string.Join(" ", messages) is { Length: > 0 } text ? text : "The form is invalid.");
        return id is null ? RedirectToPage() : RedirectToPage(new { id });
    }

    protected async Task<IActionResult> Fail(ApiException ex, bool redirect, Guid? id = null)
    {
        if (ex.StatusCode == StatusCodes.Status401Unauthorized)
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToPage("/Login");
        }

        return ShowError(ex.Message, redirect, id);
    }

    protected IActionResult Unreachable(bool redirect, Guid? id = null) =>
        ShowError("The API could not be reached.", redirect, id);

    private IActionResult ShowError(string message, bool redirect, Guid? id)
    {
        if (!redirect)
        {
            Error = message;
            return Page();
        }

        SaveError(message);
        return id is null ? RedirectToPage() : RedirectToPage(new { id });
    }
}

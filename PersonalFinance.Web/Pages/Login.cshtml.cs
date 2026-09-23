using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using PersonalFinance.Web.Models;
using PersonalFinance.Web.Services;

namespace PersonalFinance.Web.Pages;

[AllowAnonymous]
public class LoginModel : PageModel
{
    private readonly ApiClient _api;

    public LoginModel(ApiClient api)
    {
        _api = api;
    }

    [BindProperty]
    public LoginInput Input { get; set; } = new();

    public string? Error { get; private set; }

    public IActionResult OnGet()
    {
        if (TempData["Error"] is string message)
        {
            Error = message;
        }

        if (User.Identity?.IsAuthenticated == true)
        {
            return RedirectToPage("/Index");
        }

        return Page();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid)
        {
            TempData["Error"] = FirstModelError();
            return RedirectToPage();
        }

        try
        {
            var auth = await _api.LoginAsync(new LoginRequest(Input.Email, Input.Password), HttpContext.RequestAborted);
            await AuthCookie.SignInAsync(HttpContext, auth);
            return RedirectToPage("/Index");
        }
        catch (ApiException ex)
        {
            TempData["Error"] = ex.Message;
            return RedirectToPage();
        }
        catch (HttpRequestException)
        {
            TempData["Error"] = "The API could not be reached.";
            return RedirectToPage();
        }
    }

    private string FirstModelError()
    {
        var message = ModelState.Values
            .SelectMany(entry => entry.Errors)
            .Select(error => error.ErrorMessage)
            .FirstOrDefault(error => !string.IsNullOrWhiteSpace(error));
        return string.IsNullOrWhiteSpace(message) ? "The form is invalid." : message;
    }

    public class LoginInput
    {
        public string Email { get; set; } = "";
        public string Password { get; set; } = "";
    }
}

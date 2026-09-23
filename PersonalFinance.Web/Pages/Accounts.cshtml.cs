using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PersonalFinance.Web.Models;

namespace PersonalFinance.Web.Pages;

[Authorize]
public class AccountsModel : FinancePageModel
{
    public AccountsModel(Services.ApiClient api) : base(api)
    {
    }

    public AccountResponse[] Accounts { get; private set; } = [];

    public Guid? Id { get; private set; }

    [BindProperty]
    public AccountInput Input { get; set; } = new();

    public async Task<IActionResult> OnGetAsync(Guid? id)
    {
        LoadError();
        try
        {
            Accounts = (await Api.GetAccountsAsync(HttpContext.RequestAborted)).Accounts ?? [];
            if (id is Guid accountId)
            {
                var account = Accounts.FirstOrDefault(item => item.Id == accountId);
                if (account is not null)
                {
                    Id = account.Id;
                    Input = new AccountInput
                    {
                        Name = account.Name,
                        InitialBalance = account.Balance,
                        Type = account.Type,
                        Currency = account.Currency
                    };
                }
            }

            return Page();
        }
        catch (Services.ApiException ex)
        {
            return await Fail(ex, redirect: false);
        }
        catch (HttpRequestException)
        {
            return Unreachable(redirect: false);
        }
    }

    public async Task<IActionResult> OnPostAsync(Guid? id)
    {
        if (InvalidForm(id) is IActionResult invalid)
        {
            return invalid;
        }

        var request = new AccountRequest(
            Input.Name.Trim(),
            Input.InitialBalance,
            Input.Type,
            Input.Currency.Trim());

        try
        {
            if (id is Guid accountId)
            {
                await Api.UpdateAccountAsync(accountId, request, HttpContext.RequestAborted);
            }
            else
            {
                await Api.CreateAccountAsync(request, HttpContext.RequestAborted);
            }

            return RedirectToPage();
        }
        catch (Services.ApiException ex)
        {
            return await Fail(ex, redirect: true, id);
        }
        catch (HttpRequestException)
        {
            return Unreachable(redirect: true, id);
        }
    }

    public async Task<IActionResult> OnPostDeleteAsync(Guid id)
    {
        try
        {
            await Api.DeleteAccountAsync(id, HttpContext.RequestAborted);
            return RedirectToPage();
        }
        catch (Services.ApiException ex)
        {
            return await Fail(ex, redirect: true);
        }
        catch (HttpRequestException)
        {
            return Unreachable(redirect: true);
        }
    }

    public class AccountInput
    {
        public string Name { get; set; } = "";

        [Display(Name = "Opening balance")]
        public decimal InitialBalance { get; set; }

        public AccountType Type { get; set; }

        public string Currency { get; set; } = "LKR";
    }
}

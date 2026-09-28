using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using PersonalFinance.Web.Models;

namespace PersonalFinance.Web.Pages;

[Authorize]
public class AccountsModel : FinancePageModel
{
    public AccountsModel(Services.ApiClient api) : base(api)
    {
    }

    public AccountResponse[] Accounts { get; private set; } = [];

    public bool HasAnyAccounts { get; private set; }

    public Guid? Id { get; private set; }

    [BindProperty(SupportsGet = true)]
    public string? Name { get; set; }

    [BindProperty(SupportsGet = true)]
    public AccountType? Type { get; set; }

    [BindProperty(SupportsGet = true)]
    public string? Currency { get; set; }

    [BindProperty]
    public AccountInput Input { get; set; } = new();

    public async Task<IActionResult> OnGetAsync(Guid? id)
    {
        LoadError();
        try
        {
            var accounts = (await Api.GetAccountsAsync(HttpContext.RequestAborted)).Accounts ?? [];
            HasAnyAccounts = accounts.Length > 0;
            Accounts = Filter(accounts);
            if (id is Guid accountId)
            {
                var account = accounts.FirstOrDefault(item => item.Id == accountId);
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

            return RedirectToPage(FilterRoute());
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
            return RedirectToPage(FilterRoute());
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

    private AccountResponse[] Filter(AccountResponse[] accounts)
    {
        IEnumerable<AccountResponse> matches = accounts;
        if (!string.IsNullOrWhiteSpace(Name))
        {
            var name = Name.Trim();
            matches = matches.Where(account => account.Name.Contains(name, StringComparison.OrdinalIgnoreCase));
        }

        if (Type is AccountType type)
        {
            matches = matches.Where(account => account.Type == type);
        }

        if (!string.IsNullOrWhiteSpace(Currency))
        {
            var currency = Currency.Trim();
            matches = matches.Where(account => account.Currency.Contains(currency, StringComparison.OrdinalIgnoreCase));
        }

        return matches.ToArray();
    }

    private RouteValueDictionary FilterRoute(Guid? id = null)
    {
        var route = new RouteValueDictionary();
        if (id is Guid accountId)
        {
            route["id"] = accountId;
        }

        if (!string.IsNullOrWhiteSpace(Name))
        {
            route["name"] = Name.Trim();
        }

        if (Type is AccountType type)
        {
            route["type"] = type;
        }

        if (!string.IsNullOrWhiteSpace(Currency))
        {
            route["currency"] = Currency.Trim();
        }

        return route;
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

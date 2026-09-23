using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using PersonalFinance.Web.Models;

namespace PersonalFinance.Web.Pages;

[Authorize]
public class TransactionsModel : FinancePageModel
{
    public TransactionsModel(Services.ApiClient api) : base(api)
    {
    }

    public AccountResponse[] Accounts { get; private set; } = [];

    public CategoryResponse[] Categories { get; private set; } = [];

    public TransactionResponse[] Transactions { get; private set; } = [];

    public Guid? Id { get; private set; }

    [BindProperty]
    public TransactionInput Input { get; set; } = new();

    public List<SelectListItem> AccountOptions => Accounts
        .Select(account => new SelectListItem($"{account.Name} ({account.Currency})", account.Id.ToString()))
        .ToList();

    public async Task<IActionResult> OnGetAsync(Guid? id)
    {
        LoadError();
        try
        {
            await LoadAsync();
            if (id is Guid transactionId)
            {
                var transaction = Transactions.FirstOrDefault(item => item.Id == transactionId);
                if (transaction is not null)
                {
                    Id = transaction.Id;
                    Input = new TransactionInput
                    {
                        AccountId = transaction.AccountId,
                        CategoryId = transaction.CategoryId,
                        Amount = transaction.Amount,
                        Type = transaction.Type,
                        Description = transaction.Description,
                        TransactionDate = transaction.TransactionDate.Date
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

        var description = string.IsNullOrWhiteSpace(Input.Description) ? null : Input.Description.Trim();
        var transactionDate = DateTime.SpecifyKind(Input.TransactionDate.Date, DateTimeKind.Utc);
        var request = new TransactionRequest(
            Input.AccountId,
            Input.CategoryId,
            Input.Amount,
            Input.Type,
            description,
            transactionDate);

        try
        {
            if (id is Guid transactionId)
            {
                await Api.UpdateTransactionAsync(transactionId, request, HttpContext.RequestAborted);
            }
            else
            {
                await Api.CreateTransactionAsync(request, HttpContext.RequestAborted);
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
            await Api.DeleteTransactionAsync(id, HttpContext.RequestAborted);
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

    public string AccountName(Guid accountId) =>
        Accounts.FirstOrDefault(account => account.Id == accountId)?.Name ?? "Unknown account";

    public string CategoryName(Guid categoryId) =>
        Categories.FirstOrDefault(category => category.Id == categoryId)?.Name ?? "Unknown category";

    public string Money(TransactionResponse transaction)
    {
        var amount = transaction.Amount.ToString("0.00");
        var currency = Accounts.FirstOrDefault(account => account.Id == transaction.AccountId)?.Currency;
        return string.IsNullOrEmpty(currency) ? amount : $"{amount} {currency}";
    }

    private async Task LoadAsync()
    {
        var cancellationToken = HttpContext.RequestAborted;
        Accounts = (await Api.GetAccountsAsync(cancellationToken)).Accounts ?? [];
        Categories = (await Api.GetCategoriesAsync(cancellationToken)).Categories ?? [];
        Transactions = (await Api.GetTransactionsAsync(cancellationToken)).TransactionResponses ?? [];
    }

    public class TransactionInput
    {
        [Display(Name = "Account")]
        public Guid AccountId { get; set; }

        [Display(Name = "Category")]
        public Guid CategoryId { get; set; }

        public decimal Amount { get; set; }

        public TransactionType Type { get; set; }

        public string? Description { get; set; }

        [Display(Name = "Date")]
        public DateTime TransactionDate { get; set; } = DateTime.Today;
    }
}

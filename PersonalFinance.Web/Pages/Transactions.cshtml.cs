using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Routing;
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

    public bool HasAnyTransactions { get; private set; }

    public Guid? Id { get; private set; }

    [BindProperty(SupportsGet = true)]
    public Guid? AccountId { get; set; }

    [BindProperty(SupportsGet = true)]
    public Guid? CategoryId { get; set; }

    [BindProperty(SupportsGet = true)]
    public TransactionType? Type { get; set; }

    [BindProperty(SupportsGet = true)]
    public string? Description { get; set; }

    [BindProperty(SupportsGet = true)]
    [DataType(DataType.Date)]
    public DateTime? From { get; set; }

    [BindProperty(SupportsGet = true)]
    [DataType(DataType.Date)]
    public DateTime? To { get; set; }

    [BindProperty]
    public TransactionInput Input { get; set; } = new();

    private TransactionResponse[] _transactions = [];

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
                var transaction = _transactions.FirstOrDefault(item => item.Id == transactionId);
                if (transaction is not null && !CanChange(transaction))
                {
                    SaveError("Transfer transactions can only be changed with their transfer.");
                    return RedirectToPage(FilterRoute());
                }

                if (transaction is not null)
                {
                    Id = transaction.Id;
                    Input = new TransactionInput
                    {
                        AccountId = transaction.AccountId,
                        CategoryId = transaction.CategoryId ?? Guid.Empty,
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
            await Api.DeleteTransactionAsync(id, HttpContext.RequestAborted);
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

    public bool CanChange(TransactionResponse transaction) =>
        transaction.Type is not (TransactionType.TransferIn or TransactionType.TransferOut);

    public string AccountName(Guid accountId) =>
        Accounts.FirstOrDefault(account => account.Id == accountId)?.Name ?? "Unknown account";

    public string CategoryName(Guid? categoryId) =>
        categoryId is Guid id
            ? Categories.FirstOrDefault(category => category.Id == id)?.Name ?? "Unknown category"
            : "";

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
        _transactions = (await Api.GetTransactionsAsync(cancellationToken)).TransactionResponses ?? [];
        HasAnyTransactions = _transactions.Length > 0;
        Transactions = Filter(_transactions);
    }

    private TransactionResponse[] Filter(TransactionResponse[] transactions)
    {
        IEnumerable<TransactionResponse> matches = transactions;
        if (AccountId is Guid accountId)
        {
            matches = matches.Where(transaction => transaction.AccountId == accountId);
        }

        if (CategoryId == Guid.Empty)
        {
            matches = matches.Where(transaction => transaction.CategoryId is null);
        }
        else if (CategoryId is Guid categoryId)
        {
            matches = matches.Where(transaction => transaction.CategoryId == categoryId);
        }

        if (Type is TransactionType type)
        {
            matches = matches.Where(transaction => transaction.Type == type);
        }

        if (!string.IsNullOrWhiteSpace(Description))
        {
            var description = Description.Trim();
            matches = matches.Where(transaction =>
                transaction.Description?.Contains(description, StringComparison.OrdinalIgnoreCase) == true);
        }

        if (From is DateTime from)
        {
            matches = matches.Where(transaction => transaction.TransactionDate.Date >= from.Date);
        }

        if (To is DateTime to)
        {
            matches = matches.Where(transaction => transaction.TransactionDate.Date <= to.Date);
        }

        return matches.ToArray();
    }

    private RouteValueDictionary FilterRoute()
    {
        var route = new RouteValueDictionary();
        if (AccountId is Guid accountId)
        {
            route["accountId"] = accountId;
        }

        if (CategoryId is Guid categoryId)
        {
            route["categoryId"] = categoryId;
        }

        if (Type is TransactionType type)
        {
            route["type"] = type;
        }

        if (!string.IsNullOrWhiteSpace(Description))
        {
            route["description"] = Description.Trim();
        }

        if (From is DateTime from)
        {
            route["from"] = from.ToString("yyyy-MM-dd");
        }

        if (To is DateTime to)
        {
            route["to"] = to.ToString("yyyy-MM-dd");
        }

        return route;
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

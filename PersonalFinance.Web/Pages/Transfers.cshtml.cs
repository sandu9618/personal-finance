using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Routing;
using PersonalFinance.Web.Models;

namespace PersonalFinance.Web.Pages;

[Authorize]
public class TransfersModel : FinancePageModel
{
    public TransfersModel(Services.ApiClient api) : base(api)
    {
    }

    public AccountResponse[] Accounts { get; private set; } = [];

    public TransferResponse[] Transfers { get; private set; } = [];

    public bool HasAnyTransfers { get; private set; }

    public Guid? Id { get; private set; }

    [BindProperty(SupportsGet = true)]
    [Display(Name = "From account")]
    public Guid? FromAccountId { get; set; }

    [BindProperty(SupportsGet = true)]
    [Display(Name = "To account")]
    public Guid? ToAccountId { get; set; }

    [BindProperty(SupportsGet = true)]
    public string? Description { get; set; }

    [BindProperty(SupportsGet = true)]
    [DataType(DataType.Date)]
    [Display(Name = "From date")]
    public DateTime? From { get; set; }

    [BindProperty(SupportsGet = true)]
    [DataType(DataType.Date)]
    [Display(Name = "To date")]
    public DateTime? To { get; set; }

    [BindProperty]
    public TransferInput Input { get; set; } = new();

    private TransferResponse[] _transfers = [];

    public List<SelectListItem> AccountOptions => Accounts
        .Select(account => new SelectListItem($"{account.Name} ({account.Currency})", account.Id.ToString()))
        .ToList();

    public async Task<IActionResult> OnGetAsync(Guid? id)
    {
        LoadError();
        try
        {
            await LoadAsync();
            if (id is Guid transferId)
            {
                var transfer = _transfers.FirstOrDefault(item => item.Id == transferId);
                if (transfer is not null)
                {
                    Id = transfer.Id;
                    Input = new TransferInput
                    {
                        FromAccountId = transfer.FromAccountId,
                        ToAccountId = transfer.ToAccountId,
                        Amount = transfer.Amount,
                        Description = transfer.Description,
                        TransferDate = transfer.TransferDate.Date
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
        var transferDate = DateTime.SpecifyKind(Input.TransferDate.Date, DateTimeKind.Utc);
        var request = new TransferRequest(
            Input.FromAccountId,
            Input.ToAccountId,
            Input.Amount,
            description,
            transferDate);

        try
        {
            if (id is Guid transferId)
            {
                await Api.UpdateTransferAsync(transferId, request, HttpContext.RequestAborted);
            }
            else
            {
                await Api.CreateTransferAsync(request, HttpContext.RequestAborted);
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
            await Api.DeleteTransferAsync(id, HttpContext.RequestAborted);
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

    public string AccountName(Guid accountId) =>
        Accounts.FirstOrDefault(account => account.Id == accountId)?.Name ?? "Unknown account";

    public string Money(TransferResponse transfer)
    {
        var amount = transfer.Amount.ToString("0.00");
        var currency = Accounts.FirstOrDefault(account => account.Id == transfer.FromAccountId)?.Currency;
        return string.IsNullOrEmpty(currency) ? amount : $"{amount} {currency}";
    }

    private async Task LoadAsync()
    {
        var cancellationToken = HttpContext.RequestAborted;
        Accounts = (await Api.GetAccountsAsync(cancellationToken)).Accounts ?? [];
        _transfers = (await Api.GetTransfersAsync(cancellationToken)).TransferResponses ?? [];
        HasAnyTransfers = _transfers.Length > 0;
        Transfers = Filter(_transfers);
    }

    private TransferResponse[] Filter(TransferResponse[] transfers)
    {
        IEnumerable<TransferResponse> matches = transfers;
        if (FromAccountId is Guid fromAccountId)
        {
            matches = matches.Where(transfer => transfer.FromAccountId == fromAccountId);
        }

        if (ToAccountId is Guid toAccountId)
        {
            matches = matches.Where(transfer => transfer.ToAccountId == toAccountId);
        }

        if (!string.IsNullOrWhiteSpace(Description))
        {
            var description = Description.Trim();
            matches = matches.Where(transfer =>
                transfer.Description?.Contains(description, StringComparison.OrdinalIgnoreCase) == true);
        }

        if (From is DateTime from)
        {
            matches = matches.Where(transfer => transfer.TransferDate.Date >= from.Date);
        }

        if (To is DateTime to)
        {
            matches = matches.Where(transfer => transfer.TransferDate.Date <= to.Date);
        }

        return matches.ToArray();
    }

    private RouteValueDictionary FilterRoute()
    {
        var route = new RouteValueDictionary();
        if (FromAccountId is Guid fromAccountId)
        {
            route["fromAccountId"] = fromAccountId;
        }

        if (ToAccountId is Guid toAccountId)
        {
            route["toAccountId"] = toAccountId;
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

    public class TransferInput
    {
        [Display(Name = "From account")]
        public Guid FromAccountId { get; set; }

        [Display(Name = "To account")]
        public Guid ToAccountId { get; set; }

        public decimal Amount { get; set; }

        public string? Description { get; set; }

        [Display(Name = "Date")]
        public DateTime TransferDate { get; set; } = DateTime.Today;
    }
}

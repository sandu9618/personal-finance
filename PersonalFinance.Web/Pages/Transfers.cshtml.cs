using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
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

    public Guid? Id { get; private set; }

    [BindProperty]
    public TransferInput Input { get; set; } = new();

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
                var transfer = Transfers.FirstOrDefault(item => item.Id == transferId);
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
            await Api.DeleteTransferAsync(id, HttpContext.RequestAborted);
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
        Transfers = (await Api.GetTransfersAsync(cancellationToken)).TransferResponses ?? [];
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

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PersonalFinance.Web.Models;

namespace PersonalFinance.Web.Pages;

[Authorize]
public class IndexModel : FinancePageModel
{
    public IndexModel(Services.ApiClient api) : base(api)
    {
    }

    public DashboardResponse Dashboard { get; private set; } = new(0, 0, 0, [], []);

    public async Task<IActionResult> OnGetAsync()
    {
        LoadError();
        try
        {
            Dashboard = await Api.GetDashboardAsync(HttpContext.RequestAborted);
            Dashboard = Dashboard with
            {
                ExpensesByCategory = Dashboard.ExpensesByCategory ?? [],
                RecentTransactions = Dashboard.RecentTransactions ?? []
            };
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
}

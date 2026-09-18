public class DashboardService : IDashboardService
{
  private readonly IDashboardRepository _dashboardRepository;

  public DashboardService(IDashboardRepository dashboardRepository)
  {
    _dashboardRepository = dashboardRepository;
  }
  public async Task<DashboardResponse> GetDashboardSummary(Guid userId, CancellationToken cancellationToken)
  {
    var totals = await _dashboardRepository.GetTotalsForUserAsync(userId, cancellationToken);
    var expensesByCategory = await _dashboardRepository.GetExpensesByCategoryAsync(userId, cancellationToken);
    var recentTransactions = await _dashboardRepository.GetRecentTransactionsForUserAsync(userId, cancellationToken);

    return new DashboardResponse(
      totals.Income,
      totals.Expenses,
      totals.Income - totals.Expenses,
      expensesByCategory,
      recentTransactions
    );
  }
}
public interface IDashboardRepository
{
  Task<(decimal Income, decimal Expenses)> GetTotalsForUserAsync(Guid userId, CancellationToken cancellationToken);
  Task<IReadOnlyList<ExpenseByCategoryDto>> GetExpensesByCategoryAsync(Guid userId, CancellationToken cancellationToken);
  Task<IReadOnlyList<TransactionResponse>> GetRecentTransactionsForUserAsync(Guid userId, CancellationToken cancellationToken);
}
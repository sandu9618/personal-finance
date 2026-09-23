using Microsoft.EntityFrameworkCore;
using PersonalFinance.Domain.Enums;

public class DashboardRepository : IDashboardRepository
{
  private readonly AppDbContext _dbContext;
  public DashboardRepository(AppDbContext dbContext)
  {
    _dbContext = dbContext;
  }
  public async Task<IReadOnlyList<ExpenseByCategoryDto>> GetExpensesByCategoryAsync(Guid userId, CancellationToken cancellationToken)
  {
    var rows = await _dbContext.Transactions
      .AsNoTracking()
      .Where(t => t.UserId == userId && t.Type == TransactionType.Expense)
      .GroupBy(t => new { t.CategoryId, t.Category.Name })
      .Select(g => new
      {
        g.Key.CategoryId,
        g.Key.Name,
        Total = g.Sum(t => t.Amount)
      })
      .OrderByDescending(x => x.Total)
      .ToListAsync(cancellationToken);

    return rows
      .Select(row => new ExpenseByCategoryDto(row.CategoryId, row.Name, row.Total))
      .ToList();
  }

  public async Task<IReadOnlyList<TransactionResponse>> GetRecentTransactionsForUserAsync(Guid userId, CancellationToken cancellationToken)
  {
    return await _dbContext.Transactions
      .AsNoTracking()
      .Where(t => t.UserId == userId)
      .OrderByDescending(t => t.TransactionDate)
      .ThenByDescending(t => t.CreatedAt)
      .Take(5)
      .Select(t => new TransactionResponse(
        t.Id,
        t.AccountId,
        t.CategoryId,
        t.Amount,
        t.Type,
        t.Description,
        t.TransactionDate
      ))
      .ToListAsync(cancellationToken);
  }

  public async Task<(decimal Income, decimal Expenses)> GetTotalsForUserAsync(Guid userId, CancellationToken cancellationToken)
  {
    var rows = await _dbContext.Transactions
    .AsNoTracking()
    .Where(t => t.UserId == userId)
    .GroupBy(t => t.Type)
    .Select(g => new { Type = g.Key, Total = g.Sum(t => t.Amount) })
    .ToListAsync(cancellationToken);

    var income = rows.FirstOrDefault(r => r.Type == TransactionType.Income)?.Total ?? 0m;
    var expenses = rows.FirstOrDefault(r => r.Type == TransactionType.Expense)?.Total ?? 0m;
    return (income, expenses);
  }
}
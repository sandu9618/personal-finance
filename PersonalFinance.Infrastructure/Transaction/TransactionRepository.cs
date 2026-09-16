using Microsoft.EntityFrameworkCore;
using PersonalFinance.Domain.Entities;

public class TransactionRepository : ITransactionRepository
{
  private readonly AppDbContext _db;

  public TransactionRepository(AppDbContext dbContext)
  {
    _db = dbContext;
  }
  public async Task AddAsync(Transaction transaction, CancellationToken cancellationToken)
  {
    await _db.Transactions.AddAsync(transaction, cancellationToken);
  }

  public async Task<IReadOnlyList<Transaction>> GetAllForUserAsync(Guid userId, CancellationToken cancellationToken)
  {
    return await _db.Transactions
      .AsNoTracking()
      .Where(t => t.UserId == userId)
      .ToListAsync(cancellationToken);
  }

  public Task<Transaction?> GetByIdForUserAsync(Guid transactionId, Guid userId, CancellationToken cancellationToken)
  {
    return _db.Transactions
      .FirstOrDefaultAsync(t => t.UserId == userId && t.Id == transactionId, cancellationToken);
  }

  public void Remove(Transaction transaction)
  {
    _db.Transactions.Remove(transaction);
  }

  public async Task SaveChangesAsync(CancellationToken cancellationToken)
  {
    await _db.SaveChangesAsync(cancellationToken);
  }
}
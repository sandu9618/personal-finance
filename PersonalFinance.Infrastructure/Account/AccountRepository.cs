using Microsoft.EntityFrameworkCore;
using PersonalFinance.Domain.Entities;

public class AccountRepository : IAccountRepository
{
  private readonly AppDbContext _db;

  public AccountRepository(AppDbContext dbContext)
  {
    _db = dbContext;
  }
  public async Task AddAsync(Account account, CancellationToken cancellationToken)
  {
    await _db.Accounts.AddAsync(account, cancellationToken);
  }

  public async Task<IReadOnlyList<Account>> GetAllForUserAsync(Guid userId, CancellationToken cancellationToken)
  {
    return await _db.Accounts
      .AsNoTracking()
      .Where(a => a.UserId == userId)
      .ToListAsync(cancellationToken);
  }

  public async Task<Account?> GetByIdForUserAsync(Guid accountId, Guid userId, CancellationToken cancellationToken)
  {
    return await _db.Accounts
      .FirstOrDefaultAsync(a => a.Id == accountId && a.UserId == userId, cancellationToken);
  }

  public void Remove(Account account)
  {
    _db.Accounts.Remove(account);
  }

  public async Task SaveChangesAsync(CancellationToken cancellationToken)
  {
    await _db.SaveChangesAsync(cancellationToken);
  }
}
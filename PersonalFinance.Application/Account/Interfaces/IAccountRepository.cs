using PersonalFinance.Domain.Entities;

public interface IAccountRepository
{
  Task AddAsync(Account account, CancellationToken cancellationToken);
  Task<Account?> GetByIdForUserAsync(Guid accountId, Guid userId, CancellationToken cancellationToken);
  Task<IReadOnlyList<Account>> GetAllForUserAsync(Guid userId, CancellationToken cancellationToken);
  Task SaveChangesAsync(CancellationToken cancellationToken);
  void Remove(Account account);
}
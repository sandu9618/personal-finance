using PersonalFinance.Domain.Entities;

public interface ITransactionRepository
{
  Task AddAsync(Transaction transaction, CancellationToken cancellationToken);
  Task<Transaction?> GetByIdForUserAsync(Guid transactionId, Guid userId, CancellationToken cancellationToken);
  Task<IReadOnlyList<Transaction>> GetAllForUserAsync(Guid userId, CancellationToken cancellationToken);
  Task SaveChangesAsync(CancellationToken cancellationToken);
  void Remove(Transaction transaction);
}
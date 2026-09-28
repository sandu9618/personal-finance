using PersonalFinance.Domain.Entities;

public interface ITransferRepository{
  Task AddAsync(Transfer request, CancellationToken cancellationToken);
  Task<IReadOnlyList<Transfer>> GetAllAsync(Guid userId, CancellationToken cancellationToken);
  Task<Transfer?> GetByIdAsync(Guid id, Guid userId, CancellationToken cancellationToken);
  Task Remove(Transfer transfer, CancellationToken cancellationToken);
  Task SaveChangesAsync(CancellationToken cancellationToken);
}
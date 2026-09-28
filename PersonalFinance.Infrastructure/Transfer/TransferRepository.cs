using Microsoft.EntityFrameworkCore;
using PersonalFinance.Domain.Entities;

public class TransferRepository : ITransferRepository
{
  private readonly AppDbContext _dbContext;

  public TransferRepository(AppDbContext appDbContext)
  {
    _dbContext = appDbContext;
  }
  public async Task AddAsync(Transfer request, CancellationToken cancellationToken)
  {
    await _dbContext.Transfers.AddAsync(request, cancellationToken);
  }

  public async Task Remove(Transfer transfer, CancellationToken cancellationToken)
  {
    _dbContext.Transfers.Remove(transfer);
  }

  public async Task<IReadOnlyList<Transfer>> GetAllAsync(Guid userId, CancellationToken cancellationToken)
  {
    return await _dbContext.Transfers 
      .AsNoTracking()
      .Where(t => t.UserId == userId)
      .ToListAsync(cancellationToken);
  }

  public async Task<Transfer?> GetByIdAsync(Guid id, Guid userId, CancellationToken cancellationToken)
  {
    return await _dbContext.Transfers
      .FirstOrDefaultAsync(t => t.UserId == userId && t.Id == id, cancellationToken);
  }

  public async Task SaveChangesAsync(CancellationToken cancellationToken)
  {
    await _dbContext.SaveChangesAsync(cancellationToken);
  }
}
public class UnitOfWork : IUnitOfWork
{
  private readonly AppDbContext _dbContext;

  public UnitOfWork(AppDbContext dbContext) => _dbContext = dbContext;
  public async Task ExecuteAsync(Func<CancellationToken, Task> action, CancellationToken cancellationToken)
  {
    await using var tx = await _dbContext.Database.BeginTransactionAsync(cancellationToken);
    try
    {
      await action(cancellationToken);
      await _dbContext.SaveChangesAsync(cancellationToken);
      await tx.CommitAsync(cancellationToken);
    }
    catch
    {
      await tx.RollbackAsync(cancellationToken);
      throw;
    }
  }
}
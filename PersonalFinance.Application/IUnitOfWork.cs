public interface IUnitOfWork
{
  Task ExecuteAsync(Func<CancellationToken, Task> action, CancellationToken cancellationToken);
}
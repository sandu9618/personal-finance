using PersonalFinance.Domain.Entities;

public interface ICategoryRepository
{
  Task AddAsync(Category category, CancellationToken cancellationToken);
  Task<Category?> GetByIdForUserAsync(Guid categoryId, Guid userId, CancellationToken cancellationToken);
  Task<IReadOnlyList<Category>> GetAllForUserAsync(Guid userId, CancellationToken cancellationToken);
  Task SaveChangesAsync(CancellationToken cancellationToken);
  void Remove(Category category);
}
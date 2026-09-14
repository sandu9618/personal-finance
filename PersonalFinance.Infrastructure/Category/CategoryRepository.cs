using Microsoft.EntityFrameworkCore;
using PersonalFinance.Domain.Entities;

public class CategoryRepository : ICategoryRepository
{
  private readonly AppDbContext _db;

  public CategoryRepository(AppDbContext dbContext)
  {
    _db = dbContext;
  }
  public async Task AddAsync(Category category, CancellationToken cancellationToken)
  {
    await _db.Categories.AddAsync(category, cancellationToken);
  }

  public async Task<IReadOnlyList<Category>> GetAllForUserAsync(Guid userId, CancellationToken cancellationToken)
  {
    return await _db.Categories
      .AsNoTracking()
      .Where(c => c.UserId == userId)
      .ToListAsync(cancellationToken);
  }

  public Task<Category?> GetByIdForUserAsync(Guid categoryId, Guid userId, CancellationToken cancellationToken)
  {
    return _db.Categories
      .AsNoTracking()
      .FirstOrDefaultAsync(c => c.UserId == userId && c.Id == categoryId, cancellationToken);
  }

  public void Remove(Category category)
  {
    _db.Categories.Remove(category);
  }

  public async Task SaveChangesAsync(CancellationToken cancellationToken)
  {
    await _db.SaveChangesAsync(cancellationToken);
  }
}
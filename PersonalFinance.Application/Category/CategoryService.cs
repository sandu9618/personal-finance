using PersonalFinance.Domain.Entities;

public class CategoryService : ICategoryService
{

  private readonly ICategoryRepository _categoryRepository;
  public CategoryService(ICategoryRepository categoryRepository)
  {
    _categoryRepository = categoryRepository;
  }
  public async Task<CategoryResponse> createCategoryAsync(CategoryRequest request, Guid userId, CancellationToken cancellationToken)
  {
    var category = new Category
    {
      Id = Guid.NewGuid(),
      Name = request.Name,
      Type = request.Type,
      UserId = userId,
      CreatedAt = DateTime.UtcNow
    };

    await _categoryRepository.AddAsync(category,cancellationToken);
    await _categoryRepository.SaveChangesAsync(cancellationToken);

    return new CategoryResponse(
      category.Id,
      category.Name,
      category.Type
    );

  }

  public async Task DeleteCategoryAsync(Guid categoryId, Guid userId, CancellationToken cancellationToken)
  {
    var category = await _categoryRepository.GetByIdForUserAsync(categoryId, userId, cancellationToken) ?? throw new KeyNotFoundException($"Category with ID {categoryId} not found");
    _categoryRepository.Remove(category);
    await _categoryRepository.SaveChangesAsync(cancellationToken);
  }

  public async Task<CategoryListResponse> GetCategoryAsync(Guid userId, CancellationToken cancellationToken)
  {
    var categories = await _categoryRepository.GetAllForUserAsync(userId, cancellationToken);
    var categoryResponses = categories.Select(category => new CategoryResponse(
      category.Id,
      category.Name,
      category.Type
    )).ToArray();

    return new CategoryListResponse(categoryResponses);
  }

  public async Task<CategoryResponse> GetCategoryByIdAsync(Guid categoryId, Guid userId, CancellationToken cancellationToken)
  {
    var category = await _categoryRepository.GetByIdForUserAsync(categoryId, userId, cancellationToken) ?? throw new KeyNotFoundException($"Category with ID {categoryId} not found.");
    return new CategoryResponse(
      category.Id,
      category.Name,
      category.Type
    );
  }

  public async Task<CategoryResponse> UpdateCategoryAsync(Guid categoryId, Guid userId, CategoryRequest request, CancellationToken cancellationToken)
  {
    var category = await _categoryRepository.GetByIdForUserAsync(categoryId, userId, cancellationToken) ?? throw new KeyNotFoundException($"Category with ID {categoryId} not found");
    category.Name = request.Name;
    category.Type = request.Type;
    await _categoryRepository.SaveChangesAsync(cancellationToken);
    return new CategoryResponse(
      category.Id,
      category.Name,
      category.Type
    );
  }
}
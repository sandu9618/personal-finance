public interface ICategoryService
{
  Task<CategoryResponse> createCategoryAsync(CategoryRequest request, Guid userId, CancellationToken cancellationToken);
  Task<CategoryListResponse> GetCategoryAsync(Guid userId, CancellationToken cancellationToken);
  Task<CategoryResponse> GetCategoryByIdAsync(Guid categoryId, Guid userId, CancellationToken cancellationToken);
  Task<CategoryResponse> UpdateCategoryAsync(Guid categoryId, Guid userId, CategoryRequest request, CancellationToken cancellationToken);
  Task DeleteCategoryAsync(Guid categoryId, Guid userId, CancellationToken cancellationToken);
}
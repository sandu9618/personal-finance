using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PersonalFinance.Api;

[ApiController]
[Authorize]
[Route("api/[controller]")]
public class CategoryController : ControllerBase
{
  private readonly ICategoryService _categoryService;

  public CategoryController(ICategoryService categoryService)
  {
    _categoryService = categoryService;
  }

  [HttpPost]
  public async Task<ActionResult<CategoryResponse>> CreateCategory([FromBody] CategoryRequest request, CancellationToken cancellationToken)
  {
    try
    {
      var userId = User.GetUserIdFromClaims();
      var response = await _categoryService.createCategoryAsync(request, userId, cancellationToken);
      return CreatedAtAction(nameof(GetCategoryById), new {categoryId = response.id}, response); 
    } 
    catch (InvalidOperationException ex)
    {
      return BadRequest(new {message = ex.Message});
    }
  }

  [HttpGet]
  public async Task<ActionResult<CategoryListResponse>> GetCategories(CancellationToken cancellationToken)
  {
    try
    {
      var userId = User.GetUserIdFromClaims();
      var response = await _categoryService.GetCategoryAsync(userId, cancellationToken);
      return Ok(response);
    } 
    catch (InvalidOperationException ex)
    {
      return BadRequest(new {message = ex.Message});
    }
  }

  [HttpGet("{categoryId}")]
  public async Task<ActionResult<CategoryResponse>> GetCategoryById(Guid categoryId, CancellationToken cancellationToken)
  {
    var userId = User.GetUserIdFromClaims();
    try
    {
      var response = await _categoryService.GetCategoryByIdAsync(categoryId, userId, cancellationToken);
      return Ok(response);
    }
    catch (KeyNotFoundException)
    {
      return NotFound();
    }
  }

  [HttpPut("{categoryId}")]
  public async Task<ActionResult<CategoryResponse>> UpdateCategory(Guid categoryId, [FromBody] CategoryRequest request, CancellationToken cancellationToken)
  {
    var userId = User.GetUserIdFromClaims();
    try
    {
      var response = await _categoryService.UpdateCategoryAsync(categoryId, userId, request, cancellationToken);
      return Ok(response);
    } 
    catch (KeyNotFoundException)
    {
      return NotFound();
    }
  }

  [HttpDelete("{categoryId}")]
  public async Task<ActionResult> DeleteCategory(Guid categoryId, CancellationToken cancellationToken)
  {
    var userId = User.GetUserIdFromClaims();
    try
    {
      await _categoryService.DeleteCategoryAsync(categoryId, userId, cancellationToken);
      return NoContent();
    } 
    catch (KeyNotFoundException)
    {
      return NotFound();
    }
  }

}
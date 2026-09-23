using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PersonalFinance.Web.Models;

namespace PersonalFinance.Web.Pages;

[Authorize]
public class CategoriesModel : FinancePageModel
{
    public CategoriesModel(Services.ApiClient api) : base(api)
    {
    }

    public CategoryResponse[] Categories { get; private set; } = [];

    public Guid? Id { get; private set; }

    [BindProperty]
    public CategoryInput Input { get; set; } = new();

    public async Task<IActionResult> OnGetAsync(Guid? id)
    {
        LoadError();
        try
        {
            Categories = (await Api.GetCategoriesAsync(HttpContext.RequestAborted)).Categories ?? [];
            if (id is Guid categoryId)
            {
                var category = Categories.FirstOrDefault(item => item.Id == categoryId);
                if (category is not null)
                {
                    Id = category.Id;
                    Input = new CategoryInput
                    {
                        Name = category.Name,
                        Type = category.Type
                    };
                }
            }

            return Page();
        }
        catch (Services.ApiException ex)
        {
            return await Fail(ex, redirect: false);
        }
        catch (HttpRequestException)
        {
            return Unreachable(redirect: false);
        }
    }

    public async Task<IActionResult> OnPostAsync(Guid? id)
    {
        if (InvalidForm(id) is IActionResult invalid)
        {
            return invalid;
        }

        var request = new CategoryRequest(Input.Name.Trim(), Input.Type);
        try
        {
            if (id is Guid categoryId)
            {
                await Api.UpdateCategoryAsync(categoryId, request, HttpContext.RequestAborted);
            }
            else
            {
                await Api.CreateCategoryAsync(request, HttpContext.RequestAborted);
            }

            return RedirectToPage();
        }
        catch (Services.ApiException ex)
        {
            return await Fail(ex, redirect: true, id);
        }
        catch (HttpRequestException)
        {
            return Unreachable(redirect: true, id);
        }
    }

    public async Task<IActionResult> OnPostDeleteAsync(Guid id)
    {
        try
        {
            await Api.DeleteCategoryAsync(id, HttpContext.RequestAborted);
            return RedirectToPage();
        }
        catch (Services.ApiException ex)
        {
            return await Fail(ex, redirect: true);
        }
        catch (HttpRequestException)
        {
            return Unreachable(redirect: true);
        }
    }

    public class CategoryInput
    {
        public string Name { get; set; } = "";

        public TransactionType Type { get; set; }
    }
}

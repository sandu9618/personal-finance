namespace PersonalFinance.Web.Models;

public record CategoryRequest(string Name, TransactionType Type);

public record CategoryResponse(Guid Id, string Name, TransactionType Type);

public record CategoryListResponse(CategoryResponse[] Categories);

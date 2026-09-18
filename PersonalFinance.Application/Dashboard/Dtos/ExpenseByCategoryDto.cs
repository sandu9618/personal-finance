public record ExpenseByCategoryDto (
  Guid CategoryId,
  string CategoryName,
  decimal Total
);
using PersonalFinance.Domain.Enums;

public record CategoryResponse (
  Guid id, 
  string Name,
  TransactionType Type
);
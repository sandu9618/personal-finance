using PersonalFinance.Domain.Enums;
public record CategoryRequest(
  string Name,
  TransactionType Type
);
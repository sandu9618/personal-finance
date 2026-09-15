using PersonalFinance.Domain.Enums;

public record TransactionResponse(
  Guid Id,
  Guid AccountId,
  Guid CategoryId,
  decimal Amount,
  TransactionType Type,
  string? Description,
  DateTime TransactionDate
);
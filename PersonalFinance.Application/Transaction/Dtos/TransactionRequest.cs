using PersonalFinance.Domain.Enums;

public record TransactionRequest(
  Guid AccountId,
  Guid CategoryId,
  decimal Amount,
  TransactionType Type,
  string? Description,
  DateTime TransactionDate
);
using PersonalFinance.Domain.Enums;

public record AccountResponse(
  Guid Id,
  string Name,
  decimal Balance,
  AccountType Type,
  string Currency
);
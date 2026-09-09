using PersonalFinance.Domain.Enums;

public record AccountRequest(
  string Name,
  decimal InitialBalance,
  AccountType Type,
  string Currency
);
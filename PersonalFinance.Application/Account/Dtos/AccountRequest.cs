using System.ComponentModel.DataAnnotations;
using PersonalFinance.Domain.Enums;

public record AccountRequest(
  [property: Required]
  [property: MaxLength(100)]
  string Name,
  decimal InitialBalance,
  [property: DefinedEnum]
  AccountType Type,
  [property: Required]
  [property: MaxLength(3)]
  string Currency
);

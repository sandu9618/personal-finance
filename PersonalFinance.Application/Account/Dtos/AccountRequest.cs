using System.ComponentModel.DataAnnotations;
using PersonalFinance.Domain.Enums;

public record AccountRequest(
  [Required]
  [MaxLength(100)]
  string Name,
  decimal InitialBalance,
  [DefinedEnum]
  AccountType Type,
  [Required]
  [MaxLength(3)]
  string Currency
);

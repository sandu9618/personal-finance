using System.ComponentModel.DataAnnotations;
using PersonalFinance.Domain.Enums;

public record CategoryRequest(
  [property: Required]
  [property: MaxLength(100)]
  string Name,
  [property: DefinedEnum]
  TransactionType Type
);

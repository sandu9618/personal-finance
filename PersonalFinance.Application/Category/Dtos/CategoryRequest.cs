using System.ComponentModel.DataAnnotations;
using PersonalFinance.Domain.Enums;

public record CategoryRequest(
  [Required]
  [MaxLength(100)]
  string Name,
  [DefinedEnum]
  TransactionType Type
);

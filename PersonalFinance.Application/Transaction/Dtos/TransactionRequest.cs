using System.ComponentModel.DataAnnotations;
using PersonalFinance.Domain.Enums;

public record TransactionRequest(
  Guid AccountId,
  Guid CategoryId,
  [property: Range(typeof(decimal), "0.01", "9999999999999999.99")]
  decimal Amount,
  [property: DefinedEnum]
  TransactionType Type,
  [property: MaxLength(500)]
  string? Description,
  DateTime TransactionDate
) : IValidatableObject
{
  public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
  {
    if (TransactionDate == DateTime.MinValue)
    {
      yield return new ValidationResult(
        "The TransactionDate field is required.",
        [nameof(TransactionDate)]);
    }
  }
}

using System.ComponentModel.DataAnnotations;

public record TransferRequest(
  Guid FromAccountId,
  Guid ToAccountId,
  [Range(typeof(decimal), "0.01", "9999999999999999.99")]
  decimal Amount,
  [MaxLength(500)]
  string? Description,
  DateTime TransferDate
) : IValidatableObject{
  public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
  {
    if (FromAccountId == ToAccountId)
    {
      yield return new ValidationResult("From and to account cannot be the same", new[] { nameof(FromAccountId), nameof(ToAccountId) });
    }
    if (Amount <= 0)
    {
      yield return new ValidationResult("Amount must be greater than 0", new[] { nameof(Amount) });
    }
    if (TransferDate == DateTime.MinValue)
    {
      yield return new ValidationResult("Transfer date is required", new[] { nameof(TransferDate) });
    }
  }
}
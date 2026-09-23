using System.ComponentModel.DataAnnotations;

[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field | AttributeTargets.Parameter)]
public sealed class DefinedEnumAttribute : ValidationAttribute
{
  protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
  {
    if (value is null)
    {
      return ValidationResult.Success;
    }

    var type = value.GetType();
    if (type.IsEnum && Enum.IsDefined(type, value))
    {
      return ValidationResult.Success;
    }

    var enumName = type.IsEnum ? type.Name : validationContext.DisplayName;
    return new ValidationResult(
      $"The value '{value}' is not a defined {enumName}.",
      validationContext.MemberName is null ? null : [validationContext.MemberName]);
  }
}

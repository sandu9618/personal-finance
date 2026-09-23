using System.ComponentModel.DataAnnotations;

public record LoginRequest(
  [property: Required]
  [property: EmailAddress]
  [property: MaxLength(256)]
  string Email,
  [property: Required]
  string Password
);

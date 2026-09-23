using System.ComponentModel.DataAnnotations;

public record RegisterRequest(
  [Required]
  [EmailAddress]
  [MaxLength(256)]
  string Email,
  [Required]
  string Password
);

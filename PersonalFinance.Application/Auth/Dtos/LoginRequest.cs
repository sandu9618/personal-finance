using System.ComponentModel.DataAnnotations;

public record LoginRequest(
  [Required]
  [EmailAddress]
  [MaxLength(256)]
  string Email,
  [Required]
  string Password
);

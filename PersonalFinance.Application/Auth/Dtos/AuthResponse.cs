public record AuthResponse(
  string Token,
  DateTime ExpiresAt,
  Guid UserId,
  string Email
);
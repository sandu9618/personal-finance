public record TransferResponse(
  Guid Id,
  Guid FromAccountId,
  Guid ToAccountId,
  decimal Amount,
  DateTime TransferDate,
  string? Description,
  Guid OutTransactionId,
  Guid InTransactionId
);
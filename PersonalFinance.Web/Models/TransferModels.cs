namespace PersonalFinance.Web.Models;

public record TransferRequest(
    Guid FromAccountId,
    Guid ToAccountId,
    decimal Amount,
    string? Description,
    DateTime TransferDate);

public record TransferResponse(
    Guid Id,
    Guid FromAccountId,
    Guid ToAccountId,
    decimal Amount,
    DateTime TransferDate,
    string? Description,
    Guid OutTransactionId,
    Guid InTransactionId);

public record TransferListResponse(TransferResponse[] TransferResponses);

namespace PersonalFinance.Web.Models;

public record TransactionRequest(
    Guid AccountId,
    Guid CategoryId,
    decimal Amount,
    TransactionType Type,
    string? Description,
    DateTime TransactionDate);

public record TransactionResponse(
    Guid Id,
    Guid AccountId,
    Guid CategoryId,
    decimal Amount,
    TransactionType Type,
    string? Description,
    DateTime TransactionDate);

public record TransactionListResponse(TransactionResponse[] TransactionResponses);

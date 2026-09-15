public interface ITransactionService
{
  Task<TransactionResponse> CreateTransactionAsync(TransactionRequest request, Guid userId, CancellationToken cancellationToken);
  Task<TransactionListResponse> GetTransactionAsync(Guid userId, CancellationToken cancellationToken);
  Task<TransactionResponse> GetTransactionByIdAsync(Guid transactionId, Guid userId, CancellationToken cancellationToken);
  Task<TransactionResponse> UpdateTransactionAsync(Guid transactionId, Guid userId, TransactionRequest request, CancellationToken cancellationToken);
  Task DeleteTransactionAsync(Guid transactionId, Guid userId, CancellationToken cancellationToken);
}
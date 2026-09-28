public interface ITransferService{
  Task<TransferResponse> CreateTransferAsync(TransferRequest request, Guid userId, CancellationToken cancellationToken);
  Task<TransferListResponse> GetAllTransfersAsync(Guid userId, CancellationToken cancellationToken);
  Task<TransferResponse> GetTransferAsync(Guid id, Guid userId, CancellationToken cancellationToken);
  Task<TransferResponse> UpdateTransferAsync(Guid id, TransferRequest request, Guid userId, CancellationToken cancellationToken);
  Task<TransferResponse> DeleteTransferAsync(Guid id, Guid userId, CancellationToken cancellationToken);
}
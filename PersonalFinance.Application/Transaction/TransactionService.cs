using PersonalFinance.Domain.Entities;

public class TransactionService : ITransactionService
{
  private readonly ITransactionRepository _transactionRepository;

  public TransactionService(ITransactionRepository transactionRepository)
  {
    _transactionRepository = transactionRepository;
  }
  public async Task<TransactionResponse> CreateTransactionAsync(TransactionRequest request, Guid userId, CancellationToken cancellationToken)
  {
    var transaction = new Transaction
    {
      Id = Guid.NewGuid(),
      AccountId = request.AccountId,
      CategoryId = request.CategoryId,
      Amount = request.Amount,
      Type = request.Type,
      Description = request.Description,
      TransactionDate = request.TransactionDate,
      CreatedAt = new DateTime()
    };

    await _transactionRepository.AddAsync(transaction, cancellationToken);
    await _transactionRepository.SaveChangesAsync(cancellationToken);

    return new TransactionResponse(
      transaction.Id,
      transaction.AccountId,
      transaction.CategoryId,
      transaction.Amount,
      transaction.Type,
      transaction.Description,
      transaction.TransactionDate
    );
  }

  public Task DeleteTransactionAsync(Guid transactionId, Guid userId, CancellationToken cancellationToken)
  {
    throw new NotImplementedException();
  }

  public async Task<TransactionListResponse> GetTransactionAsync(Guid userId, CancellationToken cancellationToken)
  {
    var transactions = await _transactionRepository.GetAllForUserAsync(userId, cancellationToken);
    var transactionResponses = transactions.Select(t => new TransactionResponse(
      t.Id,
      t.AccountId,
      t.CategoryId,
      t.Amount,
      t.Type,
      t.Description,
      t.TransactionDate
    )).ToArray();
    return new TransactionListResponse(transactionResponses);
  }

  public async Task<TransactionResponse> GetTransactionByIdAsync(Guid transactionId, Guid userId, CancellationToken cancellationToken)
  {
    var transaction = await _transactionRepository.GetByIdForUserAsync(transactionId, userId, cancellationToken) ?? throw new KeyNotFoundException();
    return new TransactionResponse(
      transaction.Id,
      transaction.AccountId,
      transaction.CategoryId,
      transaction.Amount,
      transaction.Type,
      transaction.Description,
      transaction.TransactionDate
    );
  }

  public async Task<TransactionResponse> UpdateTransactionAsync(Guid transactionId, Guid userId, TransactionRequest request, CancellationToken cancellationToken)
  {
    var transaction = await _transactionRepository.GetByIdForUserAsync(transactionId, userId, cancellationToken) ?? throw new KeyNotFoundException();
    transaction.AccountId = request.AccountId;
    transaction.CategoryId = request.CategoryId;
    transaction.Amount = request.Amount;
    transaction.Type = request.Type;
    transaction.Description = request.Description;
    transaction.TransactionDate = request.TransactionDate;

    await _transactionRepository.SaveChangesAsync(cancellationToken);
    return new TransactionResponse(
      transaction.Id,
      transaction.AccountId,
      transaction.CategoryId,
      transaction.Amount,
      transaction.Type,
      transaction.Description,
      transaction.TransactionDate
    );
  }
}
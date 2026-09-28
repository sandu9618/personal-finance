using PersonalFinance.Domain.Entities;
using PersonalFinance.Domain.Enums;

public class TransferService : ITransferService
{
  private readonly ITransferRepository _transferRepository;
  private readonly ITransactionRepository _transactionRepository;
  private readonly IAccountRepository _accountRepository;
  private readonly IUnitOfWork _unitOfWork;

  public TransferService (
    ITransferRepository transferRepository,
    ITransactionRepository transactionRepository,
    IAccountRepository accountRepository,
    IUnitOfWork unitOfWork
  )
  {
    _transferRepository = transferRepository;
    _transactionRepository = transactionRepository;
    _accountRepository = accountRepository;
    _unitOfWork = unitOfWork; 
  }
  public async Task<TransferResponse> CreateTransferAsync(TransferRequest request, Guid userId, CancellationToken cancellationToken)
  {
    if (request.Amount <= 0)
    {
      throw new InvalidOperationException("Amount must be greater than zero");
    }

    if (request.FromAccountId == request.ToAccountId)
    {
      throw new InvalidOperationException("Source and destination account must be different");
    }

    var fromAccount = await _accountRepository.GetByIdForUserAsync(request.FromAccountId, userId, cancellationToken) 
        ?? throw new KeyNotFoundException($"Account with ID ${request.FromAccountId} is not found");
    var toAccount = await _accountRepository.GetByIdForUserAsync(request.ToAccountId, userId, cancellationToken)
        ?? throw new KeyNotFoundException($"Account with ID ${request.FromAccountId} not found");

    if (fromAccount.Currency != toAccount.Currency)
    {
      //TODO: Handle the currency conversion when accounts have different currencies.
      throw new InvalidOperationException("Both account must use the same currency for now");
    }

    var now = DateTime.UtcNow;
    var transfer = new Transfer
    {
      Id = Guid.NewGuid(),
      UserId = userId,
      FromAccountId = fromAccount.Id,
      ToAccountId = toAccount.Id,
      Amount = request.Amount,
      Description = request.Description,
      TransferDate = request.TransferDate,
      CreatedAt = now
    };

    var outTransaction = new Transaction
    {
      Id = Guid.NewGuid(),
      UserId = userId,
      AccountId = fromAccount.Id,
      TransferId = transfer.Id,
      CategoryId = null,
      Amount = request.Amount,
      Type = TransactionType.TransferOut,
      Description = request.Description,
      TransactionDate = request.TransferDate,
      CreatedAt = now
    };

    var inTransaction = new Transaction
    {
      Id = Guid.NewGuid(),
      UserId = userId,
      AccountId = toAccount.Id,
      TransferId = transfer.Id,
      CategoryId = null,
      Amount = request.Amount,
      Type = TransactionType.TransferIn,
      Description = request.Description,
      TransactionDate = request.TransferDate,
      CreatedAt = now
    };

    var signedAmountOut = TransactionHelper.Signed(TransactionType.TransferOut, request.Amount);
    var signedAmountIn = TransactionHelper.Signed(TransactionType.TransferIn, request.Amount);

    await _unitOfWork.ExecuteAsync(async ct =>
    {
      await _transferRepository.AddAsync(transfer, ct);
      await _transactionRepository.AddAsync(inTransaction, ct);
      await _transactionRepository.AddAsync(outTransaction, ct);
      fromAccount.Balance += signedAmountOut;
      toAccount.Balance += signedAmountIn;
    }, cancellationToken);

    return new TransferResponse(
      transfer.Id,
      transfer.FromAccountId,
      transfer.ToAccountId,
      transfer.Amount,
      transfer.TransferDate,
      transfer.Description,
      outTransaction.Id,
      inTransaction.Id
    );
  }

  public async Task<TransferResponse> DeleteTransferAsync(Guid id, Guid userId, CancellationToken cancellationToken)
  {
    var transfer = await _transferRepository.GetByIdAsync(id, userId, cancellationToken)
      ?? throw new KeyNotFoundException($"Transfer with ID {id} not found");
    var (outTransaction, inTransaction) = await GetLegsAsync(transfer.Id, userId, cancellationToken);
    var fromAccount = await _accountRepository.GetByIdForUserAsync(transfer.FromAccountId, userId, cancellationToken)
      ?? throw new KeyNotFoundException($"Account with ID {transfer.FromAccountId} not found");
    var toAccount = await _accountRepository.GetByIdForUserAsync(transfer.ToAccountId, userId, cancellationToken)
      ?? throw new KeyNotFoundException($"Account with ID {transfer.ToAccountId} not found");

    var response = ToResponse(transfer, outTransaction.Id, inTransaction.Id);
    var signedAmountOut = TransactionHelper.Signed(TransactionType.TransferOut, transfer.Amount);
    var signedAmountIn = TransactionHelper.Signed(TransactionType.TransferIn, transfer.Amount);

    await _unitOfWork.ExecuteAsync(async ct =>
    {
      fromAccount.Balance -= signedAmountOut;
      toAccount.Balance -= signedAmountIn;
      _transactionRepository.Remove(outTransaction);
      _transactionRepository.Remove(inTransaction);
      await _transferRepository.Remove(transfer, ct);
    }, cancellationToken);

    return response;
  }

  public async Task<TransferListResponse> GetAllTransfersAsync(Guid userId, CancellationToken cancellationToken)
  {
    var transfers = await _transferRepository.GetAllAsync(userId, cancellationToken);
    var transactions = await _transactionRepository.GetByTransferIdsForUserAsync(
      transfers.Select(t => t.Id).ToArray(),
      userId,
      cancellationToken);
    var legsByTransfer = transactions
      .GroupBy(t => t.TransferId!.Value)
      .ToDictionary(g => g.Key, g => g.ToArray());

    var responses = transfers.Select(transfer =>
    {
      var (outTransaction, inTransaction) = Legs(transfer.Id, legsByTransfer.GetValueOrDefault(transfer.Id) ?? []);
      return ToResponse(transfer, outTransaction.Id, inTransaction.Id);
    }).ToArray();

    return new TransferListResponse(responses);
  }

  public async Task<TransferResponse> GetTransferAsync(Guid id, Guid userId, CancellationToken cancellationToken)
  {
    var transfer = await _transferRepository.GetByIdAsync(id, userId, cancellationToken)
      ?? throw new KeyNotFoundException($"Transfer with ID {id} not found");
    var (outTransaction, inTransaction) = await GetLegsAsync(transfer.Id, userId, cancellationToken);
    return ToResponse(transfer, outTransaction.Id, inTransaction.Id);
  }

  public async Task<TransferResponse> UpdateTransferAsync(Guid id, TransferRequest request, Guid userId, CancellationToken cancellationToken)
  {
    var transfer = await _transferRepository.GetByIdAsync(id, userId, cancellationToken)
      ?? throw new KeyNotFoundException($"Transfer with ID {id} not found");

    if (request.Amount <= 0)
    {
      throw new InvalidOperationException("Amount must be greater than zero");
    }

    if (request.FromAccountId == request.ToAccountId)
    {
      throw new InvalidOperationException("Source and destination account must be different");
    }

    var (outTransaction, inTransaction) = await GetLegsAsync(transfer.Id, userId, cancellationToken);
    var oldFromAccount = await _accountRepository.GetByIdForUserAsync(transfer.FromAccountId, userId, cancellationToken)
      ?? throw new KeyNotFoundException($"Account with ID {transfer.FromAccountId} not found");
    var oldToAccount = await _accountRepository.GetByIdForUserAsync(transfer.ToAccountId, userId, cancellationToken)
      ?? throw new KeyNotFoundException($"Account with ID {transfer.ToAccountId} not found");
    var newFromAccount = transfer.FromAccountId == request.FromAccountId
      ? oldFromAccount
      : await _accountRepository.GetByIdForUserAsync(request.FromAccountId, userId, cancellationToken)
        ?? throw new KeyNotFoundException($"Account with ID {request.FromAccountId} not found");
    var newToAccount = transfer.ToAccountId == request.ToAccountId
      ? oldToAccount
      : await _accountRepository.GetByIdForUserAsync(request.ToAccountId, userId, cancellationToken)
        ?? throw new KeyNotFoundException($"Account with ID {request.ToAccountId} not found");

    if (newFromAccount.Currency != newToAccount.Currency)
    {
      throw new InvalidOperationException("Both account must use the same currency for now");
    }

    var oldSignedAmountOut = TransactionHelper.Signed(TransactionType.TransferOut, transfer.Amount);
    var oldSignedAmountIn = TransactionHelper.Signed(TransactionType.TransferIn, transfer.Amount);
    var newSignedAmountOut = TransactionHelper.Signed(TransactionType.TransferOut, request.Amount);
    var newSignedAmountIn = TransactionHelper.Signed(TransactionType.TransferIn, request.Amount);

    await _unitOfWork.ExecuteAsync(async ct =>
    {
      oldFromAccount.Balance -= oldSignedAmountOut;
      oldToAccount.Balance -= oldSignedAmountIn;

      transfer.FromAccountId = request.FromAccountId;
      transfer.ToAccountId = request.ToAccountId;
      transfer.Amount = request.Amount;
      transfer.Description = request.Description;
      transfer.TransferDate = request.TransferDate;

      outTransaction.AccountId = request.FromAccountId;
      outTransaction.Amount = request.Amount;
      outTransaction.Description = request.Description;
      outTransaction.TransactionDate = request.TransferDate;

      inTransaction.AccountId = request.ToAccountId;
      inTransaction.Amount = request.Amount;
      inTransaction.Description = request.Description;
      inTransaction.TransactionDate = request.TransferDate;

      newFromAccount.Balance += newSignedAmountOut;
      newToAccount.Balance += newSignedAmountIn;
    }, cancellationToken);

    return ToResponse(transfer, outTransaction.Id, inTransaction.Id);
  }

  private async Task<(Transaction Out, Transaction In)> GetLegsAsync(Guid transferId, Guid userId, CancellationToken cancellationToken)
  {
    var transactions = await _transactionRepository.GetByTransferIdsForUserAsync([transferId], userId, cancellationToken);
    return Legs(transferId, transactions);
  }

  private static (Transaction Out, Transaction In) Legs(Guid transferId, IReadOnlyList<Transaction> transactions)
  {
    var outTransaction = transactions.SingleOrDefault(t => t.Type == TransactionType.TransferOut);
    var inTransaction = transactions.SingleOrDefault(t => t.Type == TransactionType.TransferIn);
    if (outTransaction is null || inTransaction is null)
    {
      throw new InvalidOperationException($"Transfer with ID {transferId} is missing a linked transaction");
    }

    return (outTransaction, inTransaction);
  }

  private static TransferResponse ToResponse(Transfer transfer, Guid outTransactionId, Guid inTransactionId)
  {
    return new TransferResponse(
      transfer.Id,
      transfer.FromAccountId,
      transfer.ToAccountId,
      transfer.Amount,
      transfer.TransferDate,
      transfer.Description,
      outTransactionId,
      inTransactionId
    );
  }
}
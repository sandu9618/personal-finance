using PersonalFinance.Domain.Entities;
using PersonalFinance.Domain.Enums;

public class TransactionService : ITransactionService
{
  private readonly ITransactionRepository _transactionRepository;
  private readonly IAccountRepository _accountRepository;
  private readonly ICategoryRepository _categoryRepository;
  private readonly IUnitOfWork _unitOfWork;

  public TransactionService(
      ITransactionRepository transactionRepository,
      IAccountRepository accountRepository,
      ICategoryRepository categoryRepository,
      IUnitOfWork unitOfWork
    )
  {
    _transactionRepository = transactionRepository;
    _accountRepository = accountRepository;
    _categoryRepository = categoryRepository;
    _unitOfWork = unitOfWork;
  }
  public async Task<TransactionResponse> CreateTransactionAsync(TransactionRequest request, Guid userId, CancellationToken cancellationToken)
  {
    if (request.Amount <= 0)
    {
      throw new InvalidOperationException("Amount must be greater than zero.");
    }

    var account = await _accountRepository.GetByIdForUserAsync(request.AccountId, userId, cancellationToken) ?? throw new InvalidOperationException($"Account with ID {request.AccountId} not found");
    var category = await _categoryRepository.GetByIdForUserAsync(request.CategoryId, userId, cancellationToken) ?? throw new InvalidOperationException($"Category with ID {request.CategoryId} not found");

    if (category.Type != request.Type)
    {
      throw new InvalidOperationException("Transaction type and category type should be matched");
    }

    var transaction = new Transaction
    {
      Id = Guid.NewGuid(),
      UserId = userId,
      AccountId = request.AccountId,
      CategoryId = request.CategoryId,
      Amount = request.Amount,
      Type = request.Type,
      Description = request.Description,
      TransactionDate = request.TransactionDate,
      CreatedAt = new DateTime()
    };

    var signedAmount = Signed(request.Type, request.Amount);

    await _unitOfWork.ExecuteAsync(async ct =>
    {
      await _transactionRepository.AddAsync(transaction, cancellationToken);
      account.Balance += signedAmount;
    }, cancellationToken);

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

  public async Task DeleteTransactionAsync(Guid transactionId, Guid userId, CancellationToken cancellationToken)
  {
    var transaction = await _transactionRepository.GetByIdForUserAsync(transactionId, userId, cancellationToken) ?? throw new KeyNotFoundException();
    var signedAmount = Signed(transaction.Type, transaction.Amount);
    var account = await _accountRepository.GetByIdForUserAsync(transaction.AccountId, userId, cancellationToken) ?? throw new InvalidOperationException($"Account with ID {transaction.AccountId} not found");

    await _unitOfWork.ExecuteAsync(async ct =>
    {
      account.Balance -= signedAmount;
      _transactionRepository.Remove(transaction);
    }, cancellationToken);
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
    if (request.Amount <= 0)
    {
      throw new InvalidOperationException("Amount must be greater than zero");
    }

    var transaction = await _transactionRepository.GetByIdForUserAsync(transactionId, userId, cancellationToken) ?? throw new KeyNotFoundException();

    var oldSignedAmount = Signed(transaction.Type, transaction.Amount);

    var oldAccount = await _accountRepository.GetByIdForUserAsync(transaction.AccountId, userId, cancellationToken) ?? throw new InvalidOperationException($"Account not found with ID {transaction.AccountId}");
    var newAccount = transaction.AccountId == request.AccountId 
                      ? oldAccount 
                      : await _accountRepository.GetByIdForUserAsync(request.AccountId, userId, cancellationToken)
                      ?? throw new InvalidOperationException($"Account not found with ID {request.AccountId}");
    var category = await _categoryRepository.GetByIdForUserAsync(request.CategoryId, userId, cancellationToken) ?? throw new InvalidOperationException($"Category with ID {request.CategoryId} not found.");

    var newSignedAmount = Signed(request.Type, request.Amount);

    if (request.Type != category.Type)
    {
      throw new InvalidOperationException("Transaction type and category type should be matched");
    } 

    await _unitOfWork.ExecuteAsync(async ct =>
    {
      oldAccount.Balance -= oldSignedAmount;

      transaction.AccountId = request.AccountId;
      transaction.Amount = request.Amount;
      transaction.CategoryId = request.CategoryId;
      transaction.Description = request.Description;
      transaction.Type = request.Type;
      transaction.TransactionDate = request.TransactionDate;

      newAccount.Balance += newSignedAmount;
    }, cancellationToken);

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

  private decimal Signed(TransactionType type, decimal amount) => 
    type == TransactionType.Income ? amount : -amount;
}
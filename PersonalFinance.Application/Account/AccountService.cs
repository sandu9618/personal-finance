using PersonalFinance.Domain.Entities;

public class AccountService : IAccountService
{

  private readonly IAccountRepository _accountRepository;

  public AccountService(IAccountRepository accountRepository)
  {
    _accountRepository = accountRepository;
  }
  public async Task<AccountResponse> CreateAccountAsync(AccountRequest request, Guid userId, CancellationToken cancellationToken)
  {
    var account = new Account
    {
      Id = Guid.NewGuid(),
      Name = request.Name,
      Balance = request.InitialBalance,
      Type = request.Type,
      Currency = request.Currency,
      CreatedAt = DateTime.UtcNow,
      UserId = userId
    };

    await _accountRepository.AddAsync(account, cancellationToken);
    await _accountRepository.SaveChangesAsync(cancellationToken);

    return new AccountResponse(
      account.Id,
      account.Name,
      account.Balance,
      account.Type,
      account.Currency
    );
  }

  public async Task DeleteAccountAsync(Guid accountId, Guid userId, CancellationToken cancellationToken)
  {
    var account = await _accountRepository.GetByIdForUserAsync(accountId, userId, cancellationToken) ?? throw new KeyNotFoundException($"Account with ID {accountId} not found.");
    _accountRepository.Remove(account);
    await _accountRepository.SaveChangesAsync(cancellationToken);
  }

  public async Task<AccountResponse> GetAccountByIdAsync(Guid accountId, Guid userId, CancellationToken cancellationToken)
  {
    var account = await _accountRepository.GetByIdForUserAsync(accountId, userId, cancellationToken) ?? throw new KeyNotFoundException($"Account with ID {accountId} not found for user {userId}.");
    return new AccountResponse(
      account.Id,
      account.Name,
      account.Balance,
      account.Type,
      account.Currency
    );
  }

  public async Task<AccountListResponse> GetAccountsAsync(Guid userId, CancellationToken cancellationToken)
  {
    var accounts = await _accountRepository.GetAllForUserAsync(userId, cancellationToken);
    var accountResponses = accounts.Select(account => new AccountResponse(
      account.Id,
      account.Name,
      account.Balance,
      account.Type,
      account.Currency
    )).ToArray();

    return new AccountListResponse(accountResponses);
  }

  public async Task<AccountResponse> UpdateAccountAsync(Guid accountId, AccountRequest request, CancellationToken cancellationToken)
  {
    var account = await _accountRepository.GetByIdForUserAsync(accountId, Guid.Empty, cancellationToken) ?? throw new KeyNotFoundException($"Account with ID {accountId} not found.");
    account.Name = request.Name;
    account.Balance = request.InitialBalance;
    account.Type = request.Type;
    account.Currency = request.Currency;
    await _accountRepository.SaveChangesAsync(cancellationToken);
    return new AccountResponse(
      account.Id,
      account.Name,
      account.Balance,
      account.Type,
      account.Currency
    );
  }
}
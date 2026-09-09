public interface IAccountService
{
  Task<AccountResponse> CreateAccountAsync(AccountRequest request, Guid userId, CancellationToken cancellationToken);
  Task<AccountListResponse> GetAccountsAsync(Guid userId, CancellationToken cancellationToken);
  Task<AccountResponse> GetAccountByIdAsync(Guid accountId, Guid userId, CancellationToken cancellationToken);
  Task<AccountResponse> UpdateAccountAsync(Guid accountId, AccountRequest request, CancellationToken cancellationToken);
  Task DeleteAccountAsync(Guid accountId, Guid userId, CancellationToken cancellationToken);
}
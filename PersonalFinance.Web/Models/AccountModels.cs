namespace PersonalFinance.Web.Models;

public record AccountRequest(
    string Name,
    decimal InitialBalance,
    AccountType Type,
    string Currency);

public record AccountResponse(
    Guid Id,
    string Name,
    decimal Balance,
    AccountType Type,
    string Currency);

public record AccountListResponse(AccountResponse[] Accounts);

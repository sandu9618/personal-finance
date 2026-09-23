using Moq;
using PersonalFinance.Domain.Entities;
using PersonalFinance.Domain.Enums;
using Renci.SshNet.Security;

public class TransactionServiceTests
{
  private readonly Guid _userId = Guid.NewGuid();
  private readonly Mock<ITransactionRepository> _transactions = new();
  private readonly Mock<IAccountRepository> _accounts = new();
  private readonly Mock<ICategoryRepository> _categories = new();
  private readonly Mock<IUnitOfWork> _uow = new();
  private readonly TransactionService _sut;

  public TransactionServiceTests()
  {
    _uow.Setup(x => x.ExecuteAsync(
        It.IsAny<Func<CancellationToken, Task>>(),
        It.IsAny<CancellationToken>()
    ))
    .Returns((Func<CancellationToken,Task> action, CancellationToken ct) => action(ct));
    _sut = new TransactionService(
      _transactions.Object,
      _accounts.Object,
      _categories.Object,
      _uow.Object
    );
  }

  private Account Cash(decimal balance) => new()
  {
    Id = Guid.NewGuid(),
    UserId = _userId,
    Name = "Cash",
    Type = AccountType.Cash,
    Balance = balance,
    Currency = "LKR"
  };
  private Account Bank(decimal balance) => new()
  {
    Id = Guid.NewGuid(),
    UserId = _userId,
    Name = "Bank",
    Type = AccountType.Bank,
    Balance = balance,
    Currency = "LKR"
  };

  private Category Cat(TransactionType type, string name = "Salary") => new()
  {
    Id = Guid.NewGuid(),
    UserId = _userId,
    Name = name,
    Type = type
  };

  private static TransactionRequest Req(
    Guid accountId, Guid categoryId, decimal amount, TransactionType type
    ) => new(accountId, categoryId, amount, type, "test", DateTime.UtcNow);

  [Fact]
  public async Task Create_income_adds_to_account_balance()
  {
    var account = Cash(100000);
    var category = Cat(TransactionType.Income, "Salary");
    _accounts
      .Setup(x => x.GetByIdForUserAsync(account.Id, _userId, It.IsAny<CancellationToken>()))
      .ReturnsAsync(account);

    _categories
      .Setup(x => x.GetByIdForUserAsync(category.Id, _userId, It.IsAny<CancellationToken>()))
      .ReturnsAsync(category);
    
    _transactions
      .Setup(x => x.AddAsync(It.IsAny<Transaction>(), It.IsAny<CancellationToken>()))
      .Returns(Task.CompletedTask);

    var request = Req(account.Id, category.Id, 50000, TransactionType.Income);

    await _sut.CreateTransactionAsync(request, _userId, CancellationToken.None);

    Assert.Equal(150000, account.Balance);
    _transactions.Verify(x => x.AddAsync(It.IsAny<Transaction>(), It.IsAny<CancellationToken>()), Times.Once);

  }

  [Fact]
  public async Task Create_expense_reduces_account_balance()
  {
    var account = Cash(100000);
    var category = Cat(TransactionType.Expense, "Food");

    _accounts
      .Setup(x => x.GetByIdForUserAsync(account.Id, _userId, It.IsAny<CancellationToken>()))
      .ReturnsAsync(account);

    _categories
      .Setup(x => x.GetByIdForUserAsync(category.Id, _userId, It.IsAny<CancellationToken>()))
      .ReturnsAsync(category);
    
    _transactions
      .Setup(x => x.AddAsync(It.IsAny<Transaction>(), It.IsAny<CancellationToken>()))
      .Returns(Task.CompletedTask);

    var request = Req(account.Id, category.Id, 10000, TransactionType.Expense);

    await _sut.CreateTransactionAsync(request, _userId, CancellationToken.None);

    Assert.Equal(90000, account.Balance);
    _transactions.Verify(x => x.AddAsync(It.IsAny<Transaction>(), It.IsAny<CancellationToken>()), Times.Once);
  }

  [Fact]
  public async Task Create_rejects_zero_amount()
  {
    var request = Req(Guid.NewGuid(), Guid.NewGuid(), 0, TransactionType.Income);

    await Assert.ThrowsAsync<InvalidOperationException>(
      () => _sut.CreateTransactionAsync(request, _userId, CancellationToken.None)
    );

    _uow.Verify(
      x => x.ExecuteAsync(It.IsAny<Func<CancellationToken, Task>>(), It.IsAny<CancellationToken>()),
      Times.Never
    );
  }

  [Fact]
  public async Task Create_throws_when_account_not_found_for_user()
  {
    var accountId = Guid.NewGuid();
    var category = Cat(TransactionType.Income);

    _accounts
      .Setup(x => x.GetByIdForUserAsync(accountId, _userId, It.IsAny<CancellationToken>()))
      .ReturnsAsync((Account?)null);

    _categories
      .Setup(x => x.GetByIdForUserAsync(category.Id, _userId, It.IsAny<CancellationToken>()))
      .ReturnsAsync(category);

    var request = Req(accountId, category.Id, 50000, TransactionType.Income);

    await Assert.ThrowsAsync<KeyNotFoundException>(
      () => _sut.CreateTransactionAsync(request, _userId, CancellationToken.None)
    );

    _uow.Verify(
      x => x.ExecuteAsync(It.IsAny<Func<CancellationToken, Task>>(), It.IsAny<CancellationToken>()),
      Times.Never
    );
  }

  [Fact]
  public async Task Create_throws_when_category_type_mismatch()
  {
    var account = Cash(100000);
    var category = Cat(TransactionType.Income);

    _accounts
      .Setup(x => x.GetByIdForUserAsync(account.Id, _userId, It.IsAny<CancellationToken>()))
      .ReturnsAsync(account);

    _categories
      .Setup(x => x.GetByIdForUserAsync(category.Id, _userId, It.IsAny<CancellationToken>()))
      .ReturnsAsync(category);
    
    _transactions
      .Setup(x => x.AddAsync(It.IsAny<Transaction>(), It.IsAny<CancellationToken>()))
      .Returns(Task.CompletedTask);

    var request = Req(account.Id, category.Id, 10000, TransactionType.Expense);

    await Assert.ThrowsAsync<InvalidOperationException>(
      () => _sut.CreateTransactionAsync(request, _userId, CancellationToken.None)
    );

    _uow.Verify(
      x => x.ExecuteAsync(It.IsAny<Func<CancellationToken, Task>>(), It.IsAny<CancellationToken>()),
      Times.Never
    );
  }

  [Fact]
  public async Task Delete_expense_restores_balance()
  {
      var account = Cash(90000);
      var transaction = new Transaction
      {
          Id = Guid.NewGuid(),
          UserId = _userId,
          AccountId = account.Id,
          Amount = 10000,
          Type = TransactionType.Expense
      };

      _transactions
          .Setup(x => x.GetByIdForUserAsync(transaction.Id, _userId, It.IsAny<CancellationToken>()))
          .ReturnsAsync(transaction);

      _accounts
          .Setup(x => x.GetByIdForUserAsync(account.Id, _userId, It.IsAny<CancellationToken>()))
          .ReturnsAsync(account);

      await _sut.DeleteTransactionAsync(transaction.Id, _userId, CancellationToken.None);

      Assert.Equal(100000, account.Balance);
      _transactions.Verify(x => x.Remove(transaction), Times.Once);
  }

  [Fact]
  public async Task Update_amount_on_same_account_updates_account_balance()
  {
    var account = Cash(90000);
    var transaction = new Transaction
    {
      Id = Guid.NewGuid(),
      UserId = _userId,
      AccountId = account.Id,
      Amount = 10000,
      Type = TransactionType.Expense
    };
    var category = Cat(TransactionType.Expense, "Food");
    _accounts
      .Setup(x => x.GetByIdForUserAsync(account.Id, _userId, CancellationToken.None))
      .ReturnsAsync(account);

    _categories
      .Setup(x => x.GetByIdForUserAsync(category.Id, _userId, CancellationToken.None))
      .ReturnsAsync(category);

    _transactions
      .Setup(x => x.GetByIdForUserAsync(transaction.Id, _userId, CancellationToken.None))
      .ReturnsAsync(transaction);

    var request = Req(account.Id, category.Id, 20000, TransactionType.Expense);

    await _sut.UpdateTransactionAsync(transaction.Id, _userId, request, CancellationToken.None);

    Assert.Equal(80000, account.Balance);
  }

  [Fact]
  public async Task update_account_handles_the_balance()
  {
    var cashAccount = Cash(90000);
    var bankAccount = Bank(0);
    var category = Cat(TransactionType.Expense, "Food");
    var transaction = new Transaction
    {
      Id = Guid.NewGuid(),
      UserId = _userId,
      AccountId = cashAccount.Id,
      Amount = 10000,
      Type = TransactionType.Expense
    };

    _accounts
      .Setup(x => x.GetByIdForUserAsync(cashAccount.Id, _userId, CancellationToken.None))
      .ReturnsAsync(cashAccount);

    _accounts
      .Setup(x => x.GetByIdForUserAsync(bankAccount.Id, _userId, CancellationToken.None))
      .ReturnsAsync(bankAccount);

    _categories
      .Setup(x => x.GetByIdForUserAsync(category.Id, _userId, CancellationToken.None))
      .ReturnsAsync(category);

    _transactions
      .Setup(x => x.GetByIdForUserAsync(transaction.Id, _userId, CancellationToken.None))
      .ReturnsAsync(transaction);

    var request = Req(bankAccount.Id, category.Id, 10000, TransactionType.Expense);

    await _sut.UpdateTransactionAsync(transaction.Id, _userId, request, CancellationToken.None);

    Assert.Equal(100000, cashAccount.Balance);
    Assert.Equal(-10000, bankAccount.Balance);
  }
}
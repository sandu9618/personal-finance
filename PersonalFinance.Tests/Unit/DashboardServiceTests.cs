using Moq;

public class DashboardServiceTests
{
    [Fact]
    public async Task Balance_is_income_minus_expenses()
    {
        var repo = new Mock<IDashboardRepository>();
        var userId = Guid.NewGuid();

        repo.Setup(x => x.GetTotalsForUserAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((50000m, 10000m));
        repo.Setup(x => x.GetExpensesByCategoryAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Array.Empty<ExpenseByCategoryDto>());
        repo.Setup(x => x.GetRecentTransactionsForUserAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Array.Empty<TransactionResponse>());

        var sut = new DashboardService(repo.Object);

        var result = await sut.GetDashboardSummary(userId, CancellationToken.None);

        Assert.Equal(50000, result.TotalIncome);
        Assert.Equal(10000, result.TotalExpense);
        Assert.Equal(40000, result.Balanace);
    }

    [Fact]
    public async Task Empty_totals_are_zeros()
    {
        var repo = new Mock<IDashboardRepository>();
        var userId = Guid.NewGuid();

        repo.Setup(x => x.GetTotalsForUserAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((0m, 0m));
        repo.Setup(x => x.GetExpensesByCategoryAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Array.Empty<ExpenseByCategoryDto>());
        repo.Setup(x => x.GetRecentTransactionsForUserAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Array.Empty<TransactionResponse>());

        var sut = new DashboardService(repo.Object);
        var result = await sut.GetDashboardSummary(userId, CancellationToken.None);

        Assert.Equal(0, result.TotalIncome);
        Assert.Equal(0, result.TotalExpense);
        Assert.Equal(0, result.Balanace);
        Assert.Empty(result.ExpensesByCategory);
        Assert.Empty(result.RecentTransactions);
    }
}
namespace PersonalFinance.Web.Models;

public record ExpenseByCategory(Guid CategoryId, string CategoryName, decimal Total);

public record DashboardResponse(
    decimal TotalIncome,
    decimal TotalExpense,
    decimal Balanace,
    ExpenseByCategory[] ExpensesByCategory,
    TransactionResponse[] RecentTransactions);

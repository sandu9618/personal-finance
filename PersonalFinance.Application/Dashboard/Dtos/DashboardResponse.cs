public record DashboardResponse (
  decimal TotalIncome,
  decimal TotalExpense,
  decimal Balanace,
  IReadOnlyList<ExpenseByCategoryDto> ExpensesByCategory,
  IReadOnlyList<TransactionResponse> RecentTransactions
);
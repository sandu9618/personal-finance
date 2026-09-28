using PersonalFinance.Domain.Enums;

public static class TransactionHelper
{
  public static decimal Signed(TransactionType type, decimal amount) => type switch
  {
    TransactionType.Income or TransactionType.TransferIn => amount,
    TransactionType.Expense or TransactionType.TransferOut => -amount,
    _ => throw new InvalidOperationException("Invalid transaction type")
  };
}

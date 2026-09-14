using PersonalFinance.Domain.Enums;

namespace PersonalFinance.Domain.Entities;

public class Category
{
  public Guid Id { get; set; }
  public Guid UserId { get; set; }
  public string Name { get; set; } = string.Empty;
  public TransactionType Type { get; set; }
  public DateTime CreatedAt { get; set; }
  public ApplicationUser User { get; set; } = null!;
  public ICollection<Transaction> Transactions { get; set; } = new List<Transaction>();
}
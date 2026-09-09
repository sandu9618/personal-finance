using PersonalFinance.Domain.Enums;
namespace PersonalFinance.Domain.Entities;

public class Transaction
{
  public Guid Id { get; set; }
  public Guid UserId { get; set; }
  public Guid AccountId { get; set; }
  public Guid CategoryId { get; set; }
  public decimal Amount { get; set; }
  public TransactionType Type { get; set; }
  public string? Description { get; set; }
  public DateTime TransactionDate { get; set; }
  public DateTime CreatedAt { get; set; }
  public ApplicationUser User { get; set; } = null!;
  public Account Account { get; set; } = null!;
  public Category Category { get; set; } = null!;
}

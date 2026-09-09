using PersonalFinance.Domain.Enums;

namespace PersonalFinance.Domain.Entities;

public class Account
{
  public Guid Id { get; set; }
  public Guid UserId { get; set; }
  public string Name { get; set; } = string.Empty;
  public AccountType Type { get; set; }
  public decimal Balance { get; set; }
  public string Currency { get; set; } = "LKR";
  public DateTime CreatedAt { get; set; }
  public ApplicationUser User { get; set; } = null!;
  public ICollection<Transaction> Transactions { get; set; } = new List<Transaction>();
}
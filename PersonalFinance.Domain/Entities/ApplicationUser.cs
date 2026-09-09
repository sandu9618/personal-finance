using Microsoft.AspNetCore.Identity;

namespace PersonalFinance.Domain.Entities;

public class ApplicationUser : IdentityUser<Guid>
{
  public DateTime CreatedAt { get; set; }
  public ICollection<Account> Accounts { get; set; } = new List<Account>();
  public ICollection<Category> Categories { get; set; } = new List<Category>();
  public ICollection<Transaction> Transactions { get; set; } = new List<Transaction>();
}
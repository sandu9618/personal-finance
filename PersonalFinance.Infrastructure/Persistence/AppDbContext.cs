using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using PersonalFinance.Domain.Entities;

public class AppDbContext: IdentityDbContext<ApplicationUser, IdentityRole<Guid>, Guid>
{
  public AppDbContext(DbContextOptions<AppDbContext> options): base(options)
  {
  }

  public DbSet<Account> Accounts => Set<Account>();
  public DbSet<Category> Categories => Set<Category>();
  public DbSet<Transaction> Transactions => Set<Transaction>();

  protected override void OnModelCreating(ModelBuilder builder)
  {
    base.OnModelCreating(builder);
    builder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);
  } 
}
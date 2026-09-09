using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PersonalFinance.Domain.Entities;

namespace PersonalFinance.Infrastructure.Persistence.Configurations;

public class TransactionConfiguration: IEntityTypeConfiguration<Transaction>
{
  public void Configure(EntityTypeBuilder<Transaction> builder)
  {
    builder.Property(t => t.Amount)
      .HasPrecision(18, 2);

    builder.Property(t => t.Description)
      .HasMaxLength(500);

    builder.HasOne(t => t.User)
      .WithMany(u => u.Transactions)
      .HasForeignKey(t => t.UserId)
      .OnDelete(DeleteBehavior.Cascade);

    builder.HasOne(t => t.Account)
      .WithMany(u => u.Transactions)
      .HasForeignKey(t => t.AccountId)
      .OnDelete(DeleteBehavior.Restrict);

    builder.HasOne(t => t.Category)
      .WithMany(u => u.Transactions)
      .HasForeignKey(t => t.CategoryId)
      .OnDelete(DeleteBehavior.Restrict);

    builder.HasIndex(t => t.UserId);
    builder.HasIndex(t => t.AccountId);
    builder.HasIndex(t => t.CategoryId);
  }
}
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PersonalFinance.Domain.Entities;

namespace PersonalFinance.Infrastructure.Persistence.Configurations;

public class AccountConfiguration: IEntityTypeConfiguration<Account>
{
  public void Configure(EntityTypeBuilder<Account> builder)
  {
    builder.Property(a => a.Name)
      .IsRequired()
      .HasMaxLength(100);

    builder.Property(a => a.Currency)
      .IsRequired()
      .HasMaxLength(3);

    builder.Property(a => a.Balance)
      .HasPrecision(18, 2);
      
    builder.HasOne(a => a.User)
      .WithMany(u => u.Accounts)
      .HasForeignKey(a => a.UserId)
      .OnDelete(DeleteBehavior.Cascade);

    builder.HasIndex(a => a.UserId);
  }
}
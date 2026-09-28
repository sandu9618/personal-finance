using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PersonalFinance.Domain.Entities;

namespace PersonalFinance.Infrastructure.Persistence.Configurations;

public class TransferConfiguration : IEntityTypeConfiguration<Transfer>
{
  public void Configure(EntityTypeBuilder<Transfer> builder)
  {
    builder.Property(t => t.Amount)
      .HasPrecision(18, 2);

    builder.Property(t => t.Description)
      .HasMaxLength(500);

    builder.HasOne(t => t.User)
      .WithMany(u => u.Transfers)
      .HasForeignKey(t => t.UserId)
      .OnDelete(DeleteBehavior.Cascade);

    builder.HasOne(t => t.FromAccount)
      .WithMany()
      .HasForeignKey(t => t.FromAccountId)
      .OnDelete(DeleteBehavior.Restrict);

    builder.HasOne(t => t.ToAccount)
      .WithMany()
      .HasForeignKey(t => t.ToAccountId)
      .OnDelete(DeleteBehavior.Restrict);

    builder.HasIndex(t => t.UserId);
    builder.HasIndex(t => t.FromAccountId);
    builder.HasIndex(t => t.ToAccountId);
  }
}

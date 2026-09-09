using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PersonalFinance.Domain.Entities;

namespace PersonalFinance.Infrastructure.Persistence.Configurations;

public class CategoryConfiguration: IEntityTypeConfiguration<Category>
{
  public void Configure(EntityTypeBuilder<Category> builder)
  {
    builder.Property(c => c.Name)
      .IsRequired()
      .HasMaxLength(100);

    builder.HasOne(c => c.User)
      .WithMany(u => u.Categories)
      .HasForeignKey(c => c.UserId)
      .OnDelete(DeleteBehavior.Cascade);

    builder.HasIndex(c => c.UserId);
  }
}
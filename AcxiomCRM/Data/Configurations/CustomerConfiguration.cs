using AcxiomCRM.Models.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AcxiomCRM.Data.Configurations;

public class CustomerConfiguration : IEntityTypeConfiguration<Customer>
{
    public void Configure(EntityTypeBuilder<Customer> builder)
    {
        builder.HasKey(c => c.CustomerId);
        builder.Property(c => c.CustomerCode).IsRequired().HasMaxLength(20);
        builder.Property(c => c.CustomerName).IsRequired().HasMaxLength(150);
        builder.Property(c => c.Email).IsRequired().HasMaxLength(256);
        builder.Property(c => c.Phone).IsRequired().HasMaxLength(15);

        builder.HasIndex(c => c.CustomerCode).IsUnique();
        builder.HasIndex(c => c.Email).IsUnique();
        builder.HasIndex(c => c.Phone).IsUnique();
        builder.HasIndex(c => c.IsDeleted);
        builder.HasIndex(c => c.Status);

        builder.HasOne(c => c.CreatedByUser)
               .WithMany(u => u.CreatedCustomers)
               .HasForeignKey(c => c.CreatedBy)
               .OnDelete(DeleteBehavior.Restrict);

        builder.HasQueryFilter(c => !c.IsDeleted);
    }
}

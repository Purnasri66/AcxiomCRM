using AcxiomCRM.Models.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AcxiomCRM.Data.Configurations;

public class LeadConfiguration : IEntityTypeConfiguration<Lead>
{
    public void Configure(EntityTypeBuilder<Lead> builder)
    {
        builder.HasKey(l => l.LeadId);
        builder.Property(l => l.LeadCode).IsRequired().HasMaxLength(20);
        builder.Property(l => l.LeadName).IsRequired().HasMaxLength(150);
        builder.Property(l => l.Email).IsRequired().HasMaxLength(256);
        builder.Property(l => l.Phone).IsRequired().HasMaxLength(15);
        builder.Property(l => l.ExpectedValue).HasColumnType("decimal(18,2)");

        builder.HasIndex(l => l.LeadCode).IsUnique();
        builder.HasIndex(l => l.Status);
        builder.HasIndex(l => l.IsDeleted);
        builder.HasIndex(l => l.AssignedTo);

        builder.HasOne(l => l.AssignedUser)
               .WithMany(u => u.AssignedLeads)
               .HasForeignKey(l => l.AssignedTo)
               .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(l => l.ConvertedCustomer)
               .WithMany()
               .HasForeignKey(l => l.ConvertedCustomerId)
               .OnDelete(DeleteBehavior.SetNull);

        builder.HasQueryFilter(l => !l.IsDeleted);
    }
}

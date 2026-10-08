using AcxiomCRM.Models.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AcxiomCRM.Data.Configurations;

public class FollowUpConfiguration : IEntityTypeConfiguration<FollowUp>
{
    public void Configure(EntityTypeBuilder<FollowUp> builder)
    {
        builder.HasKey(f => f.FollowUpId);
        builder.Property(f => f.FollowUpType).IsRequired().HasMaxLength(50);
        builder.Property(f => f.Remarks).HasMaxLength(1000);
        builder.Property(f => f.Status).IsRequired().HasMaxLength(50);

        builder.HasIndex(f => f.FollowUpDate);
        builder.HasIndex(f => f.Status);
        builder.HasIndex(f => f.AssignedTo);

        builder.HasOne(f => f.Customer)
               .WithMany(c => c.FollowUps)
               .HasForeignKey(f => f.CustomerId)
               .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(f => f.Lead)
               .WithMany(l => l.FollowUps)
               .HasForeignKey(f => f.LeadId)
               .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(f => f.Opportunity)
               .WithMany(o => o.FollowUps)
               .HasForeignKey(f => f.OpportunityId)
               .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(f => f.AssignedUser)
               .WithMany(u => u.AssignedFollowUps)
               .HasForeignKey(f => f.AssignedTo)
               .OnDelete(DeleteBehavior.Restrict);
    }
}

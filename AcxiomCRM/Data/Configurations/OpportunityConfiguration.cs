using AcxiomCRM.Models.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AcxiomCRM.Data.Configurations;

public class OpportunityConfiguration : IEntityTypeConfiguration<Opportunity>
{
    public void Configure(EntityTypeBuilder<Opportunity> builder)
    {
        builder.HasKey(o => o.OpportunityId);
        builder.Property(o => o.OpportunityName).IsRequired().HasMaxLength(200);
        builder.Property(o => o.Amount).HasColumnType("decimal(18,2)");
        builder.Property(o => o.Stage).IsRequired().HasMaxLength(50);

        builder.HasIndex(o => o.Stage);
        builder.HasIndex(o => o.Status);
        builder.HasIndex(o => o.IsDeleted);
        builder.HasIndex(o => o.AssignedTo);
        builder.HasIndex(o => o.CustomerId);

        builder.HasOne(o => o.Customer)
               .WithMany(c => c.Opportunities)
               .HasForeignKey(o => o.CustomerId)
               .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(o => o.SourceLead)
               .WithMany(l => l.Opportunities)
               .HasForeignKey(o => o.LeadId)
               .OnDelete(DeleteBehavior.SetNull);

        builder.HasOne(o => o.AssignedUser)
               .WithMany(u => u.AssignedOpportunities)
               .HasForeignKey(o => o.AssignedTo)
               .OnDelete(DeleteBehavior.Restrict);

        builder.HasQueryFilter(o => !o.IsDeleted);
    }
}

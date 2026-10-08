using AcxiomCRM.Models.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AcxiomCRM.Data.Configurations;

public class ActivityConfiguration : IEntityTypeConfiguration<Activity>
{
    public void Configure(EntityTypeBuilder<Activity> builder)
    {
        builder.HasKey(a => a.ActivityId);
        builder.Property(a => a.ActivityType).IsRequired().HasMaxLength(50);
        builder.Property(a => a.Subject).IsRequired().HasMaxLength(200);
        builder.Property(a => a.Description).HasMaxLength(2000);

        builder.HasIndex(a => a.ActivityType);
        builder.HasIndex(a => a.ActivityDate);
        builder.HasIndex(a => a.AssignedTo);

        builder.HasOne(a => a.Customer)
               .WithMany(c => c.Activities)
               .HasForeignKey(a => a.CustomerId)
               .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(a => a.Lead)
               .WithMany(l => l.Activities)
               .HasForeignKey(a => a.LeadId)
               .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(a => a.AssignedUser)
               .WithMany(u => u.AssignedActivities)
               .HasForeignKey(a => a.AssignedTo)
               .OnDelete(DeleteBehavior.Restrict);
    }
}

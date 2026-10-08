using AcxiomCRM.Models.Entities;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace AcxiomCRM.Data;

public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    public DbSet<Customer> Customers => Set<Customer>();
    public DbSet<Lead> Leads => Set<Lead>();
    public DbSet<Opportunity> Opportunities => Set<Opportunity>();
    public DbSet<FollowUp> FollowUps => Set<FollowUp>();
    public DbSet<Activity> Activities => Set<Activity>();
    public DbSet<AuditLog> AuditLogs => Set<AuditLog>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        builder.ApplyConfiguration(new Configurations.CustomerConfiguration());
        builder.ApplyConfiguration(new Configurations.LeadConfiguration());
        builder.ApplyConfiguration(new Configurations.OpportunityConfiguration());
        builder.ApplyConfiguration(new Configurations.FollowUpConfiguration());
        builder.ApplyConfiguration(new Configurations.ActivityConfiguration());
        builder.ApplyConfiguration(new Configurations.AuditLogConfiguration());
    }
}

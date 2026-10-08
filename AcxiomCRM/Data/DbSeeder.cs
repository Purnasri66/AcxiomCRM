using AcxiomCRM.Models.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace AcxiomCRM.Data;

public static class DbSeeder
{
    public static async Task SeedAsync(IServiceProvider serviceProvider)
    {
        var context = serviceProvider.GetRequiredService<ApplicationDbContext>();
        var userManager = serviceProvider.GetRequiredService<UserManager<ApplicationUser>>();
        var roleManager = serviceProvider.GetRequiredService<RoleManager<IdentityRole>>();

        if (context.Database.IsSqlServer())
        {
            await context.Database.MigrateAsync();
        }
        else
        {
            await context.Database.EnsureCreatedAsync();
        }

        // Only seed if DB is empty
        if (await context.Customers.AnyAsync())
            return;

        // ── Roles ──
        string[] roles = { "Admin", "Manager", "SalesExecutive" };
        foreach (var role in roles)
        {
            if (!await roleManager.RoleExistsAsync(role))
                await roleManager.CreateAsync(new IdentityRole(role));
        }

        // ── Users ──
        var admin = new ApplicationUser
        {
            UserName = "admin@acxiomcrm.com",
            Email = "admin@acxiomcrm.com",
            FullName = "System Administrator",
            EmailConfirmed = true,
            IsActive = true,
            CreatedDate = DateTime.UtcNow.AddMonths(-6)
        };
        await CreateUserWithRole(userManager, admin, "Admin@12345", "Admin");

        var manager = new ApplicationUser
        {
            UserName = "manager@acxiomcrm.com",
            Email = "manager@acxiomcrm.com",
            FullName = "Sales Manager",
            EmailConfirmed = true,
            IsActive = true,
            CreatedDate = DateTime.UtcNow.AddMonths(-5)
        };
        await CreateUserWithRole(userManager, manager, "Manager@12345", "Manager");

        var sales1 = new ApplicationUser
        {
            UserName = "sales1@acxiomcrm.com",
            Email = "sales1@acxiomcrm.com",
            FullName = "Sales Executive One",
            EmailConfirmed = true,
            IsActive = true,
            CreatedDate = DateTime.UtcNow.AddMonths(-4)
        };
        await CreateUserWithRole(userManager, sales1, "Sales@12345", "SalesExecutive");

        var sales2 = new ApplicationUser
        {
            UserName = "sales2@acxiomcrm.com",
            Email = "sales2@acxiomcrm.com",
            FullName = "Sales Executive Two",
            EmailConfirmed = true,
            IsActive = true,
            CreatedDate = DateTime.UtcNow.AddMonths(-3)
        };
        await CreateUserWithRole(userManager, sales2, "Sales@12345", "SalesExecutive");

        // Reload users to get their IDs
        var adminUser = await userManager.FindByEmailAsync("admin@acxiomcrm.com");
        var managerUser = await userManager.FindByEmailAsync("manager@acxiomcrm.com");
        var sales1User = await userManager.FindByEmailAsync("sales1@acxiomcrm.com");
        var sales2User = await userManager.FindByEmailAsync("sales2@acxiomcrm.com");

        var allSalesUsers = new[] { sales1User!.Id, sales2User!.Id };
        var allUsers = new[] { adminUser!.Id, managerUser!.Id, sales1User.Id, sales2User.Id };
        var rng = new Random(42); // Fixed seed for reproducibility

        // ── Customers (30) ──
        var customers = new List<Customer>();
        var customerNames = new[]
        {
            ("Tata Consultancy Services", "Rajesh Kumar", "rajesh.kumar@tcs.example.com", "9876543210", "Mumbai", "Maharashtra"),
            ("Infosys Limited", "Priya Sharma", "priya.sharma@infosys.example.com", "9876543211", "Bangalore", "Karnataka"),
            ("Wipro Technologies", "Amit Patel", "amit.patel@wipro.example.com", "9876543212", "Hyderabad", "Telangana"),
            ("HCL Technologies", "Sneha Reddy", "sneha.reddy@hcl.example.com", "9876543213", "Noida", "Uttar Pradesh"),
            ("Tech Mahindra", "Vikram Singh", "vikram.singh@techmahindra.example.com", "9876543214", "Pune", "Maharashtra"),
            ("Reliance Industries", "Anita Desai", "anita.desai@reliance.example.com", "9876543215", "Mumbai", "Maharashtra"),
            ("Larsen & Toubro", "Suresh Nair", "suresh.nair@lnt.example.com", "9876543216", "Chennai", "Tamil Nadu"),
            ("Bharti Airtel", "Kavita Joshi", "kavita.joshi@airtel.example.com", "9876543217", "New Delhi", "Delhi"),
            ("Mahindra Group", "Deepak Verma", "deepak.verma@mahindra.example.com", "9876543218", "Mumbai", "Maharashtra"),
            ("Adani Enterprises", "Pooja Gupta", "pooja.gupta@adani.example.com", "9876543219", "Ahmedabad", "Gujarat"),
            ("Sun Pharma", "Rahul Mehta", "rahul.mehta@sunpharma.example.com", "9876543220", "Mumbai", "Maharashtra"),
            ("Bajaj Auto", "Nisha Agarwal", "nisha.agarwal@bajaj.example.com", "9876543221", "Pune", "Maharashtra"),
            ("HDFC Bank", "Kiran Rao", "kiran.rao@hdfc.example.com", "9876543222", "Mumbai", "Maharashtra"),
            ("ICICI Bank", "Sanjay Mishra", "sanjay.mishra@icici.example.com", "9876543223", "Mumbai", "Maharashtra"),
            ("State Bank of India", "Meena Iyer", "meena.iyer@sbi.example.com", "9876543224", "Mumbai", "Maharashtra"),
            ("Axis Bank", "Arun Pillai", "arun.pillai@axis.example.com", "9876543225", "Mumbai", "Maharashtra"),
            ("Hindustan Unilever", "Geeta Bhat", "geeta.bhat@hul.example.com", "9876543226", "Mumbai", "Maharashtra"),
            ("ITC Limited", "Manoj Dubey", "manoj.dubey@itc.example.com", "9876543227", "Kolkata", "West Bengal"),
            ("Asian Paints", "Swati Kulkarni", "swati.kulkarni@asianpaints.example.com", "9876543228", "Mumbai", "Maharashtra"),
            ("Godrej Group", "Nitin Saxena", "nitin.saxena@godrej.example.com", "9876543229", "Mumbai", "Maharashtra"),
            ("Cipla Ltd", "Ritu Chopra", "ritu.chopra@cipla.example.com", "9876543230", "Mumbai", "Maharashtra"),
            ("Dr Reddys Labs", "Vishal Pandey", "vishal.pandey@drreddys.example.com", "9876543231", "Hyderabad", "Telangana"),
            ("Biocon Limited", "Shweta Menon", "shweta.menon@biocon.example.com", "9876543232", "Bangalore", "Karnataka"),
            ("Zomato", "Arjun Kapoor", "arjun.kapoor@zomato.example.com", "9876543233", "Gurugram", "Haryana"),
            ("Flipkart", "Divya Nair", "divya.nair@flipkart.example.com", "9876543234", "Bangalore", "Karnataka"),
            ("Ola Cabs", "Rohit Sharma", "rohit.sharma@olacabs.example.com", "9876543235", "Bangalore", "Karnataka"),
            ("Paytm", "Anjali Tiwari", "anjali.tiwari@paytm.example.com", "9876543236", "Noida", "Uttar Pradesh"),
            ("MakeMyTrip", "Sameer Jain", "sameer.jain@makemytrip.example.com", "9876543237", "Gurugram", "Haryana"),
            ("PolicyBazaar", "Lakshmi Menon", "lakshmi.menon@policybazaar.example.com", "9876543238", "Gurugram", "Haryana"),
            ("Freshworks", "Gaurav Tandon", "gaurav.tandon@freshworks.example.com", "9876543239", "Chennai", "Tamil Nadu")
        };

        for (int i = 0; i < customerNames.Length; i++)
        {
            var (company, name, email, phone, city, state) = customerNames[i];
            var createdBy = allUsers[i % allUsers.Length];
            customers.Add(new Customer
            {
                CustomerCode = $"CUST-{(i + 1):D4}",
                CustomerName = name,
                Email = email,
                Phone = phone,
                CompanyName = company,
                Address = $"{rng.Next(1, 999)} Business Park, Sector {rng.Next(1, 50)}",
                City = city,
                State = state,
                Status = i < 25 ? "Active" : (i < 28 ? "Inactive" : "Active"),
                CreatedDate = DateTime.UtcNow.AddMonths(-rng.Next(1, 6)).AddDays(-rng.Next(0, 28)),
                CreatedBy = createdBy,
                IsActive = i < 28,
                IsDeleted = false
            });
        }

        context.Customers.AddRange(customers);
        await context.SaveChangesAsync();

        // ── Leads (40) ──
        var leads = new List<Lead>();
        var sources = new[] { "Website", "Referral", "LinkedIn", "Cold Call", "Email Campaign", "Trade Show", "Partner" };
        var leadStatuses = new[] { "New", "Contacted", "Qualified", "Converted", "Lost", "Unqualified" };
        var leadNames = new[]
        {
            "Cloud Migration Project", "ERP Implementation", "Data Analytics Platform", "Mobile App Development",
            "Cybersecurity Audit", "AI/ML Solution", "Digital Marketing Suite", "HR Management System",
            "Supply Chain Optimization", "Customer Portal Development", "IoT Platform Setup", "Payment Gateway Integration",
            "E-commerce Platform", "CRM Implementation", "Business Intelligence Tool", "Warehouse Management",
            "Fleet Management System", "Healthcare Portal", "Education Platform", "Real Estate CRM",
            "Insurance Claims System", "Banking Portal", "Retail POS System", "Restaurant Management",
            "Hotel Booking System", "Logistics Platform", "Event Management App", "Social Media Tool",
            "Content Management System", "Project Management Tool", "Inventory Management", "Billing System",
            "Document Management", "Video Conferencing Tool", "Chat Application", "Email Marketing Platform",
            "Survey Platform", "Feedback Management", "Quality Assurance Tool", "DevOps Platform"
        };

        for (int i = 0; i < 40; i++)
        {
            var status = i < 8 ? "New" : (i < 16 ? "Contacted" : (i < 24 ? "Qualified" : (i < 30 ? "Converted" : (i < 36 ? "Lost" : "Unqualified"))));
            leads.Add(new Lead
            {
                LeadCode = $"LEAD-{(i + 1):D4}",
                LeadName = leadNames[i],
                Email = $"lead{i + 1}@company{i + 1}.example.com",
                Phone = $"{6 + rng.Next(0, 4)}{rng.Next(100000000, 999999999)}",
                CompanyName = $"Company {(char)('A' + (i % 26))} Solutions",
                Source = sources[i % sources.Length],
                Status = status,
                ExpectedValue = rng.Next(1, 100) * 10000m,
                LeadScore = rng.Next(10, 100),
                Priority = rng.Next(0, 3) switch { 0 => "High", 1 => "Medium", _ => "Low" },
                CreatedDate = DateTime.UtcNow.AddMonths(-rng.Next(1, 5)).AddDays(-rng.Next(0, 28)),
                AssignedTo = allSalesUsers[i % allSalesUsers.Length],
                IsActive = status != "Lost",
                IsDeleted = false,
                ConvertedCustomerId = status == "Converted" ? customers[i % customers.Count].CustomerId : null
            });
        }

        context.Leads.AddRange(leads);
        await context.SaveChangesAsync();

        // ── Opportunities (40) ──
        var opportunities = new List<Opportunity>();
        var stages = new[] { "Qualification", "Proposal", "Negotiation", "Won", "Lost" };

        for (int i = 0; i < 40; i++)
        {
            var stage = stages[i % stages.Length];
            var prob = stage switch
            {
                "Qualification" => rng.Next(10, 30),
                "Proposal" => rng.Next(30, 60),
                "Negotiation" => rng.Next(60, 85),
                "Won" => 100,
                "Lost" => 0,
                _ => 50
            };
            var status = stage == "Won" ? "Won" : (stage == "Lost" ? "Lost" : "Open");
            opportunities.Add(new Opportunity
            {
                OpportunityName = $"Opp-{leadNames[i % leadNames.Length]}",
                CustomerId = customers[i % customers.Count].CustomerId,
                LeadId = i < 30 ? leads[i % leads.Count].LeadId : null,
                Amount = rng.Next(5, 200) * 10000m,
                Stage = stage,
                Probability = prob,
                ExpectedCloseDate = stage == "Won"
                    ? DateTime.UtcNow.AddDays(-rng.Next(1, 60))
                    : (stage == "Lost" ? DateTime.UtcNow.AddDays(-rng.Next(1, 30)) : DateTime.UtcNow.AddDays(rng.Next(7, 90))),
                Status = status,
                CreatedDate = DateTime.UtcNow.AddMonths(-rng.Next(1, 4)).AddDays(-rng.Next(0, 28)),
                AssignedTo = allSalesUsers[i % allSalesUsers.Length],
                IsActive = status != "Lost",
                IsDeleted = false
            });
        }

        context.Opportunities.AddRange(opportunities);
        await context.SaveChangesAsync();

        // ── Follow-Ups (60) ──
        var followUps = new List<FollowUp>();
        var followUpTypes = new[] { "Call", "Meeting", "Email", "Visit", "Demo" };
        var followUpStatuses = new[] { "Planned", "Completed", "Missed", "Cancelled" };

        for (int i = 0; i < 60; i++)
        {
            var fStatus = i < 20 ? "Completed" : (i < 35 ? "Planned" : (i < 50 ? "Missed" : "Cancelled"));
            var dayOffset = fStatus == "Completed" ? -rng.Next(1, 60) : (fStatus == "Planned" ? rng.Next(1, 30) : -rng.Next(1, 14));
            followUps.Add(new FollowUp
            {
                CustomerId = i % 3 == 0 ? customers[i % customers.Count].CustomerId : null,
                LeadId = i % 3 == 1 ? leads[i % leads.Count].LeadId : null,
                OpportunityId = i % 3 == 2 ? opportunities[i % opportunities.Count].OpportunityId : null,
                FollowUpDate = DateTime.UtcNow.AddDays(dayOffset),
                FollowUpType = followUpTypes[i % followUpTypes.Length],
                Remarks = $"Follow-up regarding {(i % 3 == 0 ? "customer engagement" : (i % 3 == 1 ? "lead qualification" : "opportunity progress"))} - item {i + 1}",
                Status = fStatus,
                AssignedTo = allSalesUsers[i % allSalesUsers.Length],
                CreatedDate = DateTime.UtcNow.AddDays(dayOffset - rng.Next(1, 7))
            });
        }

        context.FollowUps.AddRange(followUps);
        await context.SaveChangesAsync();

        // ── Activities (80) ──
        var activities = new List<Activity>();
        var activityTypes = new[] { "Call", "Meeting", "Email", "Task" };
        var subjects = new[]
        {
            "Initial contact call", "Product demo meeting", "Proposal follow-up email", "Contract review task",
            "Quarterly review call", "Technical discussion", "Budget approval follow-up", "Partnership discussion",
            "Feature walkthrough", "Security audit meeting", "Onboarding call", "Training session",
            "Support escalation", "Feedback collection", "Renewal discussion", "Upsell opportunity discussion"
        };

        for (int i = 0; i < 80; i++)
        {
            var aStatus = i < 50 ? "Completed" : (i < 65 ? "Planned" : "In Progress");
            activities.Add(new Activity
            {
                ActivityType = activityTypes[i % activityTypes.Length],
                Subject = subjects[i % subjects.Length],
                Description = $"Detailed description for activity #{i + 1}. This relates to ongoing business development and customer relationship management activities.",
                ActivityDate = DateTime.UtcNow.AddDays(-rng.Next(-30, 90)),
                CustomerId = i % 2 == 0 ? customers[i % customers.Count].CustomerId : null,
                LeadId = i % 2 == 1 ? leads[i % leads.Count].LeadId : null,
                AssignedTo = allSalesUsers[i % allSalesUsers.Length],
                Status = aStatus,
                CreatedDate = DateTime.UtcNow.AddMonths(-rng.Next(1, 4)).AddDays(-rng.Next(0, 28))
            });
        }

        context.Activities.AddRange(activities);
        await context.SaveChangesAsync();

        // ── Audit Logs (seed some initial entries) ──
        var auditLogs = new List<AuditLog>
        {
            new() { UserId = adminUser.Id, Action = "Login", EntityName = "User", RecordId = adminUser.Id, CreatedDate = DateTime.UtcNow.AddDays(-30), IpAddress = "127.0.0.1" },
            new() { UserId = adminUser.Id, Action = "SeedData", EntityName = "System", RecordId = "0", NewValue = "Initial database seeding completed", CreatedDate = DateTime.UtcNow, IpAddress = "127.0.0.1" }
        };

        context.AuditLogs.AddRange(auditLogs);
        await context.SaveChangesAsync();
    }

    private static async Task CreateUserWithRole(UserManager<ApplicationUser> userManager, ApplicationUser user, string password, string role)
    {
        var existing = await userManager.FindByEmailAsync(user.Email!);
        if (existing == null)
        {
            var result = await userManager.CreateAsync(user, password);
            if (result.Succeeded)
            {
                await userManager.AddToRoleAsync(user, role);
            }
        }
    }
}

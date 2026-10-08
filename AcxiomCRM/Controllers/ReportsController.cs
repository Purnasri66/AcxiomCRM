using System.Globalization;
using System.Security.Claims;
using System.Text;
using AcxiomCRM.Models.Entities;
using AcxiomCRM.Repositories;
using AcxiomCRM.Services;
using CsvHelper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AcxiomCRM.Controllers;

public class ReportsController : Controller
{
    private readonly ICustomerRepository _customerRepo;
    private readonly ILeadRepository _leadRepo;
    private readonly IOpportunityRepository _opportunityRepo;
    private readonly IFollowUpRepository _followUpRepo;
    private readonly IAuditLogRepository _auditRepo;

    public ReportsController(
        ICustomerRepository customerRepo,
        ILeadRepository leadRepo,
        IOpportunityRepository opportunityRepo,
        IFollowUpRepository followUpRepo,
        IAuditLogRepository auditRepo)
    {
        _customerRepo = customerRepo;
        _leadRepo = leadRepo;
        _opportunityRepo = opportunityRepo;
        _followUpRepo = followUpRepo;
        _auditRepo = auditRepo;
    }

    public IActionResult Index()
    {
        return View();
    }

    public async Task<IActionResult> Customers(string? search, string? format)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? string.Empty;
        var role = User.FindFirstValue(ClaimTypes.Role) ?? "SalesExecutive";
        var scopedUserId = role == "SalesExecutive" ? userId : null;

        var query = _customerRepo.GetCustomersQuery(scopedUserId);
        if (!string.IsNullOrWhiteSpace(search))
            query = query.Where(c => c.CustomerName.Contains(search) || c.Email.Contains(search));

        var data = await query.OrderByDescending(c => c.CreatedDate).ToListAsync();

        if (format == "csv")
        {
            return ExportToCsv(data.Select(c => new
            {
                c.CustomerCode, c.CustomerName, c.Email, c.Phone, c.CompanyName, c.City, c.State, c.Status, c.CreatedDate
            }), "Customers_Report.csv");
        }

        return View(data);
    }

    public async Task<IActionResult> Leads(string? status, string? format)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? string.Empty;
        var role = User.FindFirstValue(ClaimTypes.Role) ?? "SalesExecutive";
        var scopedUserId = role == "SalesExecutive" ? userId : null;

        var query = _leadRepo.GetLeadsQuery(scopedUserId);
        if (!string.IsNullOrWhiteSpace(status))
            query = query.Where(l => l.Status == status);

        var data = await query.OrderByDescending(l => l.CreatedDate).ToListAsync();

        if (format == "csv")
        {
            return ExportToCsv(data.Select(l => new
            {
                l.LeadCode, l.LeadName, l.Email, l.Phone, l.CompanyName, l.Source, l.Status, l.ExpectedValue, l.LeadScore, l.Priority, l.CreatedDate
            }), "Leads_Report.csv");
        }

        return View(data);
    }

    public async Task<IActionResult> Pipeline(string? format)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? string.Empty;
        var role = User.FindFirstValue(ClaimTypes.Role) ?? "SalesExecutive";
        var scopedUserId = role == "SalesExecutive" ? userId : null;

        var opps = await _opportunityRepo.GetOpportunitiesQuery(scopedUserId).Where(o => o.Status == "Open").ToListAsync();

        var stages = new[] { "Qualification", "Proposal", "Negotiation" };
        var report = stages.Select(s => new
        {
            Stage = s,
            Count = opps.Count(o => o.Stage == s),
            TotalValue = opps.Where(o => o.Stage == s).Sum(o => o.Amount),
            WeightedValue = opps.Where(o => o.Stage == s).Sum(o => o.Amount * o.Probability / 100)
        }).ToList();

        if (format == "csv")
        {
            return ExportToCsv(report, "Pipeline_Report.csv");
        }

        ViewBag.Opportunities = opps;
        return View(report);
    }

    public async Task<IActionResult> FollowUps(string? status, string? format)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? string.Empty;
        var role = User.FindFirstValue(ClaimTypes.Role) ?? "SalesExecutive";
        var scopedUserId = role == "SalesExecutive" ? userId : null;

        var query = _followUpRepo.GetFollowUpsQuery(scopedUserId);
        if (!string.IsNullOrWhiteSpace(status))
            query = query.Where(f => f.Status == status);

        var data = await query.OrderByDescending(f => f.FollowUpDate).ToListAsync();

        if (format == "csv")
        {
            return ExportToCsv(data.Select(f => new
            {
                f.FollowUpDate, f.FollowUpType, f.Status, f.Remarks,
                Customer = f.Customer?.CustomerName, Lead = f.Lead?.LeadName,
                Opportunity = f.Opportunity?.OpportunityName, f.AssignedTo
            }), "FollowUps_Report.csv");
        }

        return View(data);
    }

    private FileResult ExportToCsv<T>(IEnumerable<T> records, string fileName)
    {
        using var memoryStream = new MemoryStream();
        using (var writer = new StreamWriter(memoryStream, Encoding.UTF8))
        using (var csv = new CsvWriter(writer, CultureInfo.InvariantCulture))
        {
            csv.WriteRecords(records);
        }
        return File(memoryStream.ToArray(), "text/csv", fileName);
    }
}

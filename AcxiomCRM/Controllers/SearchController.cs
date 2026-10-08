using System.Security.Claims;
using AcxiomCRM.Models.ViewModels;
using AcxiomCRM.Repositories;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AcxiomCRM.Controllers;

public class SearchController : Controller
{
    private readonly ICustomerRepository _customerRepo;
    private readonly ILeadRepository _leadRepo;
    private readonly IOpportunityRepository _opportunityRepo;

    public SearchController(
        ICustomerRepository customerRepo,
        ILeadRepository leadRepo,
        IOpportunityRepository opportunityRepo)
    {
        _customerRepo = customerRepo;
        _leadRepo = leadRepo;
        _opportunityRepo = opportunityRepo;
    }

    [HttpGet]
    public async Task<IActionResult> QuickSearch(string q)
    {
        if (string.IsNullOrWhiteSpace(q) || q.Length < 2)
            return Json(new GlobalSearchResultViewModel { Query = q ?? string.Empty });

        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? string.Empty;
        var role = User.FindFirstValue(ClaimTypes.Role) ?? "SalesExecutive";
        var scopedUserId = role == "SalesExecutive" ? userId : null;

        var term = q.ToLower();

        var customers = await _customerRepo.GetCustomersQuery(scopedUserId)
            .Where(c => c.CustomerName.ToLower().Contains(term) || c.Email.ToLower().Contains(term) || (c.CompanyName != null && c.CompanyName.ToLower().Contains(term)))
            .Take(5)
            .Select(c => new SearchResultItem
            {
                Id = c.CustomerId,
                Title = c.CustomerName,
                Subtitle = $"{c.CustomerCode} • {c.Email}",
                Url = $"/Customer/Details/{c.CustomerId}",
                Badge = "Customer"
            })
            .ToListAsync();

        var leads = await _leadRepo.GetLeadsQuery(scopedUserId)
            .Where(l => l.LeadName.ToLower().Contains(term) || l.Email.ToLower().Contains(term))
            .Take(5)
            .Select(l => new SearchResultItem
            {
                Id = l.LeadId,
                Title = l.LeadName,
                Subtitle = $"{l.LeadCode} • Status: {l.Status}",
                Url = $"/Lead/Details/{l.LeadId}",
                Badge = "Lead"
            })
            .ToListAsync();

        var opps = await _opportunityRepo.GetOpportunitiesQuery(scopedUserId)
            .Where(o => o.OpportunityName.ToLower().Contains(term))
            .Take(5)
            .Select(o => new SearchResultItem
            {
                Id = o.OpportunityId,
                Title = o.OpportunityName,
                Subtitle = $"₹{o.Amount:N0} • {o.Stage}",
                Url = $"/Opportunity/Details/{o.OpportunityId}",
                Badge = "Opportunity"
            })
            .ToListAsync();

        var result = new GlobalSearchResultViewModel
        {
            Query = q,
            Customers = customers,
            Leads = leads,
            Opportunities = opps
        };

        return Json(result);
    }
}

using AcxiomCRM.Models.Entities;

namespace AcxiomCRM.Models.ViewModels;

public class AuditLogListViewModel
{
    public IEnumerable<AuditLogItemViewModel> Items { get; set; } = new List<AuditLogItemViewModel>();
    public string? EntityFilter { get; set; }
    public string? ActionFilter { get; set; }
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public int CurrentPage { get; set; } = 1;
    public int PageSize { get; set; } = 20;
    public int TotalCount { get; set; }
    public int TotalPages => (int)Math.Ceiling((double)TotalCount / PageSize);
    public bool HasPreviousPage => CurrentPage > 1;
    public bool HasNextPage => CurrentPage < TotalPages;
}

public class AuditLogItemViewModel
{
    public int AuditLogId { get; set; }
    public string UserId { get; set; } = string.Empty;
    public string? UserName { get; set; }
    public string Action { get; set; } = string.Empty;
    public string EntityName { get; set; } = string.Empty;
    public string? RecordId { get; set; }
    public string? OldValue { get; set; }
    public string? NewValue { get; set; }
    public DateTime CreatedDate { get; set; }
    public string? IpAddress { get; set; }
    public List<AuditDiffItemViewModel> DiffItems { get; set; } = new();
}

public class AuditDiffItemViewModel
{
    public string FieldName { get; set; } = string.Empty;
    public string? OldValue { get; set; }
    public string? NewValue { get; set; }
}

namespace AcxiomCRM.Services;

/// <summary>
/// Service for generating reports with filtering, sorting and CSV export.
/// </summary>
public interface IReportService
{
    Task<object> GetCustomerReportAsync(string userId, string role, string? search = null, DateTime? startDate = null, DateTime? endDate = null, int page = 1, int pageSize = 25);
    Task<object> GetLeadReportAsync(string userId, string role, string? statusFilter = null, DateTime? startDate = null, DateTime? endDate = null, int page = 1, int pageSize = 25);
    Task<object> GetOpportunityReportAsync(string userId, string role, string? stageFilter = null, DateTime? startDate = null, DateTime? endDate = null, int page = 1, int pageSize = 25);
    Task<object> GetFollowUpReportAsync(string userId, string role, string? statusFilter = null, DateTime? startDate = null, DateTime? endDate = null, int page = 1, int pageSize = 25);
    Task<object> GetPipelineReportAsync(string userId, string role);
    Task<object> GetAuditReportAsync(string userId, string role, string? entityFilter = null, string? actionFilter = null, DateTime? startDate = null, DateTime? endDate = null, int page = 1, int pageSize = 25);
}

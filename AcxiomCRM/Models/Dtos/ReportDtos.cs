namespace AcxiomCRM.Models.Dtos;

public class PipelineReportDto
{
    public string Stage { get; set; } = string.Empty;
    public int DealCount { get; set; }
    public decimal TotalAmount { get; set; }
    public decimal WeightedAmount { get; set; }
}

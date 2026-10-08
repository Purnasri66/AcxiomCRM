namespace AcxiomCRM.Models.ViewModels;

public class GlobalSearchResultViewModel
{
    public string Query { get; set; } = string.Empty;
    public List<SearchResultItem> Customers { get; set; } = new();
    public List<SearchResultItem> Leads { get; set; } = new();
    public List<SearchResultItem> Opportunities { get; set; } = new();
}

public class SearchResultItem
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Subtitle { get; set; } = string.Empty;
    public string Url { get; set; } = string.Empty;
    public string Badge { get; set; } = string.Empty;
}

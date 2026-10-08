namespace AcxiomCRM.Services;

/// <summary>
/// Service for getting notification data (overdue/upcoming follow-ups).
/// </summary>
public interface INotificationService
{
    Task<NotificationData> GetNotificationsAsync(string userId, string role);
}

public class NotificationData
{
    public int TotalCount { get; set; }
    public List<NotificationItem> Items { get; set; } = new();
}

public class NotificationItem
{
    public string Title { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty; // overdue, upcoming
    public string Url { get; set; } = string.Empty;
    public DateTime Date { get; set; }
}

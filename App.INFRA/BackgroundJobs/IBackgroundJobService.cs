namespace App.INFRA.BackgroundJobs;

/// <summary>
/// Interface for background job operations.
/// Note: Methods without CancellationToken are for Hangfire job expressions.
/// </summary>
public interface IBackgroundJobService
{
    // Email Jobs (no CT version for Hangfire expression trees)
    Task SendEmailAsync(string to, string subject, string body);
    Task SendBulkEmailAsync(IEnumerable<string> recipients, string subject, string body);
    Task SendOrderConfirmationEmailAsync(int orderId);
    Task SendPasswordResetEmailAsync(string userId, string resetToken);
    
    // Maintenance Jobs (no CT version for Hangfire expression trees)
    Task CleanupExpiredCacheAsync();
    Task CheckPendingOrdersAsync();
    Task MonthlyDataCleanupAsync();
    
    // Report Jobs (no CT version for Hangfire expression trees)
    Task GenerateWeeklyReportAsync();
    Task GenerateSalesReportAsync(DateTime startDate, DateTime endDate);
}

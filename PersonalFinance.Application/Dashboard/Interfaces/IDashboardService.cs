public interface IDashboardService
{
  Task<DashboardResponse> GetDashboardSummary(Guid userId, CancellationToken cancellationToken);
}
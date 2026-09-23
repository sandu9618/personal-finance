using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PersonalFinance.Api;

[ApiController]
[Authorize]
[Route("api/[controller]")]
public class DashboardController : ControllerBase
{
  private readonly IDashboardService _dashboardService;
  public DashboardController(IDashboardService dashboardService)
  {
    _dashboardService = dashboardService;
  }
  [HttpGet]
  public async Task<ActionResult<DashboardResponse>> GetDashboardSummary(CancellationToken cancellationToken)
  {
    try
    {
      var userId = User.GetUserIdFromClaims();
      var response = await _dashboardService.GetDashboardSummary(userId, cancellationToken);
      return Ok(response);
    }
    catch (UnauthorizedAccessException ex)
    {
      return Unauthorized(new { message = ex.Message });
    }
    catch (InvalidOperationException ex)
    {
      return BadRequest(new { message = ex.Message });
    }
  }
}

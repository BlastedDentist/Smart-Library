using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartLibrary.Api.Services;

namespace SmartLibrary.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AnalyticsController : ControllerBase
{
    private readonly IAnalyticsService _analyticsService;

    public AnalyticsController(IAnalyticsService analyticsService)
    {
        _analyticsService = analyticsService;
    }

    // GET /api/analytics/summary — any logged-in user (Admin or Student).
    [Authorize]
    [HttpGet("summary")]
    public async Task<IActionResult> GetSummary()
    {
        var result = await _analyticsService.GetAnalyticsSummaryAsync();
        return Ok(new { success = true, data = result });
    }
}

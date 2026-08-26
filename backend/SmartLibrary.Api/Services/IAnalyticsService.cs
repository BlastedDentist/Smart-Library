using SmartLibrary.Api.DTOs;

namespace SmartLibrary.Api.Services;

public interface IAnalyticsService
{
    Task<AnalyticsSummaryDto> GetAnalyticsSummaryAsync();
}

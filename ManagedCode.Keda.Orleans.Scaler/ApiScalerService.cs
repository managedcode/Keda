using System.Text.Json;
using ManagedCode.Keda.Orleans.Scaler.Models;
using Microsoft.Extensions.Logging;

namespace ManagedCode.Keda.Orleans.Scaler;

public class ApiScalerService
{
    private readonly GrainStatsService _grainStatsService;
    private readonly ILogger<ApiScalerService> _logger;

    public ApiScalerService(ILogger<ApiScalerService> logger, GrainStatsService grainStatsService)
    {
        _logger = logger;
        _grainStatsService = grainStatsService;
    }

    public async Task<OrleansStats> GetOrleansStatsAsync()
    {
        var grains = await _grainStatsService.GetGrainActivationsAsync();
        var grainsCount = grains.Sum(x => x.Value);

        var stats = new OrleansStats(grainsCount, grains);

        _logger.LogInformation(JsonSerializer.Serialize(stats));

        return stats;
    }
}
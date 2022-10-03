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

    public async Task<ScalerStats> GetStatsAsync()
    {
        var siloCount = await _grainStatsService.GetActiveSiloCountAsync();
        var grains = await _grainStatsService.GetGrainActivationsAsync();
        var grainsCount = grains.Sum(x => x.Value);

        var stats = new ScalerStats(grainsCount, grainsCount / siloCount, grains);

        _logger.LogInformation(JsonSerializer.Serialize(stats));

        return stats;
    }
}
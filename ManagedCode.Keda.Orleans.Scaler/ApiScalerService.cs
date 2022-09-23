using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.Extensions.Logging;

namespace ManagedCode.Keda.Orleans.Scaler;

public class ApiScalerService
{
    private readonly ILogger<ApiScalerService> _logger;
    private readonly GrainStatsService _grainStatsService;

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

public record ScalerStats(int GrainCount, int GrainsPerSilo, Dictionary<string, int> Grains);
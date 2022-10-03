using System.Text.Json;
using ManagedCode.Keda.Orleans.Scaler.Client.Models;

namespace ManagedCode.Keda.Orleans.Scaler.Client;

public class ApiOrleansScalerService
{
    private readonly OrleansStatsService _orleansStatsService;
    private readonly ILogger<ApiOrleansScalerService> _logger;

    public ApiOrleansScalerService(ILogger<ApiOrleansScalerService> logger, OrleansStatsService orleansStatsService)
    {
        _logger = logger;
        _orleansStatsService = orleansStatsService;
    }

    public async Task<OrleansStats> GetOrleansStatsAsync()
    {
        var grains = await _orleansStatsService.GetGrainActivationsAsync();
        var grainsCount = grains.Sum(x => x.Value);

        var stats = new OrleansStats(grainsCount, grains);

        _logger.LogInformation(JsonSerializer.Serialize(stats));

        return stats;
    }
}
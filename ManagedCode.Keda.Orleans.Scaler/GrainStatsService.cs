using Microsoft.Extensions.Logging;
using Orleans;
using Orleans.Runtime;

namespace ManagedCode.Keda.Orleans.Scaler;

public class GrainStatsService
{
    private readonly IManagementGrain _managementGrain;
    private readonly ILogger<GrainStatsService> _logger;

    public GrainStatsService(IClusterClient orleansClusterClient, ILogger<GrainStatsService> logger)
    {
        _logger = logger;

        OrleansClusterClient = orleansClusterClient;

        if (OrleansClusterClient.IsInitialized)
        {
            Task.Run(async () => await OrleansClusterClient.Connect());
        }

        _managementGrain = OrleansClusterClient.GetGrain<IManagementGrain>(0);
    }

    public IClusterClient OrleansClusterClient { get; }

    public async Task<int> GetGrainCountInClusterAsync(string? grainType = null)
    {
        var statistics = await _managementGrain.GetDetailedGrainStatistics();
        var activeGrainsInCluster =
            statistics.Select(grainStatistic => new GrainInfo(grainStatistic.GrainType, grainStatistic.GrainIdentity.IdentityString,
                grainStatistic.SiloAddress.ToGatewayUri().AbsoluteUri));

        var grainsCount = grainType switch
        {
            null => activeGrainsInCluster.Count(),
            _ => activeGrainsInCluster.Count(grainInfo => grainInfo.Type.ToLower().Contains(grainType))
        };

        _logger?.LogInformation($"Found {grainsCount} grain instances of {grainType} in cluster");

        return grainsCount;
    }

    public async Task<Dictionary<string, int>> GetGrainActivationsAsync()
    {
        var statistics = await _managementGrain.GetDetailedGrainStatistics();

        var activeGrainsInCluster =
            statistics.Select(grainStatistic => new GrainInfo(grainStatistic.GrainType, grainStatistic.GrainIdentity.IdentityString,
                grainStatistic.SiloAddress.ToGatewayUri().AbsoluteUri));

        return activeGrainsInCluster
            .GroupBy(a => a.Type)
            .ToDictionary(g => g.Key.Split('.').Last(), g => g.Count());
    }

    public async Task<int> GetActiveSiloCountAsync(string? siloNameFilter = null)
    {
        var detailedHosts = await _managementGrain.GetDetailedHosts();

        var silos = detailedHosts
            .Where(x => x.Status == SiloStatus.Active)
            .Select(_ => new SiloInfo(_.SiloName, _.SiloAddress.ToGatewayUri().AbsoluteUri));

        var activeSiloCount = siloNameFilter switch
        {
            null => silos.Count(),
            _ => silos.Count(siloInfo => siloInfo.SiloName.ToLower().Contains(siloNameFilter.ToLower()))
        };

        _logger?.LogInformation($"Found active silos {activeSiloCount} by filter '{siloNameFilter}'");

        return activeSiloCount;
    }
}

public record GrainInfo(string Type, string PrimaryKey, string SiloName);

public record SiloInfo(string SiloName, string SiloAddress);

public record Limit(string GrainType, int Upperbound);
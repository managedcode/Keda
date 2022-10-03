using ManagedCode.Keda.Orleans.Scaler.Models;
using Microsoft.Extensions.Logging;
using Orleans;
using Orleans.Runtime;

namespace ManagedCode.Keda.Orleans.Scaler;

public class GrainStatsService
{
    private readonly ILogger<GrainStatsService> _logger;
    private readonly IManagementGrain _managementGrain;

    public GrainStatsService(IGrainFactory orleansClusterClient, ILogger<GrainStatsService> logger)
    {
        _logger = logger;
        _managementGrain = orleansClusterClient.GetGrain<IManagementGrain>(0);
    }


    public async Task<int> GetGrainCountInClusterAsync(params string[] grainTypes)
    {
        var statistics = await _managementGrain.GetDetailedGrainStatistics(grainTypes.Length == 0 ? null : grainTypes);

        var activeGrainsInCluster =
            statistics.Select(grainStatistic => new GrainInfo(grainStatistic.GrainType, grainStatistic.GrainIdentity.IdentityString,
                grainStatistic.SiloAddress.ToGatewayUri().AbsoluteUri));

        var grainCount = activeGrainsInCluster.Count();

        _logger.LogInformation($"Found {grainCount} grain instances of {grainTypes} in cluster");

        return grainCount;
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
            .Select(x => new SiloInfo(x.SiloName, x.SiloAddress.ToGatewayUri().AbsoluteUri));

        var activeSiloCount = siloNameFilter switch
        {
            null => silos.Count(),
            _ => silos.Count(siloInfo => siloInfo.SiloName.ToLower().Contains(siloNameFilter.ToLower()))
        };

        _logger.LogInformation($"Found active silos {activeSiloCount} by filter '{siloNameFilter}'");

        return activeSiloCount;
    }
}
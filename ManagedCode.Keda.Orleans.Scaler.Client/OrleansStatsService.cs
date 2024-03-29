using ManagedCode.Keda.Orleans.Scaler.Client.Models;
using Orleans.Runtime;

namespace ManagedCode.Keda.Orleans.Scaler.Client;

public class OrleansStatsService
{
    private readonly ILogger<OrleansStatsService> _logger;
    private readonly IManagementGrain _managementGrain;

    public OrleansStatsService(IClusterClient grainFactory, ILogger<OrleansStatsService> logger)
    {
        _logger = logger;
        _managementGrain = grainFactory.GetGrain<IManagementGrain>(0);
    }

    public async Task<int> GetGrainCountInClusterAsync(params string[] grainTypes)
    {
        var statistics = await _managementGrain.GetSimpleGrainStatistics();

        var activeGrainsInCluster =
            statistics.Select(grainStatistic => new GrainInfo(grainStatistic.GrainType, grainStatistic.SiloAddress.ToGatewayUri().AbsoluteUri));

        var grainCount = activeGrainsInCluster.Count();

        _logger.LogInformation($"Found {grainCount} grain instances of {grainTypes} in cluster");

        return grainCount;
    }

    public async Task<Dictionary<string, int>> GetGrainActivationsAsync()
    {
        var statistics = await _managementGrain.GetSimpleGrainStatistics();

        var result = new Dictionary<string, int>();

        foreach (var statistic in statistics)
        {
            var grain = statistic.GrainType.Split('.').Last();
            
            if (result.TryGetValue(grain, out _))
            {
                result[grain] += statistic.ActivationCount;
            }
            else
            {
                result[grain] = statistic.ActivationCount;
            }
        }

        return result;
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
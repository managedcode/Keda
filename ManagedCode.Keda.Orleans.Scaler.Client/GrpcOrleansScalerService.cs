using Externalscaler;
using Grpc.Core;
using ManagedCode.Keda.Orleans.Scaler.Client.Models;

namespace ManagedCode.Keda.Orleans.Scaler.Client;

public class GrpcOrleansScalerService : ExternalScaler.ExternalScalerBase
{
    private const string MetricName = "grainThreshold";
    private const string Limits = "limits";
    private const string SiloNameFilter = "siloNameFilter";
    private readonly OrleansStatsService _orleansStatsService;
    private readonly ILogger<GrpcOrleansScalerService> _logger;

    public GrpcOrleansScalerService(OrleansStatsService orleansStatsService, ILogger<GrpcOrleansScalerService> logger)
    {
        _logger = logger;
        _orleansStatsService = orleansStatsService;
    }

    public override async Task<GetMetricsResponse> GetMetrics(GetMetricsRequest request, ServerCallContext context)
    {
        CheckRequestMetadata(request.ScaledObjectRef);

        var siloNameFilter = request.ScaledObjectRef.ScalerMetadata[SiloNameFilter];
        var siloCount = await _orleansStatsService.GetActiveSiloCountAsync(siloNameFilter);

        var needToScale = false;
        var metricValue = 0;

        foreach (var limit in GetLimits(request.ScaledObjectRef.ScalerMetadata))
        {
            var grainCount = await _orleansStatsService.GetGrainCountInClusterAsync(limit.GrainType);
            var grainsPerSilo = grainCount > 0 && siloCount > 0 ? grainCount / siloCount : 0;

            _logger?.LogInformation($"Grains Per Silo: {grainsPerSilo}, Upper Bound: {limit.Upperbound}, Grain Count: {grainCount}");

            if (grainsPerSilo >= limit.Upperbound)
            {
                needToScale = true;
                break;
            }

            if (grainsPerSilo < limit.Upperbound)
            {
                metricValue = Math.Max(metricValue, grainCount == 0 ? 1 : Convert.ToInt16(grainCount / limit.Upperbound));
            }
        }

        metricValue = needToScale ? siloCount + 1 : metricValue;

        _logger?.LogInformation($"Silo Count: {siloCount}. Scale to {metricValue}.");

        return new GetMetricsResponse
        {
            MetricValues =
            {
                new MetricValue
                {
                    MetricName = MetricName,
                    MetricValue_ = siloCount
                }
            }
        };
    }

    public override Task<GetMetricSpecResponse> GetMetricSpec(ScaledObjectRef request, ServerCallContext context)
    {
        CheckRequestMetadata(request);

        var response = new GetMetricSpecResponse();

        response.MetricSpecs.Add(new MetricSpec
        {
            MetricName = MetricName,
            TargetSize = 1
        });

        return Task.FromResult(response);
    }

    public override async Task StreamIsActive(ScaledObjectRef request,
        IServerStreamWriter<IsActiveResponse> responseStream,
        ServerCallContext context)
    {
        CheckRequestMetadata(request);

        while (!context.CancellationToken.IsCancellationRequested)
        {
            if (await AreTooManyGrainsInTheCluster(request))
            {
                _logger?.LogInformation($"Writing {nameof(IsActiveResponse)} to stream with Result = true.");

                await responseStream.WriteAsync(new IsActiveResponse
                {
                    Result = true
                });
            }

            await Task.Delay(TimeSpan.FromSeconds(30));
        }
    }

    public override async Task<IsActiveResponse> IsActive(ScaledObjectRef request, ServerCallContext context)
    {
        CheckRequestMetadata(request);

        var result = await AreTooManyGrainsInTheCluster(request);

        return new IsActiveResponse
        {
            Result = result
        };
    }

    private void CheckRequestMetadata(ScaledObjectRef request)
    {
        if (!request.ScalerMetadata.ContainsKey(Limits) || !request.ScalerMetadata.ContainsKey(SiloNameFilter))
        {
            _logger.LogError($"{Limits} or {SiloNameFilter} not specified");
            throw new ArgumentException($"{Limits} and {SiloNameFilter} must be specified");
        }
    }

    private async Task<bool> AreTooManyGrainsInTheCluster(ScaledObjectRef request)
    {
        var siloNameFilter = request.ScalerMetadata[SiloNameFilter];
        var siloCount = await _orleansStatsService.GetActiveSiloCountAsync(siloNameFilter);

        foreach (var limit in GetLimits(request.ScalerMetadata))
        {
            var grainCount = await _orleansStatsService.GetGrainCountInClusterAsync(limit.GrainType);

            if (grainCount == 0 || siloCount == 0)
            {
                continue;
            }

            if (Convert.ToInt32(limit.Upperbound) <= grainCount / siloCount)
            {
                return true;
            }
        }

        return false;
    }

    private static IEnumerable<Limit> GetLimits(IReadOnlyDictionary<string, string> metadata)
    {
        return metadata[Limits]
            .Split(';')
            .Select(l =>
            {
                var splits = l.Split(":");
                return new Limit(splits.First(), int.Parse(splits.Last()));
            });
    }
}
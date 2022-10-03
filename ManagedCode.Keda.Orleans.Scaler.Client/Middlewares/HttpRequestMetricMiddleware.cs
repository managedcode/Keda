using ManagedCode.Keda.Orleans.Scaler.Metrics;
using Microsoft.AspNetCore.Http.Extensions;
using Orleans;

namespace ManagedCode.Keda.Orleans.Scaler.Client.Middlewares;

public class HttpRequestMetricMiddleware
{
    private readonly IClusterClient _clusterClient;
    private readonly ILogger<HttpRequestMetricMiddleware> _logger;
    private readonly RequestDelegate _next;

    public HttpRequestMetricMiddleware(ILogger<HttpRequestMetricMiddleware> logger, IClusterClient clusterClient, RequestDelegate next)
    {
        _logger = logger;
        _clusterClient = clusterClient;
        _next = next;
    }

    public async Task Invoke(HttpContext httpContext)
    {
        _clusterClient.GetGrain<IRequestTrackerGrain>(0).TrackRequest().Ignore();
        await _next(httpContext);
    }
}
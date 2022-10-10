using ManagedCode.Keda.Orleans.Interfaces;
using ManagedCode.TimeSeries.Summers;
using Microsoft.AspNetCore.Http.Extensions;
using Orleans;

namespace ManagedCode.Keda.Orleans.Scaler.Client.Middlewares;

public class HttpRequestMetricMiddleware : IDisposable
{
    private readonly RequestDelegate _next;
    private readonly IClusterClient _clusterClient;
    private readonly ILogger<HttpRequestMetricMiddleware> _logger;
    private PeriodicTimer _timer;
    private CancellationTokenSource _token;
    private readonly IntTimeSeriesSummer _summer = new(TimeSpan.FromSeconds(1), 30);

    public HttpRequestMetricMiddleware(ILogger<HttpRequestMetricMiddleware> logger, IClusterClient clusterClient, RequestDelegate next, CancellationTokenSource token)
    {
        _logger = logger;
        _clusterClient = clusterClient;
        _next = next;
        _token = token;
        _timer = new PeriodicTimer(TimeSpan.FromSeconds(10));
        Task.Run(RunTimer);
    }

    private async Task RunTimer()
    {
        while (await _timer.WaitForNextTickAsync(_token.Token))
        {
            var requests = _summer.Average();
            await _clusterClient.GetGrain<IRequestTrackerGrain>(0).TrackRequest((int)Math.Round(requests));
            _logger.LogInformation($"HOSTNAME:{Environment.GetEnvironmentVariable("HOSTNAME")};MachineName:{Environment.MachineName};\n  Requests Number: {requests}");
        }
    }

    public async Task Invoke(HttpContext httpContext)
    {
        _summer.Increment();
        await _next(httpContext);
    }

    public void Dispose()
    {
        _token.Cancel();
        _timer.Dispose();
    }
}

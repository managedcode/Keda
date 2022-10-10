using ManagedCode.Keda.Orleans.Interfaces;
using Microsoft.AspNetCore.SignalR;
using Orleans;

namespace ManagedCode.Keda.Orleans.Scaler.Client.Middlewares;

public class SignalRMonitorMiddleware : IHubFilter, IDisposable
{
    private readonly IClusterClient _clusterClient;

    private readonly ILogger<SignalRMonitorMiddleware> _logger;
    private PeriodicTimer _timer;
    private CancellationTokenSource _token;
    private volatile int _connections;
    
    public SignalRMonitorMiddleware(ILogger<SignalRMonitorMiddleware> logger, IClusterClient clusterClient)
    {
        _logger = logger;
        _clusterClient = clusterClient;
        _timer = new PeriodicTimer(TimeSpan.FromSeconds(10));
        _token = new();
        Task.Run(RunTimer);
    }


    private async Task RunTimer()
    {
        while (await _timer.WaitForNextTickAsync(_token.Token))
        {
            _logger.LogInformation($"HOSTNAME:{Environment.GetEnvironmentVariable("HOSTNAME")};MachineName:{Environment.MachineName};\n  Singal Connetions: {_connections}");
            await _clusterClient.GetGrain<ISignalRTrackerGrain>(0).TrackConnections(_connections);
        }
    }
    
    public Task OnConnectedAsync(HubLifetimeContext context, Func<HubLifetimeContext, Task> next)
    {
        Interlocked.Increment(ref _connections);
        return next(context);
    }

    public Task OnDisconnectedAsync(HubLifetimeContext context, Exception exception, Func<HubLifetimeContext, Exception, Task> next)
    {
        Interlocked.Decrement(ref _connections);
        return next(context, exception);
    }

    public void Dispose()
    {
        _token.Cancel();
        _timer.Dispose();
    }
}
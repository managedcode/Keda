using ManagedCode.Keda.Orleans.Interfaces;
using Microsoft.AspNetCore.SignalR;
using Orleans;

namespace ManagedCode.Keda.Orleans.Scaler.Client.Middlewares;

public class SignalRMonitorMiddleware : IHubFilter
{
    private readonly IClusterClient _clusterClient;

    private readonly ILogger<SignalRMonitorMiddleware> _logger;
    private static volatile int _connections;
    
    public SignalRMonitorMiddleware(ILogger<SignalRMonitorMiddleware> logger, IClusterClient clusterClient)
    {
        _logger = logger;
        _clusterClient = clusterClient;
    }



    
    public Task OnConnectedAsync(HubLifetimeContext context, Func<HubLifetimeContext, Task> next)
    {
        Interlocked.Increment(ref _connections);
        _clusterClient.GetGrain<ISignalRTrackerGrain>(0).TrackConnections(_connections).Ignore();
        return next(context);
    }

    public Task OnDisconnectedAsync(HubLifetimeContext context, Exception exception, Func<HubLifetimeContext, Exception, Task> next)
    {
        Interlocked.Decrement(ref _connections);
        _clusterClient.GetGrain<ISignalRTrackerGrain>(0).TrackConnections(_connections).Ignore();
        return next(context, exception);
    }
    
}
using ManagedCode.Keda.Orleans.Interfaces;
using Microsoft.AspNetCore.SignalR;
using Orleans;

namespace ManagedCode.Keda.Orleans.Scaler.Client.Middlewares;

public class SignalRMonitorMiddleware : IHubFilter
{
    private readonly IClusterClient _clusterClient;

    private readonly ILogger<SignalRMonitorMiddleware> _logger;

    public SignalRMonitorMiddleware(ILogger<SignalRMonitorMiddleware> logger, IClusterClient clusterClient)
    {
        _logger = logger;
        _clusterClient = clusterClient;
    }

    public Task OnConnectedAsync(HubLifetimeContext context, Func<HubLifetimeContext, Task> next)
    {
        _clusterClient.GetGrain<ISignalRTrackerGrain>(0).OnConnectedAsync().Ignore();
        return next(context);
    }

    public Task OnDisconnectedAsync(HubLifetimeContext context, Exception exception, Func<HubLifetimeContext, Exception, Task> next)
    {
        _clusterClient.GetGrain<ISignalRTrackerGrain>(0).OnDisconnectedAsync().Ignore();
        return next(context, exception);
    }
}
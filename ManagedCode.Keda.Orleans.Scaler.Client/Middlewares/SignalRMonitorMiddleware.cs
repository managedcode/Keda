using ManagedCode.Keda.Orleans.Interfaces;
using Microsoft.AspNetCore.Connections.Features;
using Microsoft.AspNetCore.SignalR;
using Orleans;

namespace ManagedCode.Keda.Orleans.Scaler.Client.Middlewares;

public class SignalRMonitorMiddleware : IHubFilter
{
    private static IClusterClient? _clusterClient;
    //private static readonly HashSet<string> ConnectedClients = new ();
    private static volatile int _count;
    private static Timer _timer = new Timer(Callback, null, TimeSpan.FromSeconds(10), TimeSpan.FromSeconds(10));

    private static void Callback(object? state)
    {
        if (_clusterClient?.IsInitialized == true)
        {
            _clusterClient.GetGrain<ISignalRTrackerGrain>(0).TrackConnections(Environment.MachineName, _count).Ignore();
        }
    }

    private readonly ILogger<SignalRMonitorMiddleware> _logger;
    

    public SignalRMonitorMiddleware(ILogger<SignalRMonitorMiddleware> logger, IClusterClient clusterClient)
    {
        _logger = logger;
        _clusterClient ??= clusterClient;
    }
    


    public Task OnConnectedAsync(HubLifetimeContext context, Func<HubLifetimeContext, Task> next)
    {
        Interlocked.Increment(ref _count);
        return next(context);
    }

    public Task OnDisconnectedAsync(HubLifetimeContext context, Exception exception, Func<HubLifetimeContext, Exception, Task> next)
    {
        Interlocked.Decrement(ref _count);
        return next(context, exception);
    }
    
}
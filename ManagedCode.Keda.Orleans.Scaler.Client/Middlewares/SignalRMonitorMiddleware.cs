using ManagedCode.Keda.Orleans.Interfaces;
using Microsoft.AspNetCore.Connections.Features;
using Microsoft.AspNetCore.SignalR;
using Orleans;

namespace ManagedCode.Keda.Orleans.Scaler.Client.Middlewares;

public class SignalRMonitorMiddleware : IHubFilter
{
    private static IClusterClient? _clusterClient;
    private static readonly HashSet<string> ConnectedClients = new ();
    private static Timer _timer = new Timer(Callback, null, TimeSpan.FromSeconds(10), TimeSpan.FromSeconds(10));

    private static void Callback(object? state)
    {
        _clusterClient?.GetGrain<ISignalRTrackerGrain>(0).TrackConnections(Environment.MachineName, ConnectedClients.Count).Ignore();
    }

    private readonly ILogger<SignalRMonitorMiddleware> _logger;
    

    public SignalRMonitorMiddleware(ILogger<SignalRMonitorMiddleware> logger, IClusterClient clusterClient)
    {
        _logger = logger;
        _clusterClient ??= clusterClient;
    }
    
    public Task OnConnectedAsync(HubLifetimeContext context, Func<HubLifetimeContext, Task> next)
    {
        lock (ConnectedClients)
        {
            ConnectedClients.Add(context.Context.ConnectionId);
        }
        
        return next(context);
    }

    public Task OnDisconnectedAsync(HubLifetimeContext context, Exception exception, Func<HubLifetimeContext, Exception, Task> next)
    {
        lock (ConnectedClients)
        {
            ConnectedClients.Remove(context.Context.ConnectionId);
        }
        
        return next(context, exception);
    }
    
}
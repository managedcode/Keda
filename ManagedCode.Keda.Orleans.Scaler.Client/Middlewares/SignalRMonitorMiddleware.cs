using ManagedCode.Keda.Orleans.Interfaces;
using Microsoft.AspNetCore.Connections.Features;
using Microsoft.AspNetCore.SignalR;
using Orleans;

namespace ManagedCode.Keda.Orleans.Scaler.Client.Middlewares;

public class SignalRMonitorMiddleware : IHubFilter
{
    private readonly IClusterClient _clusterClient;
    private static readonly HashSet<string> ConnectedClients = new ();
    private readonly ILogger<SignalRMonitorMiddleware> _logger;
    

    public SignalRMonitorMiddleware(ILogger<SignalRMonitorMiddleware> logger, IClusterClient clusterClient)
    {
        _logger = logger;
        _clusterClient = clusterClient;
    }
    
    public Task OnConnectedAsync(HubLifetimeContext context, Func<HubLifetimeContext, Task> next)
    {
        lock (ConnectedClients)
        {
            ConnectedClients.Add(context.Context.ConnectionId);
        }
        _clusterClient.GetGrain<ISignalRTrackerGrain>(0).TrackConnections(Environment.MachineName, ConnectedClients.Count).Ignore();
        return next(context);
    }

    public Task OnDisconnectedAsync(HubLifetimeContext context, Exception exception, Func<HubLifetimeContext, Exception, Task> next)
    {
        lock (ConnectedClients)
        {
            ConnectedClients.Remove(context.Context.ConnectionId);
        }
        
        _clusterClient.GetGrain<ISignalRTrackerGrain>(0).TrackConnections(Environment.MachineName, ConnectedClients.Count).Ignore();
        return next(context, exception);
    }
    
}
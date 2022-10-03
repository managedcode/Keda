using Orleans;
using Orleans.Concurrency;

namespace ManagedCode.Keda.Orleans.Scaler.Metrics.Grains;

[Reentrant]
public class SignalRTrackerGrain : Grain, ISignalRTrackerGrain
{
    private int _connections;
    
    public Task OnConnectedAsync()
    {
        _connections++;
        return Task.CompletedTask;
    }

    public Task OnDisconnectedAsync()
    {
        _connections--;
        return Task.CompletedTask;
    }

    public Task<int> GetConnections()
    {
        return Task.FromResult(_connections);
    }
}
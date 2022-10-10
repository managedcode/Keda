using ManagedCode.Keda.Orleans.Interfaces;
using ManagedCode.TimeSeries.Summers;
using Orleans;
using Orleans.Concurrency;

namespace ManagedCode.Keda.Orleans.Scaler.Metrics.Grains;

[Reentrant]
public class SignalRTrackerGrain : Grain, ISignalRTrackerGrain
{
    private volatile int _count;
    
    public Task OnConnectedAsync()
    {
        Interlocked.Increment(ref _count);
        return Task.CompletedTask;
    }

    public Task OnDisconnectedAsync()
    {
        Interlocked.Decrement(ref _count);
        return Task.CompletedTask;
    }

    public Task<int> GetConnections()
    {
        return Task.FromResult(_count);
    }
}
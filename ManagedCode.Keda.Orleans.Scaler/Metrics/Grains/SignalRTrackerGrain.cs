using ManagedCode.Keda.Orleans.Interfaces;
using ManagedCode.TimeSeries.Summers;
using Orleans;
using Orleans.Concurrency;

namespace ManagedCode.Keda.Orleans.Scaler.Metrics.Grains;

[Reentrant]
public class SignalRTrackerGrain : Grain, ISignalRTrackerGrain
{
    private readonly IntTimeSeriesSummer _summer = new(TimeSpan.FromSeconds(1), 30);
    
    public Task OnConnectedAsync()
    {
        _summer.Increment();
        return Task.CompletedTask;
    }

    public Task OnDisconnectedAsync()
    {
        _summer.Decrement();
        return Task.CompletedTask;
    }

    public Task TrackConnections(int count)
    {
        _summer.AddNewData(count);
        return Task.CompletedTask;
    }

    public Task<int> GetConnections()
    {
        var avg = _summer.Average();
        return Task.FromResult((int)Math.Round(avg));
    }
}
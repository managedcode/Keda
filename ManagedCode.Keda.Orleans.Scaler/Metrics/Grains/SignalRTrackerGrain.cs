using ManagedCode.Keda.Orleans.Interfaces;
using ManagedCode.TimeSeries;
using ManagedCode.TimeSeries.Abstractions;
using ManagedCode.TimeSeries.Summers;
using Orleans;
using Orleans.Concurrency;

namespace ManagedCode.Keda.Orleans.Scaler.Metrics.Grains;

[Reentrant]
public class SignalRTrackerGrain : Grain, ISignalRTrackerGrain
{
    private readonly IntGroupNumberTimeSeriesSummer _summer = new(TimeSpan.FromSeconds(1), 30, Strategy.Replace, true);

    public Task OnConnectedAsync(string host)
    {
        _summer.Increment(host);
        return Task.CompletedTask;
    }

    public Task OnDisconnectedAsync(string host)
    {
        _summer.Decrement(host);
        return Task.CompletedTask;
    }

    public Task TrackConnections(string host, int count)
    {
        _summer.AddNewData(host, count);
        return Task.CompletedTask;
    }

    public Task<int> GetConnections()
    {
        return Task.FromResult(_summer.Average());
    }

    public Task<Dictionary<string, int>> GetDetailedConnectionsCount()
    {
        return Task.FromResult(_summer.TimeSeries.ToDictionary(k => k.Key, v => v.Value.Average()));
    }
}
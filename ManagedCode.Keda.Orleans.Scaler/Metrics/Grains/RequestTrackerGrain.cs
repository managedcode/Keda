using ManagedCode.Keda.Orleans.Interfaces;
using ManagedCode.TimeSeries.Abstractions;
using ManagedCode.TimeSeries.Summers;
using Orleans;
using Orleans.Concurrency;

namespace ManagedCode.Keda.Orleans.Scaler.Metrics.Grains;

[Reentrant]
public class RequestTrackerGrain : Grain, IRequestTrackerGrain
{
    private readonly IntGroupNumberTimeSeriesSummer _summer = new(TimeSpan.FromSeconds(1), 30, Strategy.Replace, true);

    public Task TrackRequest(string host)
    {
        _summer.Increment(host);
        return Task.CompletedTask;
    }

    public Task TrackRequest(string host, int count)
    {
        _summer.AddNewData(host, count);
        return Task.CompletedTask;
    }

    public Task<int> GetRequestsCount()
    {
        return Task.FromResult(_summer.Average());
    }

    public Task<Dictionary<string, int>> GetDetailedRequestsCount()
    {
        return Task.FromResult(_summer.TimeSeries.ToDictionary(k => k.Key, v => v.Value.Average()));
    }
}
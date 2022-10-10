using ManagedCode.Keda.Orleans.Interfaces;
using ManagedCode.TimeSeries;
using ManagedCode.TimeSeries.Summers;
using Orleans;
using Orleans.Concurrency;

namespace ManagedCode.Keda.Orleans.Scaler.Metrics.Grains;

[Reentrant]
public class RequestTrackerGrain : Grain, IRequestTrackerGrain
{
    private readonly IntGroupTimeSeriesSummer _summer = new(TimeSpan.FromSeconds(1), 30, Strategy.Sum, true);

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
}
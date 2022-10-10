using ManagedCode.Keda.Orleans.Interfaces;
using ManagedCode.TimeSeries.Summers;
using Orleans;
using Orleans.Concurrency;

namespace ManagedCode.Keda.Orleans.Scaler.Metrics.Grains;

[Reentrant]
public class RequestTrackerGrain : Grain, IRequestTrackerGrain
{
    private readonly IntTimeSeriesSummer _summer = new(TimeSpan.FromSeconds(1), 30);

    public Task TrackRequest()
    {
        _summer.Increment();
        return Task.CompletedTask;
    }

    public Task TrackRequest(int count)
    {
        _summer.AddNewData(count);
        return Task.CompletedTask;
    }

    public Task<int> GetRequestsCount()
    {
        var avg = _summer.Average();
        return Task.FromResult((int)Math.Round(avg));
    }
}
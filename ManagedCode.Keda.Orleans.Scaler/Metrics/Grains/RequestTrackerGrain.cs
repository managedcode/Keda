using ManagedCode.Keda.Orleans.Interfaces;
using ManagedCode.TimeSeries.Summers;
using Orleans;
using Orleans.Concurrency;

namespace ManagedCode.Keda.Orleans.Scaler.Metrics.Grains;

[Reentrant]
public class RequestTrackerGrain : Grain, IRequestTrackerGrain
{
    private readonly IntTimeSeriesSummer _summer = new(TimeSpan.FromSeconds(1), 10);

    public Task TrackRequest()
    {
        _summer.Increment();
        return Task.CompletedTask;
    }

    public Task<int> GetRequestsCount()
    {
        if (_summer.Samples.Count == 0)
            return Task.FromResult(0);
        
        var avg = _summer.Samples.Average(a => a.Value);
        return Task.FromResult((int) Math.Round(avg));
    }
}
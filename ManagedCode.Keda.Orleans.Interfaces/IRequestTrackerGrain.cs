using Orleans;
using Orleans.Concurrency;

namespace ManagedCode.Keda.Orleans.Scaler.Metrics;

public interface IRequestTrackerGrain : IGrainWithIntegerKey
{
    [OneWay]
    Task TrackRequest();
    
    Task<int> GetRequestsCount();
}
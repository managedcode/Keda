using Orleans;
using Orleans.Concurrency;

namespace ManagedCode.Keda.Orleans.Interfaces;

public interface IRequestTrackerGrain : IGrainWithIntegerKey
{
    [OneWay]
    Task TrackRequest();
    
    Task<int> GetRequestsCount();
}
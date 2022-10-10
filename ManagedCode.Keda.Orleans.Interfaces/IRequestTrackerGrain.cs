using Orleans;
using Orleans.Concurrency;

namespace ManagedCode.Keda.Orleans.Interfaces;

public interface IRequestTrackerGrain : IGrainWithIntegerKey
{
    [OneWay]
    Task TrackRequest();
    
    [OneWay]
    Task TrackRequest(int count);
    
    Task<int> GetRequestsCount();
}
using Orleans;
using Orleans.Concurrency;

namespace ManagedCode.Keda.Orleans.Interfaces;

public interface IRequestTrackerGrain : IGrainWithIntegerKey
{
    [OneWay]
    Task TrackRequest(string host);
    
    [OneWay]
    Task TrackRequest(string host, int count);
    
    Task<int> GetRequestsCount();
}
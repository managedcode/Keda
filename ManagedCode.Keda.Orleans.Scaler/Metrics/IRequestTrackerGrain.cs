using Orleans;
using Orleans.Concurrency;

namespace ManagedCode.Keda.Orleans.Scaler.Client;

public interface IRequestTrackerGrain : IGrainWithIntegerKey
{
    [OneWay]
    Task TrackRequest();
    
    Task<int> GetRequestsCount();
}
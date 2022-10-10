using Orleans;
using Orleans.Concurrency;

namespace ManagedCode.Keda.Orleans.Interfaces;

public interface ISignalRTrackerGrain : IGrainWithIntegerKey
{
    [OneWay]
    Task OnConnectedAsync();
    
    [OneWay]
    Task OnDisconnectedAsync();
    
    [OneWay]
    Task TrackConnections(int count);

    Task<int> GetConnections();
}
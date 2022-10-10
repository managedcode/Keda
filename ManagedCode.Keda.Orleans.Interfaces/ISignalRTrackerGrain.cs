using Orleans;
using Orleans.Concurrency;

namespace ManagedCode.Keda.Orleans.Interfaces;

public interface ISignalRTrackerGrain : IGrainWithIntegerKey
{
    [OneWay]
    Task OnConnectedAsync(string host);
    
    [OneWay]
    Task OnDisconnectedAsync(string host);
    
    [OneWay]
    Task TrackConnections(string host, int count);

    Task<int> GetConnections();
}
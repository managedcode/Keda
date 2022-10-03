using Orleans;
using Orleans.Concurrency;

namespace ManagedCode.Keda.Orleans.Interfaces;

public interface ISignalRTrackerGrain : IGrainWithIntegerKey
{
    [OneWay]
    Task OnConnectedAsync();
    
    [OneWay]
    Task OnDisconnectedAsync();

    Task<int> GetConnections();
}
using Orleans;
using Orleans.Concurrency;

namespace ManagedCode.Keda.Orleans.Scaler.Metrics;

public interface ISignalRTrackerGrain : IGrainWithIntegerKey
{
    [OneWay]
    Task OnConnectedAsync();
    
    [OneWay]
    Task OnDisconnectedAsync();

    Task<int> GetConnections();
}
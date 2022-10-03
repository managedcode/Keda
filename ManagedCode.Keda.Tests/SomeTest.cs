using System.Net;
using FluentAssertions;
using ManagedCode.Keda.Orleans.Interfaces;
using ManagedCode.Keda.Tests.Cluster;
using Xunit;

namespace ManagedCode.Keda.Tests;

[Collection(nameof(TestClusterApplication))]
public class SomeTest 
{
    private readonly TestClusterApplication _testApp;

    public SomeTest(TestClusterApplication testApp)
    {
        _testApp = testApp;
    }
    
    [Fact]
    public async Task OneRequest()
    {
        var count = await _testApp.Cluster.Client.GetGrain<IRequestTrackerGrain>(0).GetRequestsCount();
        count.Should().Be(0);
        
        var request = await _testApp.CreateClient().GetAsync("/random");
        request.StatusCode.Should().Be(HttpStatusCode.OK);
        
        count = await _testApp.Cluster.Client.GetGrain<IRequestTrackerGrain>(0).GetRequestsCount();
        count.Should().Be(1);
    }

    [Fact]
    public async Task OneSignalR()
    {
        var count = await _testApp.Cluster.Client.GetGrain<ISignalRTrackerGrain>(0).GetConnections();
        count.Should().Be(0);
        
        
        var client = _testApp.CreateSignalRClient(nameof(TestHub));
        await client.StartAsync();
        
        count = await _testApp.Cluster.Client.GetGrain<ISignalRTrackerGrain>(0).GetConnections();
        count.Should().Be(1);

        await client.StopAsync();
        await client.DisposeAsync();

        await Task.Delay(TimeSpan.FromSeconds(1));
        
        count = await _testApp.Cluster.Client.GetGrain<ISignalRTrackerGrain>(0).GetConnections();
        count.Should().Be(0);
        
    }
}
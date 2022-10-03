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
        
        request = await _testApp.CreateClient().GetAsync("/api/scaling/requests");
        request.StatusCode.Should().Be(HttpStatusCode.OK);
        var content = await request.Content.ReadAsStringAsync();
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
        
        var request = await _testApp.CreateClient().GetAsync("/api/scaling/signalr");
        request.StatusCode.Should().Be(HttpStatusCode.OK);
        var content = await request.Content.ReadAsStringAsync();

        await client.StopAsync();
        await client.DisposeAsync();

        await Task.Delay(TimeSpan.FromSeconds(1));
        
        count = await _testApp.Cluster.Client.GetGrain<ISignalRTrackerGrain>(0).GetConnections();
        count.Should().Be(0);
        
    }
    
    [Fact]
    public async Task Orleans()
    {
        var request = await _testApp.CreateClient().GetAsync("/api/scaling/orleans");
        request.StatusCode.Should().Be(HttpStatusCode.OK);
        var content = await request.Content.ReadAsStringAsync();
    }
}
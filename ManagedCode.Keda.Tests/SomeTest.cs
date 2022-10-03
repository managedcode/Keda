using System.Net;
using FluentAssertions;
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
    public async Task RequestCont()
    {
        var request = await _testApp.CreateClient().GetAsync("/random");
        request.StatusCode.Should().Be(HttpStatusCode.OK);

        await Task.Delay(TimeSpan.FromSeconds(10));
        
        var count = await _testApp.Cluster.Client.GetGrain<ManagedCode.Keda.Orleans.Scaler.Metrics.IRequestTrackerGrain>(0).GetRequestsCount();
        count.Should().Be(1);
    }
}
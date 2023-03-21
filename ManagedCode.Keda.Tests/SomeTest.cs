using System.Net;
using FluentAssertions;
using ManagedCode.Keda.Orleans.Interfaces;
using ManagedCode.Keda.Orleans.Scaler.Client.Models;
using ManagedCode.Keda.Tests.Cluster;
using Newtonsoft.Json;
using Xunit;
using Xunit.Abstractions;

namespace ManagedCode.Keda.Tests;


public class SomeTest : TestClusterApplication
{
    private readonly ITestOutputHelper _outputHelper;

    public SomeTest(ITestOutputHelper outputHelper)
    {
        _outputHelper = outputHelper;
    }
    
    [Fact]
    public async Task OneRequest()
    {
        var baseCount = await this.Cluster.Client.GetGrain<IRequestTrackerGrain>(0).GetRequestsCount();
  
        
        var request = await this.CreateClient().GetAsync("/random");
        request.StatusCode.Should().Be(HttpStatusCode.OK);

        await Task.Delay(TimeSpan.FromSeconds(15));

        var count = await this.Cluster.Client.GetGrain<IRequestTrackerGrain>(0).GetRequestsCount();
        count.Should().Be(baseCount+1);
        
        request = await this.CreateClient().GetAsync("/api/scaling/requests");
        request.StatusCode.Should().Be(HttpStatusCode.OK);
        var content = await request.Content.ReadAsStringAsync();
    }

    [Fact]
    public async Task OneSignalR()
    {
        await Task.Delay(TimeSpan.FromSeconds(15));
        var count = await this.Cluster.Client.GetGrain<ISignalRTrackerGrain>(0).GetConnections();
        count.Should().Be(0);

        var client = this.CreateSignalRClient(nameof(TestHub));
        await client.StartAsync();

        await Task.Delay(TimeSpan.FromSeconds(15));

        count = await this.Cluster.Client.GetGrain<ISignalRTrackerGrain>(0).GetConnections();
        count.Should().Be(1);
        
        var request = await this.CreateClient().GetAsync("/api/scaling/signalr");
        request.StatusCode.Should().Be(HttpStatusCode.OK);
        var content = await request.Content.ReadAsStringAsync();

        await client.StopAsync();
        await client.DisposeAsync();

        await Task.Delay(TimeSpan.FromSeconds(15));
        
        count = await this.Cluster.Client.GetGrain<ISignalRTrackerGrain>(0).GetConnections();
        count.Should().Be(0);
        
    }
    
    [Fact]
    public async Task SignalR_500()
    {
        var count = await this.Cluster.Client.GetGrain<ISignalRTrackerGrain>(0).GetConnections();
        count.Should().Be(0);

        for (int i = 0; i < 500; i++)
        {
            var connection = this.CreateSignalRClient(nameof(TestHub));
            
            try
            {
                var local = i;
                await connection.StartAsync();
                if (i % 100 == 0)
                {
                    _outputHelper.WriteLine($"Connected:{local}");
                }
               
                //if (i % 50 != 0)
                {
                    _ = Task.Delay(TimeSpan.FromMinutes(2)).ContinueWith(async task =>
                    {
                        await connection.StopAsync();
                        await connection.DisposeAsync();
                        if (local % 100 == 0)
                        {
                            _outputHelper.WriteLine($"Disconnected:{local}");
                        }
                    
                    });
                }
                
            }
            catch (Exception e)
            {
                _outputHelper.WriteLine(e.Message);
            }

        }

        _outputHelper.WriteLine($"Wait.....");
        await Task.Delay(TimeSpan.FromMinutes(1));
        _outputHelper.WriteLine($"Wait..... one more");
        await Task.Delay(TimeSpan.FromMinutes(1));
        _outputHelper.WriteLine($"Wait..... and one more");
        await Task.Delay(TimeSpan.FromMinutes(1));

        
        count = await this.Cluster.Client.GetGrain<ISignalRTrackerGrain>(0).GetConnections();
        count.Should().Be(0);
    }
    
    [Fact]
    public async Task Orleans()
    {
        var iterations = 2000;
        for (int i = 0; i < iterations; i++)
        {
            await this.Cluster.Client.GetGrain<ISignalRTrackerGrain>(i).OnConnectedAsync("host");
            await this.Cluster.Client.GetGrain<IRequestTrackerGrain>(i).TrackRequest("host");
        }
        
        var request = await this.CreateClient().GetAsync("/api/scaling/orleans");
        request.StatusCode.Should().Be(HttpStatusCode.OK);
        var content = await request.Content.ReadAsStringAsync();
        var dto = JsonConvert.DeserializeObject<OrleansStats>(content);
        dto.GrainCount.Should().BeGreaterThan(iterations);

    }
}
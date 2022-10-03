using Microsoft.AspNetCore.SignalR;

namespace ManagedCode.Keda.Tests.Cluster;

public class TestHub : Hub
{
  public Task<int> DoTest()
  {
    return Task.FromResult(new Random().Next());
  }
}
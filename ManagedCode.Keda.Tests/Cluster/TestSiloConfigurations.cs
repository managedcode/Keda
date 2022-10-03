using ManagedCode.Keda.Orleans.Scaler.Extensions;
using Orleans;
using Orleans.Hosting;
using Orleans.TestingHost;

namespace ManagedCode.Keda.Tests.Cluster;

public class TestSiloConfigurations : ISiloConfigurator
{
    public void Configure(ISiloBuilder siloBuilder)
    {
        siloBuilder.UseScaler();
        siloBuilder.ConfigureServices(services =>
        {
            // services.AddSingleton<T, Impl>(...);
        });
        siloBuilder.ConfigureApplicationParts(parts => parts.AddFromAppDomain());
    }
}
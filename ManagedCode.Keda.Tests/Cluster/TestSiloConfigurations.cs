using ManagedCode.Keda.Orleans.Scaler.Client.Extensions;
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
            services.AddGrpcOrleansScaling();
            services.AddApiOrleansScaling();
        });
    }
}
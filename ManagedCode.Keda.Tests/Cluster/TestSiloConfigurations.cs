using ManagedCode.Keda.Orleans.Scaler.Client.Extensions;
using Microsoft.Extensions.Configuration;
using Orleans.TestingHost;

namespace ManagedCode.Keda.Tests.Cluster;

public class TestSiloConfigurations : ISiloConfigurator
{
    public void Configure(ISiloBuilder siloBuilder)
    {
        siloBuilder.ConfigureServices(services =>
        {
            services.AddGrpcOrleansScaling();
            services.AddApiOrleansScaling();
        });
    }
}

public class TestClientConfigurations : IClientBuilderConfigurator
{
    public void Configure(IConfiguration configuration, IClientBuilder clientBuilder)
    {
    }
}
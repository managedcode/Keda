using ManagedCode.Keda.Orleans.Interfaces;
using ManagedCode.Keda.Orleans.Scaler.Metrics.Grains;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Orleans;
using Orleans.Hosting;

namespace ManagedCode.Keda.Orleans.Scaler.Extensions;

public static class BuilderExtensions
{
    public static ISiloBuilder UseScaler(this ISiloBuilder siloBuilder)
    {
        return siloBuilder.ConfigureApplicationParts(parts =>
        {
            parts.AddFrameworkPart(typeof(IRequestTrackerGrain).Assembly);
            parts.AddFrameworkPart(typeof(RequestTrackerGrain).Assembly);
            
        });
    }

    public static IServiceCollection AddGrpcOrleansScaling(this IServiceCollection services)
    {
        services.AddSingleton<OrleansStatsService>();
        services.AddGrpc();

        return services;
    }

    public static IServiceCollection AddApiOrleansScaling(this IServiceCollection services)
    {
        services.AddSingleton<OrleansStatsService>();
        services.AddSingleton<ApiOrleansScalerService>();

        return services;
    }
}
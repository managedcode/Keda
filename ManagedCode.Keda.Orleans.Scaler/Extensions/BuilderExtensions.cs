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
        return siloBuilder.ConfigureApplicationParts(parts => parts.AddApplicationPart(typeof(RequestTrackerGrain).Assembly).WithReferences());
    }

    public static IServiceCollection AddGrpcOrleansScaling(this IServiceCollection services)
    {
        services.AddSingleton<GrainStatsService>();
        services.AddGrpc();

        return services;
    }

    public static IServiceCollection AddApiOrleansScaling(this IServiceCollection services)
    {
        services.AddSingleton<GrainStatsService>();
        services.AddSingleton<ApiScalerService>();

        return services;
    }

    public static IApplicationBuilder UseApiOrleansScaler(this IApplicationBuilder app)
    {
        app.UseEndpoints(endpoints => { endpoints.MapGrpcService<GrpcScalerService>(); });

        return app;
    }

    public static IEndpointRouteBuilder MapApiOrleansScaler(this IEndpointRouteBuilder endpoints, string apiRoute = "/api/scaling/stats")
    {
        endpoints.MapGet(apiRoute, ([FromServices] ApiScalerService scaler) => scaler.GetStatsAsync());

        return endpoints;
    }
}
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
    public static ISiloBuilder UseActivationShedding(this ISiloBuilder siloBuilder)
    {
        return siloBuilder.ConfigureApplicationParts(parts => parts.AddApplicationPart(typeof(RequestTrackerGrain).Assembly).WithReferences());
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

    public static IApplicationBuilder UseApiOrleansScaler(this IApplicationBuilder app)
    {
        app.UseEndpoints(endpoints => { endpoints.MapGrpcService<GrpcOrleansScalerService>(); });

        return app;
    }

    public static IEndpointRouteBuilder MapApiOrleansScaler(this IEndpointRouteBuilder endpoints, string apiRoute = "/api/scaling/orleans")
    {
        endpoints.MapGet(apiRoute, ([FromServices] ApiOrleansScalerService scaler) => scaler.GetOrleansStatsAsync());

        return endpoints;
    }
}
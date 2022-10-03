using ManagedCode.Keda.Orleans.Interfaces;
using ManagedCode.Keda.Orleans.Scaler.Client.Middlewares;
using ManagedCode.Keda.Orleans.Scaler.Client.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Orleans;

namespace ManagedCode.Keda.Orleans.Scaler.Client.Extensions;

public static class BuilderExtensions
{
    public static IApplicationBuilder UseScalerForRequest(this IApplicationBuilder app)
    {
        if (app == null)
        {
            throw new ArgumentNullException(nameof(app));
        }

        return app.UseMiddleware<HttpRequestMetricMiddleware>();
    }

    public static IApplicationBuilder UseApiOrleansScaler(this IApplicationBuilder app)
    {
        app.UseEndpoints(endpoints => { endpoints.MapGrpcService<GrpcOrleansScalerService>(); });

        return app;
    }

    public static IEndpointRouteBuilder MapApiRequestsScaler(this IEndpointRouteBuilder endpoints, string apiRoute = "/api/scaling/requests")
    {
        endpoints.MapGet(apiRoute, async ([FromServices] IClusterClient clusterClient) =>
        {
            var count = await clusterClient.GetGrain<IRequestTrackerGrain>(0).GetRequestsCount();

            return new RequestsStats(count);
        });

        return endpoints;
    }

    public static IEndpointRouteBuilder MapSignalRScaler(this IEndpointRouteBuilder endpoints, string apiRoute = "/api/scaling/signalr")
    {
        endpoints.MapGet(apiRoute, async ([FromServices] IClusterClient clusterClient) =>
        {
            var count = await clusterClient.GetGrain<ISignalRTrackerGrain>(0).GetConnections();
            return new SignalRStats(count);
        });

        return endpoints;
    }

    public static IEndpointRouteBuilder MapOrleansScaler(this IEndpointRouteBuilder endpoints, string apiRoute = "/api/scaling/orleans")
    {
        endpoints.MapGet(apiRoute, ([FromServices] ApiOrleansScalerService scaler) => scaler.GetOrleansStatsAsync());

        return endpoints;
    }
    
    public static IEndpointRouteBuilder MapRequestsScaler(this IEndpointRouteBuilder endpoints, string apiRoute = "/api/scaling/requests")
    {
        endpoints.MapGet(apiRoute, async ([FromServices] IClusterClient clusterClient) =>
        {
            var count = await clusterClient.GetGrain<IRequestTrackerGrain>(0).GetRequestsCount();
            return new RequestStats(count);
        });

        return endpoints;
    }

    public static void AddScalerForSignalR(this HubOptions options)
    {
        if (options == null)
        {
            throw new ArgumentNullException(nameof(options));
        }

        options.AddFilter<SignalRMonitorMiddleware>();
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
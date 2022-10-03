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

    public static void AddScalerForSignalR(this HubOptions options)
    {
        if (options == null)
        {
            throw new ArgumentNullException(nameof(options));
        }

        options.AddFilter<SignalRMonitorMiddleware>();
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
}
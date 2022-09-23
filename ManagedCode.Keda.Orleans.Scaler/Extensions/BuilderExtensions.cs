using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;

namespace ManagedCode.Keda.Orleans.Scaler.Extensions;

public static class BuilderExtensions
{
    public static IServiceCollection AddGRPcOrleansScaling(this IServiceCollection services)
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
        app.UseEndpoints(endpoints => { endpoints.MapGrpcService<GRPcScalerService>(); });

        return app;
    }

    public static IEndpointRouteBuilder MapApiOrleansScaler(this IEndpointRouteBuilder endpoints, string apiRoute = "/api/scaling/stats")
    {
        endpoints.MapGet(apiRoute, ([FromServices] ApiScalerService scaler) => scaler.GetStatsAsync());

        return endpoints;
    }
}
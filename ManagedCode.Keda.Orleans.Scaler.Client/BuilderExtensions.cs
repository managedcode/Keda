using ManagedCode.Keda.Orleans.Scaler.Client.Middlewares;
using Microsoft.AspNetCore.SignalR;

namespace ManagedCode.Keda.Orleans.Scaler.Client;

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
}
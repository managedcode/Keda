using ManagedCode.Keda.Orleans.Interfaces;
using ManagedCode.Keda.Orleans.Scaler.Metrics.Grains;
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
}
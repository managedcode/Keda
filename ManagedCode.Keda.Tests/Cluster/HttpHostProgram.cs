using ManagedCode.Keda.Orleans.Scaler.Client.Extensions;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Random = System.Random;

namespace ManagedCode.Keda.Tests.Cluster;

public class HttpHostProgram
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);
        builder.Services.AddControllers();
        var app = builder.Build();

        app.UseScalerForRequest();
        app.MapApiRequestsScaler();
        app.MapSignalRScaler();
        
        
        app.MapGet("/random", (a) => Task.FromResult(new Random().Next()));
        
        
        app.Run();
    }
}
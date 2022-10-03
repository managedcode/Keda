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

        builder.Services.AddGrpcOrleansScaling();
        builder.Services.AddApiOrleansScaling();
        
        builder.Services.AddSignalR().AddHubOptions<TestHub>(options =>
        {
            options.AddScalerForSignalR();
        });
        
        var app = builder.Build();
        
        //add Middlewares
        app.UseScalerForRequest();
        
        
        //map controllers
        app.MapApiRequestsScaler();
        app.MapSignalRScaler();
        app.MapOrleansScaler();
        
        
        
        app.MapGet("/random", (a) => Task.FromResult(new Random().Next()));
        app.MapHub<TestHub>(nameof(TestHub));

        app.Run();
    }
}
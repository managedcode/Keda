![img|300x200](https://raw.githubusercontent.com/managedcode/Keda/main/logo.png)


# Keda

[![.NET](https://github.com/managedcode/Keda/actions/workflows/dotnet.yml/badge.svg)](https://github.com/managedcode/Keda/actions/workflows/dotnet.yml)
[![Coverage Status](https://coveralls.io/repos/github/managedcode/Keda/badge.svg?branch=main)](https://coveralls.io/github/managedcode/Keda?branch=main)
[![nuget](https://github.com/managedcode/Keda/actions/workflows/nuget.yml/badge.svg?branch=main)](https://github.com/managedcode/Keda/actions/workflows/nuget.yml)
[![CodeQL](https://github.com/managedcode/Keda/actions/workflows/codeql-analysis.yml/badge.svg?branch=main)](https://github.com/managedcodeb/Keda/actions/workflows/codeql-analysis.yml)

Keda is a Kubernetes autoscaler that uses the KEDA autoscaling system to automatically 
scale applications based on metrics such as the number of active Grains in Orleans, the number of API requests, 
and the number of SignalR connections. This allows your .NET applications to handle increased workloads without manual intervention.

## Motivation
Kubernetes makes it easy to deploy and manage containerized applications at scale, 
but it can be challenging to ensure that your applications have the resources they need to handle sudden spikes in traffic. 
Keda solves this problem by using KEDA to automatically adjust the number of active Grains, 
as well as the number of API requests and SignalR connections, based on real-time metrics. 
This ensures that your applications have the resources they need to handle increased workloads without manual intervention.

## Getting Started
To use Keda, you will need to have a Kubernetes cluster with KEDA installed. 
Once you have that set up, you can deploy Keda using the provided YAML files.

- Install ``` ManagedCode.Keda.Orleans.Scaler``` package into your Silo project.
- Install ``` ManagedCode.Keda.Orleans.Scaler.Client``` package into your SingalR or WebAPI projects.
- Install ``` ManagedCode.Keda.Orleans.Scaler.Client``` package into your Scaler projects (let's call it orleans-scale).

## Usage
Keda is used by specifying the target metric and the desired range for that metric.
Keda will then automatically adjust the number of active Grains, as well as the number of API requests and SignalR connections, 
to keep the metric within the specified range.


You have to create one more service, and add this logic, let's call it "orleans-scale" :
``` cs
await Host.CreateDefaultBuilder(args)
    .ConfigureServices((context, services) =>
    {
        services.AddApiOrleansScaling();
        services.AddHealthChecks();
    })
    .ConfigureWebHostDefaults(webBuilder =>
    {
        webBuilder.Configure((_, app) =>
        {
            app.UseRouting();
            app.UseEndpoints(endpoints =>
            {
                endpoints.MapOrleansScaler();
                endpoints.MapApiRequestsScaler();
                endpoints.MapSignalRScaler();
                endpoints.MapHealthChecks("/health");
            });
        });
    })
    .UseConsoleLifetime()
    .RunConsoleAsync();
```

For add scaler for SignalR:
``` cs
services.AddSignalR()
    .AddHubOptions<SomeHub>(options =>
    {
        options.AddScalerForSignalR();
    });
```

For add scaler for WebAPI:
``` cs
app.UseScalerForRequest();
```

YAML configuration for Scaler project:

``` yaml
# Orleans Silo
apiVersion: keda.sh/v1alpha1
kind: ScaledObject
metadata:
    name: orleans-silo
spec:
  scaleTargetRef:
    name: orleans-silo
  minReplicaCount: 2
  maxReplicaCount: 6
  triggers:
    - type: metrics-api
      metadata:
        targetValue: "1500" # active grains per silo
        url: "http://orleans-scaler.#{namespace}#/api/scaling/orleans"
        valueLocation: 'grainCount'

---
# SignalR
apiVersion: keda.sh/v1alpha1
kind: ScaledObject
metadata:
  name: signalr-scaler-object
spec:
  scaleTargetRef:
    name: api-service
  minReplicaCount: 2
  maxReplicaCount: 10
  triggers:
    - type: metrics-api
      metadata:
        targetValue: "1000" # active connectinos per node
        url: "http://orleans-scaler.#{namespace}#/api/scaling/signalr"
        valueLocation: 'count'

---
# Web API
apiVersion: keda.sh/v1alpha1
kind: ScaledObject
metadata:
  name: http-scaler-object
spec:
  scaleTargetRef:
    name: api-service
  minReplicaCount: 2
  maxReplicaCount: 10
  triggers:
    - type: metrics-api
      metadata:
        targetValue: "500" # requests per second per node
        url: "http://orleans-scaler.#{namespace}#/api/scaling/requests"
        valueLocation: 'count'
        
```
This will cause Keda to automatically scale pods depend on load.

## Contributing
We welcome contributions to Keda! If you have an idea for a new feature or have found a bug, please open an issue on GitHub.

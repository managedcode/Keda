namespace ManagedCode.Keda.Orleans.Scaler.Client.Models;

public record Limit(string GrainType, int Upperbound);
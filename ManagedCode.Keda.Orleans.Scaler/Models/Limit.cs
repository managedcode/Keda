namespace ManagedCode.Keda.Orleans.Scaler.Models;

public record Limit(string GrainType, int Upperbound);
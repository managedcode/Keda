namespace ManagedCode.Keda.Orleans.Scaler.Client.Models;

public record OrleansStats(int GrainCount, Dictionary<string, int> Grains);
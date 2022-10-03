namespace ManagedCode.Keda.Orleans.Scaler.Models;

public record OrleansStats(int GrainCount, Dictionary<string, int> Grains);
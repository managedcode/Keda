namespace ManagedCode.Keda.Orleans.Scaler.Models;

public record ScalerStats(int GrainCount, int GrainsPerSilo, Dictionary<string, int> Grains);
using System.Threading.Tasks;

public interface ISimulationBackendAdapter
{
    bool IsAvailable { get; }

    Task<bool> StartSessionAsync(SimulationSessionContext context);

    Task<bool> RecordEventAsync(SimulationEventRecord record);

    Task<bool> CompleteSessionAsync(SimulationResultPayload result);
}

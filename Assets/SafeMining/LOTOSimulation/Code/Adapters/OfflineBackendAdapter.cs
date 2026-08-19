using System.Threading.Tasks;
using UnityEngine;

public class OfflineBackendAdapter : ISimulationBackendAdapter
{
    public bool IsAvailable => false;

    public Task<bool> StartSessionAsync(SimulationSessionContext context)
    {
        Debug.Log($"[OfflineBackendAdapter] StartSession queued locally sessionId={context?.sessionId}");
        return Task.FromResult(true);
    }

    public Task<bool> RecordEventAsync(SimulationEventRecord record)
    {
        Debug.Log($"[OfflineBackendAdapter] Event queued locally type={record?.eventType} objective={record?.objectiveId}");
        return Task.FromResult(true);
    }

    public Task<bool> CompleteSessionAsync(SimulationResultPayload result)
    {
        Debug.Log($"[OfflineBackendAdapter] CompleteSession queued locally score={result?.scoreTotal} approved={result?.approved}");
        return Task.FromResult(true);
    }
}

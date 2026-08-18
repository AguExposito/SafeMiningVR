using System.Threading.Tasks;
using UnityEngine;

public class SafeMiningBackendAdapter : ISimulationBackendAdapter
{
    private const string BaseUrlPlaceholder = "";

    public bool IsAvailable => !string.IsNullOrEmpty(BaseUrlPlaceholder);

    public Task<bool> StartSessionAsync(SimulationSessionContext context)
    {
        Debug.Log($"[SafeMiningBackendAdapter] StartSession stub — POST /api/vr/sessions/start sessionId={context?.sessionId}");
        return Task.FromResult(false);
    }

    public Task<bool> RecordEventAsync(SimulationEventRecord record)
    {
        Debug.Log($"[SafeMiningBackendAdapter] Event stub — POST /api/vr/sessions/{{id}}/events type={record?.eventType}");
        return Task.FromResult(false);
    }

    public Task<bool> CompleteSessionAsync(SimulationResultPayload result)
    {
        Debug.Log($"[SafeMiningBackendAdapter] Complete stub — POST /api/vr/sessions/{{id}}/complete score={result?.scoreTotal}");
        return Task.FromResult(false);
    }
}

using System;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;

public class SafeMiningBackendAdapter : ISimulationBackendAdapter
{
    public const string SyncErrorNetwork = "network";
    public const string SyncErrorServer = "server";
    public const string SyncErrorAuth = "auth";

    private const int RequestTimeoutSeconds = 15;

    public static string LastCompleteErrorKey { get; private set; }

    public bool IsAvailable
    {
        get
        {
            LotoVrLoginController login = LotoVrLoginController.Instance;
            if (login == null || login.Mode != LotoVrLoginController.ConnectionMode.Online || !login.IsLoggedIn)
            {
                return false;
            }

            return login.TryBuildResultEndpoint(LotoObjectiveIds.SimulationSceneId, out _);
        }
    }

    public Task<bool> StartSessionAsync(SimulationSessionContext context)
    {
        return Task.FromResult(true);
    }

    public Task<bool> RecordEventAsync(SimulationEventRecord record)
    {
        return Task.FromResult(true);
    }

    public async Task<bool> CompleteSessionAsync(SimulationResultPayload result)
    {
        LastCompleteErrorKey = null;
        if (result == null)
        {
            LastCompleteErrorKey = SyncErrorServer;
            return false;
        }

        LotoVrLoginController login = LotoVrLoginController.Instance;
        if (login == null || !login.IsLoggedIn)
        {
            LastCompleteErrorKey = SyncErrorAuth;
            return false;
        }

        string simulationId = string.IsNullOrEmpty(result.catalogSimulationId)
            ? LotoObjectiveIds.SimulationSceneId
            : result.catalogSimulationId;
        if (!login.TryBuildResultEndpoint(simulationId, out string endpoint))
        {
            LastCompleteErrorKey = SyncErrorNetwork;
            return false;
        }

        VrResultSubmitBody body = ResolveSubmitBody(result);
        string json = JsonUtility.ToJson(body);
        byte[] payload = Encoding.UTF8.GetBytes(json);
        using (UnityWebRequest request = new UnityWebRequest(endpoint, UnityWebRequest.kHttpVerbPOST))
        {
            request.uploadHandler = new UploadHandlerRaw(payload);
            request.downloadHandler = new DownloadHandlerBuffer();
            request.SetRequestHeader("Content-Type", "application/json");
            request.timeout = RequestTimeoutSeconds;
            login.ApplyBearer(request);
            UnityWebRequestAsyncOperation operation = request.SendWebRequest();
            while (!operation.isDone)
            {
                await Task.Yield();
            }

            if (request.result == UnityWebRequest.Result.ConnectionError || request.responseCode == 0)
            {
                LastCompleteErrorKey = SyncErrorNetwork;
                return false;
            }

            if (request.responseCode == 401 || request.responseCode == 403)
            {
                LastCompleteErrorKey = SyncErrorAuth;
                return false;
            }

            if (request.responseCode == 200 || request.responseCode == 202)
            {
                if (!string.IsNullOrEmpty(request.downloadHandler.text))
                {
                    VrResultSubmitResponse response = JsonUtility.FromJson<VrResultSubmitResponse>(request.downloadHandler.text);
                    if (response != null)
                    {
                        Debug.Log($"[SafeMiningBackendAdapter] Result accepted aprobado={response.aprobado} puntaje={response.puntajeFinal}");
                    }
                }

                return true;
            }

            LastCompleteErrorKey = SyncErrorServer;
            return false;
        }
    }

    private static VrResultSubmitBody ResolveSubmitBody(SimulationResultPayload result)
    {
        if (!string.IsNullOrEmpty(result.vrResultJson))
        {
            VrResultSubmitBody parsed = JsonUtility.FromJson<VrResultSubmitBody>(result.vrResultJson);
            if (parsed != null)
            {
                return parsed;
            }
        }

        LotoPhaseScoreTracker tracker = LotoPhaseScoreTracker.Instance;
        return LotoSessionScore.BuildVrSubmitBody(tracker, result.durationSeconds);
    }
}

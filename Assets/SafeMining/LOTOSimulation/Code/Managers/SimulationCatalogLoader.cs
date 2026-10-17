using System;
using System.IO;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;

public static class SimulationCatalogLoader
{
    public const string ResourceName = "simulations";
    public const string CacheFileName = "simulations.json";
    public const string SourceRemote = "remote";
    public const string SourceCache = "cache";
    public const string SourceResources = "resources";

    private const int RequestTimeoutSeconds = 8;

    private static bool scoringDeferred;

    public static SimulationCatalogFile Current { get; private set; }
    public static string LastSource { get; private set; }

    #region Public API

    public static void Initialize(string remoteUrl)
    {
        if (!TryApplyLocal(out string localSource))
        {
            Debug.LogWarning("[SimulationCatalog] No local catalog.");
        }
        else
        {
            Debug.Log($"[SimulationCatalog] loaded count={Count(Current)} source={localSource}");
        }

        if (!IsHttps(remoteUrl))
        {
            return;
        }

        _ = RefreshRemoteAsync(remoteUrl.Trim());
    }

    public static void ApplyDeferredScoring()
    {
        if (!scoringDeferred || Current == null || IsRunBlockingCatalog())
        {
            return;
        }

        scoringDeferred = false;
        ApplyScoring();
    }

    public static void ApplyToTracker(LotoPhaseScoreTracker tracker)
    {
        if (tracker == null || Current == null || IsRunBlockingCatalog())
        {
            if (IsRunBlockingCatalog())
            {
                scoringDeferred = true;
            }

            return;
        }

        SimulationCatalogEntry entry = Find(LotoObjectiveIds.SimulationSceneId);
        if (entry == null)
        {
            return;
        }

        tracker.ApplyCatalogEntry(entry);
    }

    public static SimulationCatalogEntry Find(string id)
    {
        if (Current == null || Current.simulations == null || string.IsNullOrEmpty(id))
        {
            return null;
        }

        for (int i = 0; i < Current.simulations.Length; i++)
        {
            SimulationCatalogEntry entry = Current.simulations[i];
            if (entry != null && entry.id == id)
            {
                return entry;
            }
        }

        return null;
    }

    #endregion

    #region Load

    private static bool TryApplyLocal(out string source)
    {
        source = string.Empty;
        SimulationCatalogFile cached = null;
        string cacheJson = ReadCacheJson();
        if (!string.IsNullOrEmpty(cacheJson))
        {
            cached = ParseCatalog(cacheJson);
        }

        string bundledJson = ReadBundledJson();
        SimulationCatalogFile bundled = ParseCatalog(bundledJson);
        bool cacheOk = cached != null;
        bool bundledOk = bundled != null;
        if (!cacheOk && !bundledOk)
        {
            return false;
        }

        bool useCache = cacheOk && (!bundledOk || cached.version >= bundled.version);
        if (useCache)
        {
            return CommitCatalog(cached, SourceCache, out source);
        }

        if (!CommitCatalog(bundled, SourceResources, out source))
        {
            return false;
        }

        if (cacheOk && !string.IsNullOrEmpty(bundledJson))
        {
            TryWriteCache(bundledJson);
        }

        return true;
    }

    private static async Task RefreshRemoteAsync(string remoteUrl)
    {
        using (UnityWebRequest request = UnityWebRequest.Get(remoteUrl))
        {
            request.timeout = RequestTimeoutSeconds;
            UnityWebRequestAsyncOperation operation = request.SendWebRequest();
            while (!operation.isDone)
            {
                await Task.Yield();
            }

            if (request.result != UnityWebRequest.Result.Success || request.responseCode != 200)
            {
                Debug.LogWarning($"[SimulationCatalog] Remote fetch failed code={request.responseCode}");
                return;
            }

            string json = request.downloadHandler.text;
            if (!TryApplyJson(json, SourceRemote))
            {
                Debug.LogWarning("[SimulationCatalog] Remote JSON rejected.");
                return;
            }

            TryWriteCache(json);

            Debug.Log($"[SimulationCatalog] loaded count={Count(Current)} source={SourceRemote}");
        }
    }

    private static bool TryApplyJson(string json, string source)
    {
        if (string.IsNullOrWhiteSpace(json))
        {
            return false;
        }

        SimulationCatalogFile parsed = ParseCatalog(json);
        if (parsed == null)
        {
            return false;
        }

        return CommitCatalog(parsed, source, out _);
    }

    private static bool CommitCatalog(SimulationCatalogFile parsed, string source, out string appliedSource)
    {
        appliedSource = string.Empty;
        if (parsed == null || FindEntry(parsed, LotoObjectiveIds.SimulationSceneId) == null)
        {
            return false;
        }

        Current = parsed;
        LastSource = source;
        appliedSource = source;
        if (IsRunBlockingCatalog())
        {
            scoringDeferred = true;
            return true;
        }

        scoringDeferred = false;
        ApplyScoring();
        return true;
    }

    private static void ApplyScoring()
    {
        SimulationCatalogEntry loto = Find(LotoObjectiveIds.SimulationSceneId);
        LotoObjectiveIds.ApplyCatalog(loto);
        if (LotoPhaseScoreTracker.Instance != null && !IsRunBlockingCatalog())
        {
            LotoPhaseScoreTracker.Instance.ApplyCatalogEntry(loto);
        }
    }

    private static SimulationCatalogFile ParseCatalog(string json)
    {
        if (string.IsNullOrWhiteSpace(json))
        {
            return null;
        }

        SimulationCatalogFile parsed = JsonUtility.FromJson<SimulationCatalogFile>(json);
        if (parsed == null || parsed.simulations == null || parsed.simulations.Length == 0)
        {
            return null;
        }

        if (FindEntry(parsed, LotoObjectiveIds.SimulationSceneId) == null)
        {
            return null;
        }

        return parsed;
    }

    private static SimulationCatalogEntry FindEntry(SimulationCatalogFile catalog, string id)
    {
        if (catalog == null || catalog.simulations == null || string.IsNullOrEmpty(id))
        {
            return null;
        }

        for (int i = 0; i < catalog.simulations.Length; i++)
        {
            SimulationCatalogEntry entry = catalog.simulations[i];
            if (entry != null && entry.id == id)
            {
                return entry;
            }
        }

        return null;
    }

    private static string ReadCacheJson()
    {
        string cachePath = CachePath();
        if (!File.Exists(cachePath))
        {
            return string.Empty;
        }

        try
        {
            return File.ReadAllText(cachePath);
        }
        catch (Exception exception)
        {
            Debug.LogWarning($"[SimulationCatalog] Cache read failed: {exception.Message}");
            return string.Empty;
        }
    }

    private static string ReadBundledJson()
    {
        TextAsset bundled = Resources.Load<TextAsset>(ResourceName);
        if (bundled == null)
        {
            return string.Empty;
        }

        return bundled.text;
    }

    private static void TryWriteCache(string json)
    {
        if (string.IsNullOrEmpty(json))
        {
            return;
        }

        try
        {
            File.WriteAllText(CachePath(), json);
        }
        catch (Exception exception)
        {
            Debug.LogWarning($"[SimulationCatalog] Cache write failed: {exception.Message}");
        }
    }

    private static bool IsRunBlockingCatalog()
    {
        LotoPhaseScoreTracker tracker = LotoPhaseScoreTracker.Instance;
        return tracker != null && tracker.BlocksCatalogApply;
    }

    #endregion

    #region Helpers

    private static string CachePath()
    {
        return Path.Combine(Application.persistentDataPath, CacheFileName);
    }

    private static int Count(SimulationCatalogFile catalog)
    {
        if (catalog == null || catalog.simulations == null)
        {
            return 0;
        }

        return catalog.simulations.Length;
    }

    private static bool IsHttps(string url)
    {
        if (string.IsNullOrWhiteSpace(url))
        {
            return false;
        }

        return url.Trim().StartsWith("https://", StringComparison.OrdinalIgnoreCase);
    }

    #endregion
}

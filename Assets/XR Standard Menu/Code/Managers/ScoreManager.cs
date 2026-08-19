using UnityEngine;
using NJ.Finder;
using UnityEngine.SceneManagement;
using System;
using System.Collections.Generic;
public class ScoreManager : MonoBehaviour
{
    private UIContentCarousel uIContentCarousel;
    [SerializeField] private SimScore simScore;
    private ref_MainMenu refMainMenu;
    private ref_Score refScore;
    private readonly List<GameObject> sceneScoreInstances = new List<GameObject>();
    private readonly Dictionary<int, ActiveSceneScoreState> activeSceneScores = new Dictionary<int, ActiveSceneScoreState>();
    private int lastInitializedSceneIndex = -1;
    private static bool s_shouldShowScoreOnReturn;
    private bool _lobbySetupDoneThisLoad;

    #region Initialization

    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    public void Initialize()
    {
        LoadSimScore();
        TrySetupLobbyScoreView();
    }

    #endregion

    #region Event Handlers

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // Si ControlCenter ya tiene otra instancia como principal, este ScoreManager
        // es el del duplicado que está siendo destruido; ignorar para evitar doble setup.
        if (ControlCenter.Instance != null && ControlCenter.Instance.scoreManager != this)
            return;

        if (scene.buildIndex != 0)
        {
            // Salimos del menú: resetear el flag para que al volver se haga el setup fresco.
            _lobbySetupDoneThisLoad = false;
            return;
        }

        // Volvimos al menú: solo ejecutar si Initialize() no lo hizo ya en este ciclo.
        TrySetupLobbyScoreView();
    }

    private void TrySetupLobbyScoreView()
    {
        if (_lobbySetupDoneThisLoad)
            return;

        _lobbySetupDoneThisLoad = true;
        SetupLobbyScoreView();
    }

    #endregion

    #region Public API

    public void LoadSceneByCarouselIndex(){
        if (uIContentCarousel == null)
        {
            uIContentCarousel = ComponentFinder.FindComponentInScene<UIContentCarousel>();
        }
        int targetBuildIndex = LotoSimulationConstants.LotoHubBuildIndex;
        if (uIContentCarousel != null)
        {
            int requestedIndex = uIContentCarousel.currentIndex + 1;
            if (IsBuildIndexLoadable(requestedIndex))
            {
                targetBuildIndex = requestedIndex;
            }
        }
        LoadSceneByBuildIndex(targetBuildIndex);
    }

    public async void LoadSceneByBuildIndex(int buildIndex)
    {
        if (!IsBuildIndexLoadable(buildIndex))
        {
            buildIndex = LotoSimulationConstants.LotoHubBuildIndex;
        }
        if (!IsBuildIndexLoadable(buildIndex) || ControlCenter.Instance == null || ControlCenter.Instance.loadManager == null)
        {
            return;
        }
        await ControlCenter.Instance.loadManager.LoadScene_VisualProgress(buildIndex);
    }

    private static bool IsBuildIndexLoadable(int buildIndex)
    {
        if (buildIndex < 0 || buildIndex >= SceneManager.sceneCountInBuildSettings)
        {
            return false;
        }

        string scenePath = SceneUtility.GetScenePathByBuildIndex(buildIndex);
        return !string.IsNullOrEmpty(scenePath);
    }

    public void InitializeScene(int index)
    {
        LoadSimScore();
        if (simScore == null || simScore.sceneScores == null || index < 0 || index >= simScore.sceneScores.Count)
        {
            return;
        }
        ActiveSceneScoreState state = new ActiveSceneScoreState
        {
            sceneScore = CloneSceneScore(simScore.sceneScores[index]),
            startTime = Time.time
        };
        activeSceneScores[index] = state;
        lastInitializedSceneIndex = index;
    }

    public void AchiveObjective(int index, bool value)
    {
        if (lastInitializedSceneIndex < 0)
        {
            return;
        }
        AchiveObjective(lastInitializedSceneIndex, index, value);
    }

    public void AchiveObjective(int sceneIndex, int objectiveIndex, bool value)
    {
        if (!activeSceneScores.TryGetValue(sceneIndex, out ActiveSceneScoreState state))
        {
            return;
        }
        if (state.sceneScore == null || state.sceneScore.objectives == null || objectiveIndex < 0 || objectiveIndex >= state.sceneScore.objectives.Count)
        {
            return;
        }
        state.sceneScore.objectives[objectiveIndex].isAchivied = value;
        activeSceneScores[sceneIndex] = state;
    }

    public void EndScene(int index)
    {
        EndScene(index, LotoSessionScore.CompletionCompleted);
    }

    public void EndScene(int index, string completionStatus)
    {
        if (!activeSceneScores.TryGetValue(index, out ActiveSceneScoreState state))
        {
            return;
        }
        float elapsedTime = Time.time - state.startTime;
        DateTime endDate = DateTime.Now;
        DateTime startDate = endDate.AddSeconds(-Mathf.Max(0f, elapsedTime));
        state.sceneScore.time = FormatElapsedTime(elapsedTime);
        state.sceneScore.date = endDate.ToString("dd/MM/yy");
        SimulationResultPayload payload = null;
        if (ControlCenter.Instance != null && ControlCenter.Instance.syncQueueManager != null)
        {
            payload = ControlCenter.Instance.syncQueueManager.EnqueueCompleteFromSceneScore(
                state.sceneScore,
                elapsedTime,
                startDate,
                endDate,
                completionStatus);
        }

        if (payload != null)
        {
            state.sceneScore.points = payload.scoreTotal;
            state.sceneScore.approbalRate = payload.scoreTotal;
        }
        else
        {
            state.sceneScore.points = CountAchieved(state.sceneScore.objectives);
            state.sceneScore.approbalRate = CalculateApprobalRate(state.sceneScore.objectives);
        }

        SceneScore savedScene = SaveSceneResult(state.sceneScore, index);
        if (ControlCenter.Instance != null && ControlCenter.Instance.ibManager != null)
        {
            _ = SendScoreToIAWithDebugAsync(savedScene, elapsedTime, startDate, endDate);
        }
        activeSceneScores.Remove(index);
        if (lastInitializedSceneIndex == index)
        {
            lastInitializedSceneIndex = -1;
        }
        s_shouldShowScoreOnReturn = true;
        if (SceneManager.GetActiveScene().buildIndex == 0)
        {
            SetupLobbyScoreView();
        }
    }

    public void RefreshLobbyScoreView()
    {
        _lobbySetupDoneThisLoad = false;
        SetupLobbyScoreView();
    }

    public bool TryGetActiveSceneElapsed(int sceneIndex, out float elapsedSeconds)
    {
        elapsedSeconds = 0f;
        if (!activeSceneScores.TryGetValue(sceneIndex, out ActiveSceneScoreState state))
        {
            return false;
        }

        elapsedSeconds = Mathf.Max(0f, Time.time - state.startTime);
        return true;
    }

    public SceneScore GetActiveSceneScore(int sceneIndex)
    {
        if (!activeSceneScores.TryGetValue(sceneIndex, out ActiveSceneScoreState state))
        {
            return null;
        }

        return state.sceneScore;
    }

    [ContextMenu("Debug/1 Initialize Random Scene Score")]
    public void DebugInitializeRandomSceneScore()
    {
        LoadSimScore();
        if (simScore == null || simScore.sceneScores == null || simScore.sceneScores.Count == 0)
        {
            return;
        }
        int randomSceneIndex = UnityEngine.Random.Range(0, simScore.sceneScores.Count);
        InitializeScene(randomSceneIndex);
    }

    [ContextMenu("Debug/2 Randomize Objectives On Active Scores")]
    public void DebugRandomizeObjectivesOnActiveScores()
    {
        if (activeSceneScores.Count == 0)
        {
            return;
        }
        List<int> activeKeys = new List<int>(activeSceneScores.Keys);
        for (int sceneKeyIndex = 0; sceneKeyIndex < activeKeys.Count; sceneKeyIndex++)
        {
            int sceneIndex = activeKeys[sceneKeyIndex];
            if (!activeSceneScores.TryGetValue(sceneIndex, out ActiveSceneScoreState state))
            {
                continue;
            }
            int objectiveCount = state.sceneScore != null && state.sceneScore.objectives != null ? state.sceneScore.objectives.Count : 0;
            for (int objectiveIndex = 0; objectiveIndex < objectiveCount; objectiveIndex++)
            {
                bool achieved = UnityEngine.Random.value >= 0.5f;
                AchiveObjective(sceneIndex, objectiveIndex, achieved);
            }
        }
    }

    [ContextMenu("Debug/3 End All Active Scene Scores")]
    public void DebugEndAllActiveSceneScores()
    {
        if (activeSceneScores.Count == 0)
        {
            return;
        }
        List<int> activeKeys = new List<int>(activeSceneScores.Keys);
        for (int i = 0; i < activeKeys.Count; i++)
        {
            EndScene(activeKeys[i]);
        }
    }

    #endregion

    #region Lobby Score View

    private void SetupLobbyScoreView()
    {
        if (SceneManager.GetActiveScene().buildIndex != 0)
        {
            return;
        }
        refMainMenu = ComponentFinder.FindComponentInScene<ref_MainMenu>();
        refScore = ComponentFinder.FindComponentInScene<ref_Score>();
        ScorePersistantData scoreData = ControlCenter.Instance.persistantDataManager.scorePersistantData;
        bool hasSavedScenes = scoreData != null && scoreData.sceneScores != null && scoreData.sceneScores.Count > 0;
        if (refMainMenu != null && refMainMenu.go_btn_score != null)
        {
            refMainMenu.go_btn_score.SetActive(hasSavedScenes);
        }
        if (!hasSavedScenes || refScore == null)
        {
            return;
        }
        BuildSceneScoreList(scoreData.sceneScores);
    }

    /// <summary>
    /// Devuelve <c>true</c> la primera vez que se consulta tras volver de una escena con score.
    /// Usar desde un script en la escena del menú (no en objetos persistentes).
    /// </summary>
    public static bool ConsumeAutoShowFlag()
    {
        if (!s_shouldShowScoreOnReturn)
            return false;

        s_shouldShowScoreOnReturn = false;
        return true;
    }

    private static List<SceneScore> DeduplicateScores(List<SceneScore> sceneScores)
    {
        List<SceneScore> result = new List<SceneScore>();
        for (int i = 0; i < sceneScores.Count; i++)
        {
            SceneScore s = sceneScores[i];
            bool alreadyAdded = false;
            for (int j = 0; j < result.Count; j++)
            {
                if (result[j].id == s.id && result[j].date == s.date && result[j].time == s.time)
                {
                    alreadyAdded = true;
                    break;
                }
            }
            if (!alreadyAdded)
                result.Add(s);
        }
        return result;
    }

    private void BuildSceneScoreList(List<SceneScore> sceneScores)
    {
        ClearSceneScoreInstances();
        if (refScore.sceneScoreContainer == null || refScore.sceneScorePrefab == null)
        {
            return;
        }
        sceneScores = DeduplicateScores(sceneScores);
        for (int i = 0; i < sceneScores.Count; i++)
        {
            SceneScore sceneScore = sceneScores[i];
            GameObject sceneScoreObject = Instantiate(refScore.sceneScorePrefab, refScore.sceneScoreContainer);
            sceneScoreInstances.Add(sceneScoreObject);
            ref_SceneScore sceneScoreRef = sceneScoreObject.GetComponent<ref_SceneScore>();
            if (sceneScoreRef == null)
            {
                continue;
            }
            if (sceneScoreRef.attempts != null)
            {
                sceneScoreRef.attempts.text = sceneScore.attempts.ToString();
            }
            if (sceneScoreRef.title != null)
            {
                sceneScoreRef.title.text = GetTranslatedText(sceneScore.id);
            }
            if (sceneScoreRef.date != null)
            {
                sceneScoreRef.date.text = sceneScore.date;
            }
            if (sceneScoreRef.time != null)
            {
                sceneScoreRef.time.text = sceneScore.time;
            }
            if (sceneScoreRef.points != null)
            {
                sceneScoreRef.points.text = sceneScore.points.ToString();
            }
            if (sceneScoreRef.view != null)
            {
                SceneScore capturedSceneScore = CloneSceneScore(sceneScore);
                sceneScoreRef.view.onClick.RemoveAllListeners();
                sceneScoreRef.view.onClick.AddListener(() => refScore.SetDetailScreen(capturedSceneScore));
            }
        }
    }

    private void ClearSceneScoreInstances()
    {
        for (int i = 0; i < sceneScoreInstances.Count; i++)
        {
            if (sceneScoreInstances[i] != null)
            {
                DestroyImmediate(sceneScoreInstances[i]);
            }
        }
        sceneScoreInstances.Clear();
    }

    #endregion

    #region Persistence

    private SceneScore SaveSceneResult(SceneScore result, int simScoreIndex)
    {
        ScorePersistantData scoreData = ControlCenter.Instance.persistantDataManager.scorePersistantData;
        if (scoreData.sceneScores == null)
        {
            scoreData.sceneScores = new List<SceneScore>();
        }

        // Deduplicar por POSICION en el array (no por id), para evitar entradas
        // duplicadas cuando los ids cambian entre sesiones o versiones del asset.
        while (scoreData.sceneScores.Count <= simScoreIndex)
        {
            scoreData.sceneScores.Add(new SceneScore());
        }

        SceneScore existing = scoreData.sceneScores[simScoreIndex];
        SceneScore updated = CloneSceneScore(result);
        updated.attempts = (existing != null && existing.attempts > 0 ? existing.attempts : 0) + 1;
        scoreData.sceneScores[simScoreIndex] = updated;

        ControlCenter.Instance.persistantDataManager.scorePersistantData = scoreData;
        ControlCenter.Instance.persistantDataManager.SaveScoreData();
        return updated;
    }

    [ContextMenu("Debug/Borrar scores guardados")]
    public void DebugClearPersistedScores()
    {
        ScorePersistantData empty = new ScorePersistantData
        {
            sceneScores = new List<SceneScore>()
        };
        ControlCenter.Instance.persistantDataManager.scorePersistantData = empty;
        _lobbySetupDoneThisLoad = false;
        SetupLobbyScoreView();
        Debug.Log("[ScoreManager] Scores persistentes borrados.");
    }

    #endregion

    #region Utilities

    private struct ActiveSceneScoreState
    {
        public SceneScore sceneScore;
        public float startTime;
    }

    private void LoadSimScore()
    {
        if (simScore == null)
        {
            simScore = Resources.Load<SimScore>("SimScore");
        }
    }

    private SceneScore CloneSceneScore(SceneScore source)
    {
        SceneScore clone = new SceneScore
        {
            id = source != null ? source.id : string.Empty,
            attempts = source != null ? source.attempts : 0,
            date = source != null ? source.date : string.Empty,
            time = source != null ? source.time : string.Empty,
            points = source != null ? source.points : 0,
            approbalRate = source != null ? Mathf.Clamp(source.approbalRate, 0, 100) : 0,
            objectives = new List<ObjectiveScore>()
        };
        if (source != null && source.objectives != null)
        {
            for (int i = 0; i < source.objectives.Count; i++)
            {
                ObjectiveScore objective = source.objectives[i];
                clone.objectives.Add(new ObjectiveScore
                {
                    id = objective.id,
                    isAchivied = objective.isAchivied
                });
            }
        }
        return clone;
    }

    private int CountAchieved(List<ObjectiveScore> objectives)
    {
        int achieved = 0;
        if (objectives == null)
        {
            return achieved;
        }
        for (int i = 0; i < objectives.Count; i++)
        {
            if (objectives[i].isAchivied)
            {
                achieved++;
            }
        }
        return achieved;
    }

    private int CalculateApprobalRate(List<ObjectiveScore> objectives)
    {
        if (objectives == null || objectives.Count == 0)
        {
            return 0;
        }
        int achieved = CountAchieved(objectives);
        float ratio = (float)achieved / objectives.Count;
        return Mathf.Clamp(Mathf.RoundToInt(ratio * 100f), 0, 100);
    }

    private string FormatElapsedTime(float elapsedSeconds)
    {
        if (elapsedSeconds < 0f)
        {
            elapsedSeconds = 0f;
        }
        TimeSpan elapsed = TimeSpan.FromSeconds(elapsedSeconds);
        return $"{elapsed.Minutes:00}:{elapsed.Seconds:00}";
    }

    private string GetTranslatedText(string translationId)
    {
        if (ControlCenter.Instance == null || ControlCenter.Instance.languageManager == null || string.IsNullOrEmpty(translationId))
        {
            return translationId;
        }
        string translated = ControlCenter.Instance.languageManager.GetText(translationId);
        return string.IsNullOrEmpty(translated) ? translationId : translated;
    }

    #endregion

    private async System.Threading.Tasks.Task SendScoreToIAWithDebugAsync(SceneScore savedScene, float elapsedTime, DateTime startDate, DateTime endDate)
    {
        bool sent = await ControlCenter.Instance.ibManager.SendScoreToCloud(savedScene, elapsedTime, startDate, endDate);
        if (sent)
        {
            Debug.Log($"[ScoreManager] Reporte enviado a IA. scene={savedScene?.id}, points={savedScene?.points}");
            return;
        }
        Debug.LogWarning($"[ScoreManager] Reporte NO enviado. scene={savedScene?.id}. Revisa logs de IBManager para el motivo.");
    }
}

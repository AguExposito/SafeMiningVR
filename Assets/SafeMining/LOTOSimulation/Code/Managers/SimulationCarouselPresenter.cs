using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SimulationCarouselPresenter : MonoBehaviour
{
    private const string PlayKey = "loto_vr_sim_play";
    private const string SimulateKey = "loto_vr_sim_score";

    private ref_SceneViewer sceneViewer;
    private bool configured;

    #region Lifecycle

    private void OnEnable()
    {
        SimulationCatalogLoader.CatalogChanged += HandleCatalogChanged;
        if (sceneViewer != null && sceneViewer.uIContentCarousel != null)
        {
            sceneViewer.uIContentCarousel.IndexChanged += HandleIndexChanged;
        }

        if (configured)
        {
            SyncCards();
        }
    }

    private void OnDisable()
    {
        SimulationCatalogLoader.CatalogChanged -= HandleCatalogChanged;
        if (sceneViewer != null && sceneViewer.uIContentCarousel != null)
        {
            sceneViewer.uIContentCarousel.IndexChanged -= HandleIndexChanged;
        }
    }

    #endregion

    #region Public API

    public void Configure(ref_SceneViewer viewer)
    {
        if (sceneViewer != null && sceneViewer.uIContentCarousel != null)
        {
            sceneViewer.uIContentCarousel.IndexChanged -= HandleIndexChanged;
        }

        sceneViewer = viewer;
        configured = viewer != null;
        SyncCards();
    }

    public bool IsCurrentPlayable()
    {
        SimulationCatalogEntry entry = CurrentEntry();
        return entry != null && entry.id == LotoObjectiveIds.SimulationSceneId;
    }

    public void SimulateCurrentScore()
    {
        SimulationCatalogEntry entry = CurrentEntry();
        if (entry == null || IsCurrentPlayable())
        {
            return;
        }

        VrResultSubmitBody body = BuildSimulatedBody(entry);
        SimulationResultPayload payload = new SimulationResultPayload
        {
            catalogSimulationId = entry.id,
            loginUsername = CurrentUsername(),
            scoreTotal = Mathf.Clamp(Mathf.RoundToInt((body.epp + body.limpieza + body.loto) / 3f), 0, 100),
            approved = false,
            durationSeconds = body.duracion_total,
            finishedAtUtc = DateTime.UtcNow.ToString("o"),
            completionStatus = LotoSessionScore.CompletionCompleted,
            vrResultJson = JsonUtility.ToJson(body)
        };
        payload.approved = payload.scoreTotal >= LotoObjectiveIds.EffectiveApprovalThresholdPercent;
        SetSummary(CurrentCard(), body.epp + " / " + body.limpieza + " / " + body.loto);
        if (ControlCenter.Instance != null && ControlCenter.Instance.syncQueueManager != null)
        {
            ControlCenter.Instance.syncQueueManager.EnqueueComplete(payload);
        }

        Debug.Log("[SimulationCarousel] Simulated score id=" + entry.id + " epp=" + body.epp + " limpieza=" + body.limpieza + " loto=" + body.loto);
    }

    #endregion

    #region Cards

    private void HandleCatalogChanged()
    {
        SyncCards();
    }

    private void HandleIndexChanged(int index)
    {
        ApplyActionButton();
    }

    private void SyncCards()
    {
        if (!configured || sceneViewer == null || sceneViewer.uIContentCarousel == null)
        {
            return;
        }

        UIContentCarousel carousel = sceneViewer.uIContentCarousel;
        if (carousel.gridLayoutGroup == null)
        {
            carousel.gridLayoutGroup = carousel.GetComponentInChildren<GridLayoutGroup>(true);
        }

        if (carousel.gridLayoutGroup == null)
        {
            return;
        }

        Transform root = carousel.gridLayoutGroup.transform;
        SimulationCatalogFile catalog = SimulationCatalogLoader.Current;
        if (catalog == null || catalog.simulations == null || catalog.simulations.Length == 0 || root.childCount == 0)
        {
            ApplyActionButton();
            return;
        }

        Transform template = root.GetChild(0);
        while (root.childCount < catalog.simulations.Length)
        {
            Instantiate(template.gameObject, root);
        }

        while (root.childCount > catalog.simulations.Length)
        {
            DestroyImmediate(root.GetChild(root.childCount - 1).gameObject);
        }

        for (int i = 0; i < catalog.simulations.Length; i++)
        {
            SimulationCatalogEntry entry = catalog.simulations[i];
            Transform card = root.GetChild(i);
            card.gameObject.SetActive(true);
            string title = entry != null && !string.IsNullOrEmpty(entry.name) ? entry.name : string.Empty;
            SetTitle(card, title);
            if (entry != null && entry.id == LotoObjectiveIds.SimulationSceneId)
            {
                continue;
            }

            int objectiveCount = entry != null && entry.objectives != null ? entry.objectives.Length : 0;
            string objectiveLabel = objectiveCount == 1 ? " objetivo" : " objetivos";
            SetSummary(card, objectiveCount + objectiveLabel);
        }

        carousel.RefreshPageCount();
        ApplyActionButton();
    }

    private void ApplyActionButton()
    {
        if (sceneViewer == null || sceneViewer.go_btn_select == null)
        {
            return;
        }

        TMP_Text label = sceneViewer.go_btn_select.GetComponentInChildren<TMP_Text>(true);
        if (label == null)
        {
            return;
        }

        bool playable = IsCurrentPlayable();
        label.text = Translate(playable ? PlayKey : SimulateKey);
    }

    private SimulationCatalogEntry CurrentEntry()
    {
        SimulationCatalogFile catalog = SimulationCatalogLoader.Current;
        if (catalog == null || catalog.simulations == null || sceneViewer == null || sceneViewer.uIContentCarousel == null)
        {
            return null;
        }

        int index = sceneViewer.uIContentCarousel.currentIndex;
        if (index < 0 || index >= catalog.simulations.Length)
        {
            return null;
        }

        return catalog.simulations[index];
    }

    private Transform CurrentCard()
    {
        if (sceneViewer == null || sceneViewer.uIContentCarousel == null || sceneViewer.uIContentCarousel.gridLayoutGroup == null)
        {
            return null;
        }

        Transform root = sceneViewer.uIContentCarousel.gridLayoutGroup.transform;
        int index = sceneViewer.uIContentCarousel.currentIndex;
        if (index < 0 || index >= root.childCount)
        {
            return null;
        }

        return root.GetChild(index);
    }

    #endregion

    #region Score

    private static VrResultSubmitBody BuildSimulatedBody(SimulationCatalogEntry entry)
    {
        VrResultSubmitBody body = new VrResultSubmitBody();
        bool wrotePhase = false;
        if (entry.phases != null)
        {
            for (int i = 0; i < entry.phases.Length; i++)
            {
                SimulationCatalogPhase phase = entry.phases[i];
                if (phase == null || string.IsNullOrEmpty(phase.apiKey))
                {
                    continue;
                }

                float score = UnityEngine.Random.Range(60, 101);
                int seconds = UnityEngine.Random.Range(20, 91);
                if (phase.apiKey == "epp")
                {
                    body.epp = score;
                    body.duracion_epp = seconds;
                    wrotePhase = true;
                }
                else if (phase.apiKey == "limpieza")
                {
                    body.limpieza = score;
                    body.duracion_limpieza = seconds;
                    wrotePhase = true;
                }
                else if (phase.apiKey == "loto")
                {
                    body.loto = score;
                    body.duracion_loto = seconds;
                    wrotePhase = true;
                }
            }
        }

        if (!wrotePhase)
        {
            body.epp = UnityEngine.Random.Range(60, 101);
            body.duracion_epp = UnityEngine.Random.Range(20, 91);
        }

        body.duracion_total = body.duracion_epp + body.duracion_limpieza + body.duracion_loto;
        return body;
    }

    private static string CurrentUsername()
    {
        if (LotoVrLoginController.Instance == null || LotoVrLoginController.Instance.Session == null)
        {
            return string.Empty;
        }

        return LotoVrLoginController.Instance.Session.username ?? string.Empty;
    }

    #endregion

    #region Labels

    private static void SetTitle(Transform card, string value)
    {
        SetNamedText(card, "SceneTitle", value);
    }

    private static void SetSummary(Transform card, string value)
    {
        SetNamedText(card, "SceneSummary", value);
    }

    private static void SetNamedText(Transform card, string childName, string value)
    {
        if (card == null)
        {
            return;
        }

        TMP_Text[] texts = card.GetComponentsInChildren<TMP_Text>(true);
        for (int i = 0; i < texts.Length; i++)
        {
            if (texts[i] != null && texts[i].name == childName)
            {
                texts[i].text = value;
            }
        }
    }

    private static string Translate(string key)
    {
        if (ControlCenter.Instance == null || ControlCenter.Instance.languageManager == null)
        {
            return key == PlayKey ? "JUGAR" : "SIMULAR SCORE";
        }

        string translated = ControlCenter.Instance.languageManager.GetText(key);
        if (string.IsNullOrEmpty(translated) || translated == key)
        {
            return key == PlayKey ? "JUGAR" : "SIMULAR SCORE";
        }

        return translated;
    }

    #endregion
}

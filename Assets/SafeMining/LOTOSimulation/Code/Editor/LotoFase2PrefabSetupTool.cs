using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

public static class LotoFase2PrefabSetupTool
{
    private const string YardPrefabPath = "Assets/SafeMining/LOTOSimulation/Prefabs/PREF_S2_InspectionYard.prefab";
    private const string HudPrefabPath = "Assets/SafeMining/LOTOSimulation/Prefabs/PREF_LotoPlayerHUD.prefab";
    private const string BriefingPrefabPath = "Assets/SafeMining/LOTOSimulation/Prefabs/PREF_LotoBriefingPanel.prefab";
    private const string ObjectiveRowPrefabPath = "Assets/SafeMining/LOTOSimulation/Prefabs/PREF_LotoObjectiveRow.prefab";
    private const float HudCanvasScale = 0.001f;

    [MenuItem("SafeMining/LOTO/Apply Fase 2 Brand and Loc Prefab Setup")]
    public static void ApplyFase2Setup()
    {
        FixS2YardLabels();
        ApplyLockTagLabels();
        ApplyHudBranding();
        ApplyBriefingBranding();
        ApplyObjectiveRowBranding();
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        Debug.Log("[LOTO Fase2] Prefab brand/loc setup complete.");
    }

    #region S2 Labels

    private static void FixS2YardLabels()
    {
        GameObject root = PrefabUtility.LoadPrefabContents(YardPrefabPath);
        LotoWorldLabel[] labels = root.GetComponentsInChildren<LotoWorldLabel>(true);
        for (int i = 0; i < labels.Length; i++)
        {
            LotoWorldLabel worldLabel = labels[i];
            SerializedObject serialized = new SerializedObject(worldLabel);
            string currentKey = serialized.FindProperty("locKey").stringValue;
            if (currentKey == "loto_s2_exit")
            {
                continue;
            }

            string hazardKey = ResolveS2StaticLabelKey(worldLabel.transform, root.transform);
            if (string.IsNullOrEmpty(hazardKey))
            {
                worldLabel.gameObject.SetActive(false);
                continue;
            }

            serialized.FindProperty("locKey").stringValue = hazardKey;
            serialized.ApplyModifiedPropertiesWithoutUndo();
            worldLabel.gameObject.SetActive(true);
        }

        EnsureS2ExtinguisherLabel(root);

        PrefabUtility.SaveAsPrefabAsset(root, YardPrefabPath);
        PrefabUtility.UnloadPrefabContents(root);
    }

    private static void EnsureS2ExtinguisherLabel(GameObject yardRoot)
    {
        Transform extinguisher = FindDeepChild(yardRoot.transform, "Hazard_Extinguisher");
        if (extinguisher == null)
        {
            return;
        }

        LotoWorldLabel existing = extinguisher.GetComponentInChildren<LotoWorldLabel>(true);
        if (existing != null && existing.transform.IsChildOf(extinguisher))
        {
            SerializedObject serializedExisting = new SerializedObject(existing);
            serializedExisting.FindProperty("locKey").stringValue = "loto_s2_label_extinguisher";
            serializedExisting.ApplyModifiedPropertiesWithoutUndo();
            existing.gameObject.SetActive(true);
            return;
        }

        EnsureWorldLabel(extinguisher, "loto_s2_label_extinguisher", new Vector3(0f, 0.9f, 0f));
    }

    private static string ResolveS2StaticLabelKey(Transform labelTransform, Transform yardRoot)
    {
        Transform walk = labelTransform;
        while (walk != null && walk != yardRoot)
        {
            if (walk.name == "Hazard_Extinguisher")
            {
                return "loto_s2_label_extinguisher";
            }

            if (walk.name == "Hazard_Signage")
            {
                return "loto_s2_label_signage";
            }

            walk = walk.parent;
        }

        return null;
    }

    private static void SetWorldLabelKey(GameObject root, string path, string locKey)
    {
        Transform target = root.transform.Find(path);
        if (target == null)
        {
            Debug.LogWarning("[LOTO Fase2] Missing label path: " + path);
            return;
        }

        LotoWorldLabel worldLabel = target.GetComponent<LotoWorldLabel>();
        if (worldLabel == null)
        {
            Debug.LogWarning("[LOTO Fase2] Missing LotoWorldLabel on: " + path);
            return;
        }

        SerializedObject serialized = new SerializedObject(worldLabel);
        serialized.FindProperty("locKey").stringValue = locKey;
        serialized.ApplyModifiedPropertiesWithoutUndo();
        target.gameObject.SetActive(true);
    }

    #endregion

    #region S3 Lock Tag Labels

    private static void ApplyLockTagLabels()
    {
        EnsureWorldLabelPrefab("Assets/SafeMining/LOTOSimulation/Prefabs/Interactables/PREF_LotoLock_Personal.prefab", "loto_lock_personal");
        EnsureWorldLabelPrefab("Assets/SafeMining/LOTOSimulation/Prefabs/Interactables/PREF_LotoLock_Visitor.prefab", "loto_lock_visitor");
        EnsureWorldLabelPrefab("Assets/SafeMining/LOTOSimulation/Prefabs/Interactables/PREF_LotoLock_Group.prefab", "loto_lock_group");
        EnsureWorldLabelPrefab("Assets/SafeMining/LOTOSimulation/Prefabs/Interactables/PREF_LotoTag_Danger.prefab", "loto_tag_danger");
        EnsureWorldLabelPrefab("Assets/SafeMining/LOTOSimulation/Prefabs/Interactables/PREF_LotoTag_Inspected.prefab", "loto_tag_inspected");
        EnsureWorldLabelPrefab("Assets/SafeMining/LOTOSimulation/Prefabs/Interactables/PREF_LotoTag_Operational.prefab", "loto_tag_operational");
        EnsureWorldLabelPrefab("Assets/SafeMining/LOTOSimulation/Prefabs/Interactables/PREF_LotoTag_Maintenance.prefab", "loto_tag_maintenance");
    }

    private static void EnsureWorldLabelPrefab(string prefabPath, string locKey)
    {
        GameObject root = PrefabUtility.LoadPrefabContents(prefabPath);
        EnsureWorldLabel(root.transform, locKey, new Vector3(0f, 0.22f, 0f));
        PrefabUtility.SaveAsPrefabAsset(root, prefabPath);
        PrefabUtility.UnloadPrefabContents(root);
    }

    private static void EnsureWorldLabel(Transform root, string locKey, Vector3 localPosition)
    {
        Transform labelTransform = root.Find("Label");
        GameObject labelObject;
        if (labelTransform == null)
        {
            labelObject = new GameObject("Label");
            labelObject.transform.SetParent(root, false);
            labelObject.transform.localPosition = localPosition;
            labelObject.transform.localRotation = Quaternion.Euler(0f, 180f, 0f);
            labelObject.transform.localScale = Vector3.one;
        }
        else
        {
            labelObject = labelTransform.gameObject;
        }

        TextMeshPro textMesh = labelObject.GetComponent<TextMeshPro>();
        if (textMesh == null)
        {
            textMesh = labelObject.AddComponent<TextMeshPro>();
        }

        if (labelObject.GetComponent<MeshRenderer>() == null)
        {
            labelObject.AddComponent<MeshRenderer>();
        }

        TMP_FontAsset font = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(
            "Assets/TextMesh Pro/Resources/Fonts & Materials/LiberationSans SDF.asset");
        if (font != null)
        {
            textMesh.font = font;
        }

        textMesh.fontSize = 1.4f;
        textMesh.alignment = TextAlignmentOptions.Center;
        textMesh.rectTransform.sizeDelta = new Vector2(0.9f, 0.2f);
        textMesh.enableWordWrapping = true;
        textMesh.text = locKey;

        LotoWorldLabel worldLabel = labelObject.GetComponent<LotoWorldLabel>();
        if (worldLabel == null)
        {
            worldLabel = labelObject.AddComponent<LotoWorldLabel>();
        }

        SerializedObject serialized = new SerializedObject(worldLabel);
        serialized.FindProperty("label").objectReferenceValue = textMesh;
        serialized.FindProperty("locKey").stringValue = locKey;
        serialized.ApplyModifiedPropertiesWithoutUndo();
        labelObject.SetActive(true);
    }

    #endregion

    #region Branding

    private static void ApplyHudBranding()
    {
        GameObject root = PrefabUtility.LoadPrefabContents(HudPrefabPath);
        root.transform.localScale = new Vector3(HudCanvasScale, HudCanvasScale, HudCanvasScale);

        RemoveBrandApplicator(root, "ActiveObjectiveTitle");
        RemoveBrandApplicator(root, "ScenarioIndicator");
        RemoveBrandApplicator(root, "TimerText");
        ResetHudTextColor(root, "ActiveObjectiveTitle");
        ResetHudTextColor(root, "ScenarioIndicator");
        ResetHudTextColor(root, "TimerText");

        EnsureBrandColor(root, "Background", BrandColorRole.Primary);
        EnsureBrandColor(root, "SessionSummaryPanel", BrandColorRole.Primary);
        EnsureBrandColor(root, "ExitConfirmPanel", BrandColorRole.Primary);
        EnsureBrandColor(root, "MenuButton", BrandColorRole.Secondary);
        EnsureBrandColor(root, "SessionSummaryContinue", BrandColorRole.Secondary);
        EnsureBrandColor(root, "ExitConfirmYes", BrandColorRole.Secondary);
        EnsureBrandColor(root, "ExitConfirmNo", BrandColorRole.Secondary);
        EnsureBrandColor(root, "TimerFill", BrandColorRole.Secondary);

        PrefabUtility.SaveAsPrefabAsset(root, HudPrefabPath);
        PrefabUtility.UnloadPrefabContents(root);
    }

    private static void ApplyBriefingBranding()
    {
        GameObject root = PrefabUtility.LoadPrefabContents(BriefingPrefabPath);
        root.transform.localScale = new Vector3(HudCanvasScale, HudCanvasScale, HudCanvasScale);
        EnsureBrandColor(root, "Background", BrandColorRole.Primary);
        EnsureBrandColor(root, "BeginButton", BrandColorRole.Secondary);
        PrefabUtility.SaveAsPrefabAsset(root, BriefingPrefabPath);
        PrefabUtility.UnloadPrefabContents(root);
    }

    private static void ApplyObjectiveRowBranding()
    {
        GameObject root = PrefabUtility.LoadPrefabContents(ObjectiveRowPrefabPath);
        ref_LotoObjectiveRow row = root.GetComponent<ref_LotoObjectiveRow>();
        if (row != null && row.title != null)
        {
            RectTransform titleRect = row.title.rectTransform;
            titleRect.anchorMin = new Vector2(0f, 0f);
            titleRect.anchorMax = new Vector2(1f, 1f);
            titleRect.offsetMin = new Vector2(56f, 0f);
            titleRect.offsetMax = new Vector2(-8f, 0f);
        }

        if (row != null && row.statusIcon != null)
        {
            BrandApplicator applicator = row.statusIcon.GetComponent<BrandApplicator>();
            if (applicator == null)
            {
                applicator = row.statusIcon.gameObject.AddComponent<BrandApplicator>();
            }

            SerializedObject serialized = new SerializedObject(applicator);
            serialized.FindProperty("graphicRole").enumValueIndex = (int)BrandGraphicRole.None;
            serialized.FindProperty("colorRole").enumValueIndex = (int)BrandColorRole.Secondary;
            serialized.FindProperty("textRole").enumValueIndex = (int)BrandTextRole.None;
            serialized.FindProperty("applyColorToGraphic").boolValue = true;
            serialized.ApplyModifiedPropertiesWithoutUndo();
            applicator.Apply();
        }

        PrefabUtility.SaveAsPrefabAsset(root, ObjectiveRowPrefabPath);
        PrefabUtility.UnloadPrefabContents(root);
    }

    private static void EnsureBrandColor(GameObject root, string childName, BrandColorRole role)
    {
        Transform target = FindDeepChild(root.transform, childName);
        if (target == null)
        {
            Debug.LogWarning("[LOTO Fase2] Missing branding target: " + childName);
            return;
        }

        BrandApplicator applicator = target.GetComponent<BrandApplicator>();
        if (applicator == null)
        {
            applicator = target.gameObject.AddComponent<BrandApplicator>();
        }

        SerializedObject serialized = new SerializedObject(applicator);
        serialized.FindProperty("graphicRole").enumValueIndex = (int)BrandGraphicRole.None;
        serialized.FindProperty("colorRole").enumValueIndex = (int)role;
        serialized.FindProperty("textRole").enumValueIndex = (int)BrandTextRole.None;
        serialized.FindProperty("applyColorToGraphic").boolValue = true;
        serialized.ApplyModifiedPropertiesWithoutUndo();
        applicator.Apply();
    }

    private static void ResetHudTextColor(GameObject root, string childName)
    {
        Transform target = FindDeepChild(root.transform, childName);
        if (target == null)
        {
            return;
        }

        TMP_Text text = target.GetComponent<TMP_Text>();
        if (text != null)
        {
            text.color = new Color(0.976f, 0.98f, 0.984f, 1f);
        }
    }

    private static void RemoveBrandApplicator(GameObject root, string childName)
    {
        Transform target = FindDeepChild(root.transform, childName);
        if (target == null)
        {
            return;
        }

        BrandApplicator applicator = target.GetComponent<BrandApplicator>();
        if (applicator != null)
        {
            Object.DestroyImmediate(applicator);
        }
    }

    private static Transform FindDeepChild(Transform parent, string childName)
    {
        if (parent.name == childName)
        {
            return parent;
        }

        for (int i = 0; i < parent.childCount; i++)
        {
            Transform found = FindDeepChild(parent.GetChild(i), childName);
            if (found != null)
            {
                return found;
            }
        }

        return null;
    }

    #endregion
}

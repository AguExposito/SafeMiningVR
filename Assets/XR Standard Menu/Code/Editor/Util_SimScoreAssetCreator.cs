using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class Util_SimScoreAssetCreator : EditorWindow
{
    private const string AssetFolderPath = "Assets/Resources";
    private const string AssetPath = "Assets/Resources/SimScore.asset";
    private int sceneCount = 3;
    private int objectivesPerScene = 5;
    private string sceneIdPrefix = "scene_score_";
    private string objectiveIdPrefix = "objective_";

    [MenuItem("XR Standard Menu/Score/SimScore Creator")]
    public static void OpenWindow()
    {
        Util_SimScoreAssetCreator window = GetWindow<Util_SimScoreAssetCreator>("SimScore Creator");
        window.minSize = new Vector2(360f, 260f);
    }

    private void OnGUI()
    {
        GUILayout.Space(8f);
        sceneCount = EditorGUILayout.IntField("Scene Count", sceneCount);
        objectivesPerScene = EditorGUILayout.IntField("Objectives Per Scene", objectivesPerScene);
        sceneIdPrefix = EditorGUILayout.TextField("Scene ID Prefix", sceneIdPrefix);
        objectiveIdPrefix = EditorGUILayout.TextField("Objective ID Prefix", objectiveIdPrefix);
        GUILayout.Space(12f);
        if (GUILayout.Button("Create Or Replace SimScore Asset", GUILayout.Height(34f)))
        {
            CreateOrReplaceAsset();
        }
        GUILayout.Space(8f);
        if (GUILayout.Button("Select SimScore Asset", GUILayout.Height(26f)))
        {
            SelectAsset();
        }
    }

    private void CreateOrReplaceAsset()
    {
        sceneCount = Mathf.Max(0, sceneCount);
        objectivesPerScene = Mathf.Max(0, objectivesPerScene);
        EnsureResourceFolder();
        SimScore asset = AssetDatabase.LoadAssetAtPath<SimScore>(AssetPath);
        if (asset == null)
        {
            asset = ScriptableObject.CreateInstance<SimScore>();
            AssetDatabase.CreateAsset(asset, AssetPath);
        }
        asset.sceneScores = BuildSceneScores();
        EditorUtility.SetDirty(asset);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        Selection.activeObject = asset;
        EditorGUIUtility.PingObject(asset);
    }

    private List<SceneScore> BuildSceneScores()
    {
        List<SceneScore> scenes = new List<SceneScore>();
        for (int sceneIndex = 0; sceneIndex < sceneCount; sceneIndex++)
        {
            SceneScore sceneScore = new SceneScore
            {
                id = sceneIdPrefix + sceneIndex,
                attempts = 0,
                date = string.Empty,
                time = "00:00",
                points = 0,
                objectives = new List<ObjectiveScore>()
            };
            for (int objectiveIndex = 0; objectiveIndex < objectivesPerScene; objectiveIndex++)
            {
                sceneScore.objectives.Add(new ObjectiveScore
                {
                    id = objectiveIdPrefix + sceneIndex + "_" + objectiveIndex,
                    isAchivied = false
                });
            }
            scenes.Add(sceneScore);
        }
        return scenes;
    }

    private static void EnsureResourceFolder()
    {
        if (AssetDatabase.IsValidFolder(AssetFolderPath))
        {
            return;
        }
        AssetDatabase.CreateFolder("Assets", "Resources");
    }

    private static void SelectAsset()
    {
        SimScore asset = AssetDatabase.LoadAssetAtPath<SimScore>(AssetPath);
        if (asset == null)
        {
            return;
        }
        Selection.activeObject = asset;
        EditorGUIUtility.PingObject(asset);
    }
}

using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "SimScore", menuName = "XR Standard Menu/SimScore")]
public class SimScore : ScriptableObject
{
    public List<SceneScore> sceneScores = new List<SceneScore>();
}

[Serializable]
public class ScorePersistantData
{
    public List<SceneScore> sceneScores = new List<SceneScore>();
}

[Serializable]
public class SceneScore
{
    public string id;
    public int attempts;
    public string date;
    public string time;
    public int points;
    public int approbalRate;
    public List<ObjectiveScore> objectives = new List<ObjectiveScore>();
}

[Serializable]
public class ObjectiveScore
{
    public string id;
    public bool isAchivied;
}

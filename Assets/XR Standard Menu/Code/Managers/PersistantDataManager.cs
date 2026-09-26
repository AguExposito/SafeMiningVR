using System;
using System.IO;
using UnityEngine;
using NJ.Observable;

public class PersistantDataManager : MonoBehaviour
{
    private GeneralPersistantData _generalPersistantData;
    private ScorePersistantData _scorePersistantData;
    private UserPersistantData _userPersistantData;
    private string generalPersistantDataPath;
    private string scorePersistantDataPath;
    private string userPersistantDataPath;
    
    public GeneralPersistantData generalPersistantData{
        get{
            if(_generalPersistantData == null){
                _generalPersistantData = GetData<GeneralPersistantData>(generalPersistantDataPath);
            }
            return _generalPersistantData;
        }
        set{
            _generalPersistantData = value;
            SetData(_generalPersistantData, generalPersistantDataPath);
            SaveGeneralData();
        }
    }
    public ScorePersistantData scorePersistantData
    {
        get
        {
            if (_scorePersistantData == null)
            {
                _scorePersistantData = GetData<ScorePersistantData>(scorePersistantDataPath);
                if (_scorePersistantData.sceneScores == null)
                {
                    _scorePersistantData.sceneScores = new System.Collections.Generic.List<SceneScore>();
                }
                if (_scorePersistantData.lotoAttempts == null)
                {
                    _scorePersistantData.lotoAttempts = new System.Collections.Generic.List<LotoAttemptRecord>();
                }
            }
            return _scorePersistantData;
        }
        set
        {
            _scorePersistantData = value ?? new ScorePersistantData();
            if (_scorePersistantData.sceneScores == null)
            {
                _scorePersistantData.sceneScores = new System.Collections.Generic.List<SceneScore>();
            }
            if (_scorePersistantData.lotoAttempts == null)
            {
                _scorePersistantData.lotoAttempts = new System.Collections.Generic.List<LotoAttemptRecord>();
            }
            SetData(_scorePersistantData, scorePersistantDataPath);
            SaveScoreData();
        }
    }
    public UserPersistantData userPersistantData
    {
        get
        {
            if (_userPersistantData == null)
            {
                _userPersistantData = GetData<UserPersistantData>(userPersistantDataPath);
            }
            return _userPersistantData;
        }
        set
        {
            _userPersistantData = value ?? new UserPersistantData();
            SetData(_userPersistantData, userPersistantDataPath);
            SaveUserData();
        }
    }

    public T GetData<T>(string path) where T : class, new()
    {
        try
        {
            if (File.Exists(path))
            {
                string data = File.ReadAllText(path);
                T dataObject = JsonUtility.FromJson<T>(data);
                if (dataObject == null)
                {
                    dataObject = new T();
                }
                return dataObject;
            }
            else
            {
                // Si el archivo no existe, crear uno nuevo
                T dataObject = new T();
                SetData(dataObject, path);
                return dataObject;
            }
        }
        catch (Exception e)
        {
            Debug.LogError($"Error loading data from {path}: {e.Message}");
            return new T();
        }
    }
    public void SetData<T>(T dataObject, string path) where T : class
    {
        try
        {
            string data = JsonUtility.ToJson(dataObject, true); // prettyPrint = true para formato legible
            File.WriteAllText(path, data);
            Debug.Log($"Data saved successfully to: {path}");
        }
        catch (Exception e)
        {
            Debug.LogError($"Error saving data to {path}: {e.Message}");
        }
    }
    void Awake()
    {
        generalPersistantDataPath = Path.Combine(Application.persistentDataPath, "GeneralPersistantData.json");
        scorePersistantDataPath = Path.Combine(Application.persistentDataPath, "ScorePersistantData.json");
        userPersistantDataPath = Path.Combine(Application.persistentDataPath, "UserPersistantData.json");
    }
    
    public void Initialize(){}
    
    public void SaveGeneralData()
    {
        if (_generalPersistantData != null)
        {
            SetData(_generalPersistantData, generalPersistantDataPath);
        }
    }
    public void SaveScoreData()
    {
        if (_scorePersistantData != null)
        {
            SetData(_scorePersistantData, scorePersistantDataPath);
        }
    }
    public void SaveUserData()
    {
        if (_userPersistantData != null)
        {
            SetData(_userPersistantData, userPersistantDataPath);
        }
    }
}
public class GeneralPersistantData
{
    public float volume = 0.5f;
    public bool enableSound = true;
    public int language = 0; // 0 = Spanish, 1 = English
}
public class UserPersistantData
{
    public string activationCode = string.Empty;
    public string iaCloudUrl = string.Empty;
    public string participantID = string.Empty;
    public string resourceID = string.Empty;
    public string firstName = string.Empty;
    public string lastName = string.Empty;
    public string userPin = string.Empty;
    public bool isLicenseValid;
    public bool isPlayerLoggedIn;
}
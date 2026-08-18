using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;
using XRStandardMenu;
using NJ.Finder;
public class ControlCenter : MonoBehaviour
{
    public static ControlCenter Instance;
    #region Managers
    private protected PersistantDataManager _persistantDataManager;
    public PersistantDataManager persistantDataManager
    {
        get
        {
            if (_persistantDataManager == null)
            {
                _persistantDataManager = gameObject.AddComponent<PersistantDataManager>();
                return _persistantDataManager;
            }
            else return _persistantDataManager;
        }
        set
        {
            persistantDataManager = value;
        }
    }
    private protected LoadManager _loadManager;
    public LoadManager loadManager
    {
        get
        {
            if (_loadManager == null)
            {
                _loadManager = gameObject.AddComponent<LoadManager>();
                return _loadManager;
            }
            else return _loadManager;
        }
        set => _loadManager = value;
    }
    private protected LanguageManager _languageManager;
    public LanguageManager languageManager
    {
        get
        {
            if (_languageManager == null)
            {
                _languageManager = gameObject != null ? gameObject.AddComponent<LanguageManager>() : null;
                return _languageManager;
            }
            else return _languageManager;
        }
        set => _languageManager = value;
    }
    private protected SoundManager _soundManager;
    public SoundManager soundManager
    {
        get
        {
            if (_soundManager == null)
            {
                _soundManager = gameObject.AddComponent<SoundManager>();
                return _soundManager;
            }
            else return _soundManager;
        }
        set => _soundManager = value;
    }
    private protected ScoreManager _scoreManager;
    public ScoreManager scoreManager
    {
        get
        {
            if (_scoreManager == null)
            {
                _scoreManager = gameObject.GetComponent<ScoreManager>();
                if (_scoreManager == null)
                {
                    Debug.LogError("[ControlCenter] Falta ScoreManager en el mismo GameObject de ControlCenter. Debe agregarse manualmente en la escena.");
                }
            }
            return _scoreManager;
        }
        set => _scoreManager = value;
    }
    private protected IBManager _ibManager;
    public IBManager ibManager
    {
        get
        {
            if (_ibManager == null)
            {
                _ibManager = gameObject.GetComponent<IBManager>();
                if (_ibManager == null)
                {
                    Debug.LogError("[ControlCenter] Falta IBManager en el mismo GameObject de ControlCenter. Debe agregarse manualmente en la escena.");
                }
            }
            return _ibManager;
        }
        set => _ibManager = value;
    }
    private protected SyncQueueManager _syncQueueManager;
    public SyncQueueManager syncQueueManager
    {
        get
        {
            if (_syncQueueManager == null)
            {
                _syncQueueManager = gameObject.GetComponent<SyncQueueManager>();
                if (_syncQueueManager == null)
                {
                    _syncQueueManager = gameObject.AddComponent<SyncQueueManager>();
                }
            }
            return _syncQueueManager;
        }
        set => _syncQueueManager = value;
    }
    private protected SimulationSessionManager _simulationSessionManager;
    public SimulationSessionManager simulationSessionManager
    {
        get
        {
            if (_simulationSessionManager == null)
            {
                _simulationSessionManager = gameObject.GetComponent<SimulationSessionManager>();
                if (_simulationSessionManager == null)
                {
                    _simulationSessionManager = gameObject.AddComponent<SimulationSessionManager>();
                }
            }
            return _simulationSessionManager;
        }
        set => _simulationSessionManager = value;
    }
    private protected SimulationOrchestratorManager _simulationOrchestratorManager;
    public SimulationOrchestratorManager simulationOrchestratorManager
    {
        get
        {
            if (_simulationOrchestratorManager == null)
            {
                _simulationOrchestratorManager = gameObject.GetComponent<SimulationOrchestratorManager>();
                if (_simulationOrchestratorManager == null)
                {
                    _simulationOrchestratorManager = gameObject.AddComponent<SimulationOrchestratorManager>();
                }
            }
            return _simulationOrchestratorManager;
        }
        set => _simulationOrchestratorManager = value;
    }
    #endregion

    [Header("Branding")]
    [Tooltip("Optional override. If empty, loads Resources/BrandConfig.")]
    [SerializeField] private BrandConfig brandConfig;

    public bool test;
    void Awake()
    {
        if (Instance == null)
        {
            DontDestroyOnLoad(this);
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
            return;
        }
        _scoreManager = gameObject.GetComponent<ScoreManager>();
        _ibManager = gameObject.GetComponent<IBManager>();
        InitializeBranding();
        InitializeSBDs();
        InitializeManagers();
    }

    void InitializeBranding()
    {
        if (brandConfig != null)
        {
            Brand.SetOverride(brandConfig);
        }
        BrandApplicator[] applicators = FindObjectsByType<BrandApplicator>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        for (int i = 0; i < applicators.Length; i++)
        {
            applicators[i].Apply();
        }
        ApplyBrandToNamedLogos();
    }

    void ApplyBrandToNamedLogos()
    {
        if (Brand.Logo == null)
        {
            return;
        }
        Image[] images = FindObjectsByType<Image>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        for (int i = 0; i < images.Length; i++)
        {
            if (images[i] != null && images[i].gameObject.name == "Logo")
            {
                images[i].sprite = Brand.Logo;
                images[i].preserveAspect = true;
            }
        }
    }
    void InitializeSBDs()
    {
        SBD_Audio sBD_Audio = ComponentFinder.FindComponentInScene<SBD_Audio>();
        SBD_Prefab sBD_Prefab = ComponentFinder.FindComponentInScene<SBD_Prefab>();
        SBD_Images sBD_Images = ComponentFinder.FindComponentInScene<SBD_Images>();
        sBD_Audio.Initialize();
        sBD_Prefab.Initialize();
        sBD_Images.Initialize();
    }
    void InitializeManagers()
    {
        persistantDataManager.Initialize();
        syncQueueManager.Initialize();
        simulationSessionManager.Initialize();
        simulationOrchestratorManager.Initialize();
        loadManager.Initialize();
        languageManager.Initialize();
        soundManager.Initialize();
        if (ibManager != null)
        {
            ibManager.Initialize();
        }
        if (scoreManager != null)
        {
            scoreManager.Initialize();
        }
    }
}

using UnityEngine;
using UnityEngine.SceneManagement;

[DisallowMultipleComponent]
public class LotoAmbientMusicController : MonoBehaviour
{
    public static LotoAmbientMusicController Instance { get; private set; }

    public const string MenuSceneName = "StandardMainMenu";
    public const string Scenario1SceneName = "VR-LOTO-S1";
    public const string Scenario2SceneName = "VR-LOTO-S2S3";

    [Header("Ambient Clips")]
    [SerializeField] private AudioClip menuClip;
    [SerializeField] private AudioClip scenario1Clip;
    [SerializeField] private AudioClip scenario2Clip;

    [Header("Playback")]
    [SerializeField] [Range(0f, 1f)] private float ambientVolumeScale = 1f;

    private AudioSource audioSource;
    private float volume = 0.5f;
    private bool enableSound = true;
    private string activeSceneName = string.Empty;

    #region Lifecycle

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(this);
            return;
        }

        Instance = this;
        EnsureAudioSource();
    }

    private void OnEnable()
    {
        SceneManager.sceneLoaded += HandleSceneLoaded;
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= HandleSceneLoaded;
    }

    private void Start()
    {
        ResolveClipsFromSbd();
        ApplyForActiveScene();
    }

    private void OnDestroy()
    {
        if (Instance == this)
        {
            Instance = null;
        }
    }

    #endregion

    #region Public API

    public void Initialize(float initialVolume, bool soundEnabled)
    {
        EnsureAudioSource();
        ResolveClipsFromSbd();
        ApplyVolume(initialVolume, soundEnabled);
        ApplyForActiveScene();
    }

    public void ApplyVolume(float nextVolume, bool soundEnabled)
    {
        volume = Mathf.Clamp01(nextVolume);
        enableSound = soundEnabled;
        if (audioSource == null)
        {
            return;
        }

        audioSource.volume = enableSound ? volume * ambientVolumeScale : 0f;
    }

    public void ApplyForActiveScene()
    {
        ApplyForScene(SceneManager.GetActiveScene().name);
    }

    #endregion

    #region Scene Music

    private void HandleSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        ApplyForScene(scene.name);
    }

    private void ApplyForScene(string sceneName)
    {
        EnsureAudioSource();
        AudioClip nextClip = ResolveClipForScene(sceneName);
        if (nextClip == null)
        {
            if (audioSource.isPlaying)
            {
                audioSource.Stop();
            }

            audioSource.clip = null;
            activeSceneName = sceneName;
            return;
        }

        if (activeSceneName == sceneName && audioSource.clip == nextClip && audioSource.isPlaying)
        {
            return;
        }

        audioSource.clip = nextClip;
        audioSource.loop = true;
        audioSource.volume = enableSound ? volume * ambientVolumeScale : 0f;
        if (audioSource.isPlaying)
        {
            audioSource.Stop();
        }

        audioSource.Play();
        activeSceneName = sceneName;
    }

    private AudioClip ResolveClipForScene(string sceneName)
    {
        if (sceneName == Scenario1SceneName)
        {
            return scenario1Clip;
        }

        if (sceneName == Scenario2SceneName)
        {
            return scenario2Clip;
        }

        return menuClip;
    }

    #endregion

    #region Helpers

    private void EnsureAudioSource()
    {
        if (audioSource != null)
        {
            return;
        }

        audioSource = gameObject.AddComponent<AudioSource>();
        audioSource.playOnAwake = false;
        audioSource.loop = true;
        audioSource.spatialBlend = 0f;
    }

    private void ResolveClipsFromSbd()
    {
        if (SBD_Audio.Instance == null)
        {
            return;
        }

        if (menuClip == null)
        {
            menuClip = SBD_Audio.Instance.Ambiental_menu_music0;
        }

        if (scenario1Clip == null)
        {
            scenario1Clip = SBD_Audio.Instance.Ambiental_s1_music0;
        }

        if (scenario2Clip == null)
        {
            scenario2Clip = SBD_Audio.Instance.Ambiental_s2_music0;
        }
    }

    #endregion
}

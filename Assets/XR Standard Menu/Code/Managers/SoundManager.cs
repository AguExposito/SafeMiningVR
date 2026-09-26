using UnityEngine;
using NJ.Finder;
using UnityEngine.UI;

public class SoundManager : MonoBehaviour
{
    private Toggle toggle_sound;
    private Slider slider_sound;
    private float volume = 0.5f;
    private bool enableSound = true;
    private AudioSource audioSource;
    private LotoAmbientMusicController ambientMusicController;

    public void Initialize()
    {
        audioSource = gameObject.AddComponent<AudioSource>();
        audioSource.playOnAwake = false;
        audioSource.enabled = true;

        if (ControlCenter.Instance != null && ControlCenter.Instance.persistantDataManager != null)
        {
            volume = ControlCenter.Instance.persistantDataManager.generalPersistantData.volume;
            enableSound = ControlCenter.Instance.persistantDataManager.generalPersistantData.enableSound;
        }

        EnsureAmbientMusicController();
        ApplyVolumeToManagedSources();
        FindMiscInteractables();
    }

    private void EnsureAmbientMusicController()
    {
        ambientMusicController = GetComponent<LotoAmbientMusicController>();
        if (ambientMusicController == null)
        {
            ambientMusicController = gameObject.AddComponent<LotoAmbientMusicController>();
        }

        ambientMusicController.Initialize(volume, enableSound);
    }
    void ChangeSingleASVolume(float _volume)
    {
        volume = _volume;
        ApplyVolumeToManagedSources();
    }
    void ChangeSingleASEnable(bool enable)
    {
        enableSound = enable;
        ApplyVolumeToManagedSources();
    }
    void FindMiscInteractables()
    {
        toggle_sound = ComponentFinder.FindComponentInSceneWithName<Toggle>("Toggle Sound");
        slider_sound = ComponentFinder.FindComponentInSceneWithName<Slider>("Slider Volume");

        if (toggle_sound != null)
        {
            toggle_sound.onValueChanged.RemoveListener(OnToggleSoundValueChanged);
            toggle_sound.onValueChanged.AddListener(OnToggleSoundValueChanged);
            // Sincronizar UI con estado actual
            toggle_sound.isOn = enableSound;
        }

        if (slider_sound != null)
        {
            slider_sound.onValueChanged.RemoveListener(OnSliderSoundValueChanged);
            slider_sound.onValueChanged.AddListener(OnSliderSoundValueChanged);
            // Sincronizar UI con estado actual
            slider_sound.value = volume;
        }
    }
    public AudioSource[] FindAllSingleASInScene()
    {
        return ComponentFinder.FindAllComponentsInScene<AudioSource>();
    }
    public void OnToggleSoundValueChanged(bool value)
    {
        if (value) EnableSound(); else DisableSound();
        ControlCenter.Instance.persistantDataManager.SaveGeneralData(); // Guardar el estado de sonido en la persistencia
    }
    public void OnSliderSoundValueChanged(float value)
    {
        ControlCenter.Instance.persistantDataManager.generalPersistantData.volume = value;
        ControlCenter.Instance.persistantDataManager.SaveGeneralData(); // Guardar el volumen de sonido en la persistencia
        ChangeSingleASVolume(value);
    }
    public void PlayAudioAtPoint(AudioClip audioClip, Vector3 position)
    {
        if (enableSound)
        {
            AudioSource.PlayClipAtPoint(audioClip, position, volume);
        }
    }
    public void PlayOneShotClip(AudioClip audioClip)
    {
        if (enableSound)
        {
            audioSource.PlayOneShot(audioClip, volume);
        }
    }
    public void EnableSound()
    {
        enableSound = true;
        ControlCenter.Instance.persistantDataManager.generalPersistantData.enableSound = enableSound;
        ControlCenter.Instance.persistantDataManager.SaveGeneralData(); // Guardar el estado de sonido en la persistencia
        ApplyVolumeToManagedSources();
    }
    public void DisableSound()
    {
        enableSound = false;
        ControlCenter.Instance.persistantDataManager.generalPersistantData.enableSound = enableSound;
        ControlCenter.Instance.persistantDataManager.SaveGeneralData(); // Guardar el estado de sonido en la persistencia
        ApplyVolumeToManagedSources();
    }

    void ApplyVolumeToManagedSources()
    {
        float effectiveVolume = enableSound ? volume : 0f;
        if (audioSource != null)
        {
            audioSource.enabled = true;
            audioSource.volume = effectiveVolume;
        }

        if (ambientMusicController == null)
        {
            ambientMusicController = GetComponent<LotoAmbientMusicController>();
        }

        if (ambientMusicController != null)
        {
            ambientMusicController.ApplyVolume(volume, enableSound);
        }
    }
}

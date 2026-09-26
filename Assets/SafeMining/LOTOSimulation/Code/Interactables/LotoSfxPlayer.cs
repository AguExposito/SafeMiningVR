using UnityEngine;

[DisallowMultipleComponent]
[RequireComponent(typeof(AudioSource))]
public class LotoSfxPlayer : MonoBehaviour
{
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip clip;
    [SerializeField] [Range(0f, 1f)] private float baseVolume = 1f;
    [SerializeField] [Range(0f, 0.5f)] private float volumeVariance = 0.08f;
    [SerializeField] [Range(0.1f, 3f)] private float basePitch = 1f;
    [SerializeField] [Range(0f, 0.5f)] private float pitchVariance = 0.06f;

    #region Lifecycle

    private void Awake()
    {
        if (audioSource == null)
        {
            audioSource = GetComponent<AudioSource>();
        }

        if (audioSource != null)
        {
            audioSource.playOnAwake = false;
        }
    }

    #endregion

    #region Public API

    public void Play()
    {
        PlayClip(clip);
    }

    public void PlayClip(AudioClip audioClip)
    {
        if (audioSource == null || audioClip == null)
        {
            return;
        }

        audioSource.pitch = basePitch + Random.Range(-pitchVariance, pitchVariance);
        float volume = Mathf.Clamp01(baseVolume + Random.Range(-volumeVariance, volumeVariance));
        audioSource.PlayOneShot(audioClip, volume);
    }

    #endregion
}

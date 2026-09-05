using UnityEngine;

public class LotoAmbientDustController : MonoBehaviour
{
    [SerializeField] private ParticleSystem dustParticles;

    private SimulationOrchestratorManager orchestrator;
    private bool subscribed;

    #region Lifecycle

    private void Awake()
    {
        if (dustParticles == null)
        {
            dustParticles = GetComponent<ParticleSystem>();
        }
    }

    private void OnEnable()
    {
        TrySubscribe();
    }

    private void OnDisable()
    {
        Unsubscribe();
        PauseDust();
    }

    public void RefreshPlayback()
    {
        TrySubscribe();
        ApplyPlaybackForCurrentState();
    }

    #endregion

    #region Orchestrator

    private void TrySubscribe()
    {
        if (subscribed)
        {
            return;
        }

        if (ControlCenter.Instance == null)
        {
            return;
        }

        orchestrator = ControlCenter.Instance.simulationOrchestratorManager;
        if (orchestrator == null)
        {
            return;
        }

        orchestrator.OnStateChanged += HandleStateChanged;
        subscribed = true;
    }

    private void Unsubscribe()
    {
        if (!subscribed || orchestrator == null)
        {
            subscribed = false;
            return;
        }

        orchestrator.OnStateChanged -= HandleStateChanged;
        subscribed = false;
    }

    private void HandleStateChanged(LotoSimulationState state)
    {
        if (state == LotoSimulationState.Active)
        {
            PlayDust();
            return;
        }

        PauseDust();
    }

    private void ApplyPlaybackForCurrentState()
    {
        if (orchestrator == null)
        {
            PauseDust();
            return;
        }

        if (orchestrator.CurrentState == LotoSimulationState.Active)
        {
            PlayDust();
            return;
        }

        PauseDust();
    }

    #endregion

    #region Playback

    private void PlayDust()
    {
        if (dustParticles == null)
        {
            return;
        }

        if (!dustParticles.isPlaying)
        {
            dustParticles.Play(true);
        }
    }

    private void PauseDust()
    {
        if (dustParticles == null)
        {
            return;
        }

        if (dustParticles.isPlaying)
        {
            dustParticles.Pause(true);
        }
    }

    #endregion
}

using UnityEngine;

public class BeltAnimator : MonoBehaviour
{
    [SerializeField] private Transform beltVisual;
    [SerializeField] private float scrollSpeed = 0.35f;
    [SerializeField] private float failedStartDurationSeconds = 1.75f;
    [SerializeField] private float failedStartScrollMultiplier = 3f;

    private bool isRunning = true;
    private float failedStartTimer;

    #region Public API

    public void SetRunning(bool running)
    {
        isRunning = running;
        failedStartTimer = 0f;
    }

    public void PlayFailedStartAttempt()
    {
        isRunning = true;
        failedStartTimer = failedStartDurationSeconds;
    }

    #endregion

    #region Lifecycle

    private void Update()
    {
        if (beltVisual == null)
        {
            return;
        }

        if (failedStartTimer > 0f)
        {
            failedStartTimer -= Time.deltaTime;
            beltVisual.localPosition += Vector3.right * (scrollSpeed * failedStartScrollMultiplier * Time.deltaTime);
            if (beltVisual.localPosition.x > 1f)
            {
                beltVisual.localPosition = new Vector3(-1f, beltVisual.localPosition.y, beltVisual.localPosition.z);
            }

            if (failedStartTimer <= 0f)
            {
                isRunning = false;
            }

            return;
        }

        if (!isRunning)
        {
            return;
        }

        beltVisual.localPosition += Vector3.right * (scrollSpeed * Time.deltaTime);
        if (beltVisual.localPosition.x > 1f)
        {
            beltVisual.localPosition = new Vector3(-1f, beltVisual.localPosition.y, beltVisual.localPosition.z);
        }
    }

    #endregion

    #region Setup

    public void Configure(Transform visual)
    {
        beltVisual = visual;
    }

    #endregion
}

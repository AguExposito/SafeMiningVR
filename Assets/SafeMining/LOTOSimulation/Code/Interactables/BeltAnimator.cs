using UnityEngine;

public class BeltAnimator : MonoBehaviour
{
    [SerializeField] private Transform beltVisual;
    [SerializeField] private float scrollSpeed = 0.35f;

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
        failedStartTimer = 1.25f;
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
            beltVisual.localPosition += Vector3.right * (scrollSpeed * 2f * Time.deltaTime);
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

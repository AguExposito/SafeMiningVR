using UnityEngine;

public class LotoPcLocomotion : MonoBehaviour
{
    [SerializeField] private float moveSpeed = 3.5f;
    [SerializeField] private float lookSensitivity = 1.8f;
    [SerializeField] private float minPitch = -40f;
    [SerializeField] private float maxPitch = 55f;

    private CharacterController characterController;
    private Transform lookTransform;
    private float pitch;

    private void Awake()
    {
#if UNITY_ANDROID && !UNITY_EDITOR
        enabled = false;
        return;
#endif
        characterController = GetComponent<CharacterController>();
        Camera cam = Camera.main;
        lookTransform = cam != null ? cam.transform : null;
        if (lookTransform != null)
        {
            Vector3 euler = lookTransform.localEulerAngles;
            pitch = euler.x > 180f ? euler.x - 360f : euler.x;
        }
    }

    private void Update()
    {
#if UNITY_ANDROID && !UNITY_EDITOR
        return;
#endif
        if (!Application.isPlaying)
        {
            return;
        }

        ApplyLook();
        ApplyMove();
    }

    private void ApplyMove()
    {
        float x = 0f;
        float z = 0f;
        if (Input.GetKey(KeyCode.A))
        {
            x -= 1f;
        }

        if (Input.GetKey(KeyCode.D))
        {
            x += 1f;
        }

        if (Input.GetKey(KeyCode.S))
        {
            z -= 1f;
        }

        if (Input.GetKey(KeyCode.W))
        {
            z += 1f;
        }

        Transform yawSource = lookTransform != null ? lookTransform : transform;
        Vector3 forward = yawSource.forward;
        forward.y = 0f;
        if (forward.sqrMagnitude < 0.0001f)
        {
            forward = transform.forward;
            forward.y = 0f;
        }

        forward.Normalize();
        Vector3 right = Vector3.Cross(Vector3.up, forward);
        Vector3 planar = (forward * z) + (right * x);
        if (planar.sqrMagnitude > 1f)
        {
            planar.Normalize();
        }

        Vector3 velocity = planar * moveSpeed;
        if (characterController != null && characterController.enabled)
        {
            velocity.y = Physics.gravity.y;
            characterController.Move(velocity * Time.deltaTime);
            return;
        }

        transform.position += velocity * Time.deltaTime;
    }

    private void ApplyLook()
    {
        if (!Input.GetMouseButton(1))
        {
            return;
        }

        float yaw = Input.GetAxisRaw("Mouse X") * lookSensitivity;
        float pitchDelta = Input.GetAxisRaw("Mouse Y") * lookSensitivity;
        transform.Rotate(0f, yaw, 0f, Space.World);

        if (lookTransform == null)
        {
            return;
        }

        pitch = Mathf.Clamp(pitch - pitchDelta, minPitch, maxPitch);
        Vector3 localEuler = lookTransform.localEulerAngles;
        lookTransform.localEulerAngles = new Vector3(pitch, localEuler.y, 0f);
    }
}

using UnityEngine;

public class ReticleLookAtPlayer : MonoBehaviour
{
    [Header("Target")]
    [Tooltip("Transform del jugador. Si está vacío, buscará la Main Camera.")]
    [SerializeField] private Transform player;

    [Header("Rotation")]
    [Tooltip("Velocidad de rotación sobre el eje Y.")]
    [SerializeField] private float rotationSpeed = 90f;

    private void Start()
    {
        // Si no asignamos player manualmente,
        // utilizamos la Main Camera como referencia.
        if (player == null && Camera.main != null)
        {
            player = Camera.main.transform;
        }
    }

    private void Update()
    {
        if (player == null)
            return;

        // Dirección desde la retícula hacia el jugador.
        Vector3 direction = player.position - transform.position;

        if (direction.sqrMagnitude > 0.001f)
        {
            // Hacer que la retícula mire al jugador.
            Quaternion lookRotation = Quaternion.LookRotation(direction);

            transform.rotation = Quaternion.Slerp(
                transform.rotation,
                lookRotation,
                rotationSpeed * Time.deltaTime
            );
        }

        // Rotación adicional sobre el eje Y.
        transform.Rotate(
            0f,
            rotationSpeed * Time.deltaTime,
            0f,
            Space.Self
        );
    }
}
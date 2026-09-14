using UnityEngine;

public class ConveyorMovement : MonoBehaviour
{
    [Header("Configuración")]
    [Tooltip("Velocidad máxima a la que se mueve la textura")]
    public float velocidadMaxima = 0.5f;

    [Tooltip("Qué tan rápido acelera y desacelera (cuánto mayor, más rápido reacciona)")]
    public float tasaAceleracion = 2.0f;

    [Tooltip("Define si la cinta debe estar en movimiento o detenida")]
    public bool estaMoviendose = false;

    public bool locked = false;

    private Material materialCinta;
    private Vector2 offsetActual;
    private float velocidadActual = 0f;

    void Start()
    {
        Renderer rendererCinta = GetComponent<Renderer>();
        if (rendererCinta != null)
        {
            materialCinta = rendererCinta.material;
        }
    }

    void Update()
    {
        if (materialCinta == null) return;

        // Definimos la velocidad a la que queremos llegar según el estado
        float velocidadObjetivo = estaMoviendose ? velocidadMaxima : 0f;

        // Suavizamos el cambio de velocidad (aceleración / desaceleración)
        velocidadActual = Mathf.MoveTowards(velocidadActual, velocidadObjetivo, tasaAceleracion * Time.deltaTime);

        // Si la velocidad es distinta de cero, desplazamos la textura
        if (velocidadActual != 0f)
        {
            offsetActual.y += velocidadActual * Time.deltaTime;
            materialCinta.mainTextureOffset = offsetActual;
        }
    }

    public void SetMovementState(bool estado)
    {
        if (locked) return;
        estaMoviendose = estado;
    }

    public void SetLocked(bool state) { 
        if (state) { 
            locked = true;
            estaMoviendose = false;
        }
        else { 
            locked = false;
        }
    }
}
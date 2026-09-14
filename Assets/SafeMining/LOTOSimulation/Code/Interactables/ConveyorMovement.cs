using UnityEngine;

public class ConveyorMovement : MonoBehaviour
{
    [Header("Configuración")]
    [Tooltip("Velocidad a la que se mueve la textura")]
    public float velocidadMovimiento = 0.5f;

    [Tooltip("Define si la cinta está en movimiento actualmente")]
    public bool estaMoviendose = false;

    private Material materialCinta;
    private Vector2 offsetActual;

    void Start()
    {
        // Obtenemos el material del Renderer del objeto
        Renderer rendererCinta = GetComponent<Renderer>();
        if (rendererCinta != null)
        {
            // Usamos material en lugar de sharedMaterial para no afectar a otros objetos que usen el mismo material
            materialCinta = rendererCinta.material;
        }
    }

    void Update()
    {
        if (materialCinta == null) return;

        if (estaMoviendose)
        {
            // Desplazamos la textura en el eje Y (puedes cambiarlo a X dependiendo de cómo esté mapeada la UV)
            offsetActual.y += velocidadMovimiento * Time.deltaTime;

            // Aplicamos el nuevo offset a la textura principal del material
            materialCinta.mainTextureOffset = offsetActual;
        }
    }
}
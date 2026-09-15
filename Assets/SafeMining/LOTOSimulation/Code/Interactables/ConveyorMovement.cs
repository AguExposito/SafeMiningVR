using UnityEngine;

public class ConveyorMovement : MonoBehaviour
{
    public float velocidadMaxima = 0.5f;
    public float tasaAceleracion = 2.0f;
    public bool estaMoviendose = false;
    public bool locked = false;

    private Material materialCinta;
    private Vector2 offsetActual;
    private float velocidadActual = 0f;

    #region Lifecycle

    private void Start()
    {
        Renderer rendererCinta = GetComponent<Renderer>();
        if (rendererCinta != null)
        {
            materialCinta = rendererCinta.material;
        }
    }

    private void Update()
    {
        if (materialCinta == null)
        {
            return;
        }

        float velocidadObjetivo = estaMoviendose ? velocidadMaxima : 0f;
        velocidadActual = Mathf.MoveTowards(velocidadActual, velocidadObjetivo, tasaAceleracion * Time.deltaTime);
        if (velocidadActual != 0f)
        {
            offsetActual.y += velocidadActual * Time.deltaTime;
            materialCinta.mainTextureOffset = offsetActual;
        }
    }

    #endregion

    #region Public API

    public void SetMovementState(bool estado)
    {
        if (locked || estado)
        {
            return;
        }

        estaMoviendose = false;
    }

    public void SetLocked(bool state)
    {
        if (state)
        {
            locked = true;
            estaMoviendose = false;
            return;
        }

        locked = false;
    }

    #endregion
}

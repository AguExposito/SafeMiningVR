using UnityEngine;

/// <summary>
/// Colocar en un objeto siempre activo de la escena del menú.
/// Al cargar la escena, si el jugador viene de una actividad con score,
/// activa automáticamente el <see cref="scorePanel"/> especificado.
/// </summary>
public class Util_ShowScoreOnReturn : MonoBehaviour
{
    [Tooltip("Panel de puntuaciones a mostrar al volver de una escena con score.")]
    [SerializeField] private GameObject scorePanel;

    private void Start()
    {
        if (scorePanel == null)
            return;

        if (ScoreManager.ConsumeAutoShowFlag())
        {
            scorePanel.SetActive(true);
        }
    }
}

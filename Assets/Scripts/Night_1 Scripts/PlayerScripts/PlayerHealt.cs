using UnityEngine;

public class PlayerHealth : MonoBehaviour
{
    [Header("Configuración de Vida")]
    public float vidaMaxima = 100f;
    public float vidaActual;
    private bool estaMuerto = false;

    [Header("UI de Derrota (Asignar en cada escena)")]
    public GameObject pantallaDerrota; 

    void Start()
    {
        vidaActual = vidaMaxima;
        estaMuerto = false;
    }

    public void RecibirDanio(float cantidad)
    {
        if (estaMuerto) return;

        vidaActual -= cantidad;
        Debug.Log($"Vida del jugador: {vidaActual}/{vidaMaxima}");

        if (vidaActual <= 0)
        {
            GatillarDerrota();
        }
    }

    void GatillarDerrota()
    {
        estaMuerto = true;
        Debug.Log("SISTEMA: Jugador derrotado.");

        // 1. Activa la UI de la escena actual
        if (pantallaDerrota != null)
        {
            pantallaDerrota.SetActive(true);
        }

        // 2. Libera el mouse para el botón "Reintentar"
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        // 3. Congela el juego
        Time.timeScale = 0f;
    }
}
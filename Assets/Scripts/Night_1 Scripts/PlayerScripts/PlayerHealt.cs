using UnityEngine;

public class PlayerHealth : MonoBehaviour
{
    // 1. Creás el Singleton (si ya lo tenías, dejas el tuyo)
    public static PlayerHealth Instance { get; private set; }

    [Header("Vida")]
    public float vidaActual = 100f;

    [Header("UI de Derrota")]
    // 2. AGREGÁ ESTA VARIABLE: Acá vas a arrastrar el Canvas de derrota desde el Inspector
    public GameObject canvasDerrota; 

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    void Start()
    {
        // Aseguramos que la pantalla de derrota arranque apagada al iniciar la noche
        if (canvasDerrota != null) canvasDerrota.SetActive(false);
    }

    // Esta es la función que ya está llamando el enemigo
    public void RecibirDanio(float cantidad)
    {
        if (vidaActual <= 0) return; 

        vidaActual -= cantidad;
        Debug.Log("Vida del jugador: " + vidaActual);

        if (vidaActual <= 0)
        {
            Murió();
        }
    }

    private void Murió()
    {
        Debug.Log("Lucas fue derrotado por las sombras...");

        // 3. ACTIVAMOS EL CANVAS DE DERROTA
        if (canvasDerrota != null)
        {
            canvasDerrota.SetActive(true);
        }

        // 4. CONGELAMOS EL JUEGO (Opcional, pero ideal para que los enemigos no te sigan pegando de fondo)
        Time.timeScale = 0f; 

        // 5. LIBERAMOS EL MOUSE (Para que el jugador pueda hacer click en los botones de "Reintentar" o "Salir")
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }
}
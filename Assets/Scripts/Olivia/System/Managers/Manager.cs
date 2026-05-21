using TMPro;
using UnityEngine;
using System.Collections;

public class Manager : MonoBehaviour
{
    private Coroutine corrutinaActiva;

    [Header("UI y Diálogos")]
    public TextMeshProUGUI textoSubtitulos;
    public CanvasGroup canvasGroupDialogo; // Asegúrate de que el fondo tenga este componente
    public float velocidadEscritura = 0.04f;
    public float velocidadFade = 3f; // Nueva variable para controlar la suavidad

    public static Manager Instance;

    private void Awake()
    {
         Instance = this;
    }

    public void MostrarDialogo(string mensaje)
    {
        if (textoSubtitulos != null && canvasGroupDialogo != null)
        {
            // Si ya hay algo escribiéndose, lo matamos de raíz
            if (corrutinaActiva != null)
            {
                StopCoroutine(corrutinaActiva);
            }
        
            // Guardamos la nueva corrutina en la variable
            corrutinaActiva = StartCoroutine(SecuenciaDialogo(mensaje));
        }
    }

    IEnumerator SecuenciaDialogo(string frase)
    {
        // 1. Limpieza total antes de empezar la nueva frase
        textoSubtitulos.text = "";
    
        // Forzamos el fade in (si ya estaba visible, no pasa nada)
        while (canvasGroupDialogo.alpha < 1)
        {
            canvasGroupDialogo.alpha += Time.deltaTime * velocidadFade;
            yield return null;
        }

        // 2. Efecto Typewriter
        foreach (char letra in frase.ToCharArray())
        {
            textoSubtitulos.text += letra;
            yield return new WaitForSeconds(velocidadEscritura);
        }

        // 3. Tiempo de lectura
        yield return new WaitForSeconds(3f);

        // 4. Fade Out
        while (canvasGroupDialogo.alpha > 0)
        {
            canvasGroupDialogo.alpha -= Time.deltaTime * (velocidadFade / 2);
            yield return null;
        }
    
        // Importante: decimos que ya terminó para limpiar la referencia
        corrutinaActiva = null; 
    }
}

using UnityEngine;
using System.Collections; // Necesario para IEnumerator

public class TriggerRegreso : MonoBehaviour
{
    private bool activado = false;

    // Nombre de la animación tal cual está en el Animator Controller
    public string nombreAnimacion = "mixamo_com"; 

    void OnTriggerEnter(Collider other)
    {
        // 1. Verificamos que sea el Player y que el trigger no se haya activado antes
        if (other.CompareTag("Player") && !activado)
        {
            activado = true;
            StartCoroutine(AparicionMujer());
        }
    }

    IEnumerator AparicionMujer()
    {
        // Obtenemos el Manager (usando la sintaxis moderna para evitar warnings)
        Act1Manager manager = Object.FindFirstObjectByType<Act1Manager>();
        
        // --- 2. EL MOMENTO DEL PARPADEO ---
        // Iniciamos el efecto visual para ocultar la aparición
        if (manager.effectoParpadeo != null) manager.effectoParpadeo.IniciarParpadeo();
        ParanoiaSystem.Instance.AddParanoia(20f);
        
        // Esperamos un momento breve mientras las luces están apagadas
        yield return new WaitForSeconds(1f);

        manager.CambiarIluminacion("Servicio");

        // --- 3. CONFIGURAR Y ACTIVAR LA ANIMACIÓN ---
        if (manager.objetoMujer != null)
        {
            // Obtenemos el componente Animator de la mujer
            Animator animatorMujer = manager.objetoMujer.GetComponent<Animator>();

            if (animatorMujer != null)
            {
                // Forzamos la reproducción de la animación desde el principio (time 0)
                // Esto es crucial para que aparezca ya en la pose correcta.
                animatorMujer.Play(nombreAnimacion, 0, 0f);
            }

            // Activamos el GameObject para que el jugador la vea
            manager.objetoMujer.SetActive(true);
        }
        
        // --- 4. EL DIÁLOGO DE CIERRE DE ACTO ---
        // 1. Ella habla
        manager.MostrarDialogo("Mujer: Lucas... ¿Todavía servís lo mismo de siempre?");
        yield return new WaitForSeconds(4f);

        // 2. Lucas duda (Indicios de que la conoce)
        // Como sos barítono, aquí la actuación de voz sería un susurro confuso
        manager.MostrarDialogo("Lucas: Esa voz... ¿Nos conocemos de algún lado?");
        yield return new WaitForSeconds(3f);
    
        manager.MostrarDialogo("Mujer: No lo sé. Quizás el tiempo borró más que solo los nombres.");
        yield return new WaitForSeconds(3f);

        // 3. Lucas intenta volver a la normalidad (mecanismo de defensa)
        manager.MostrarDialogo("Lucas: Tengo una botella especial en la cocina. Es... de la casa. Ya vuelvo.");
    
        // 4. Habilitamos la cocina de nuevo
        manager.HabilitarTriggerCocinaFinal();
    }
}
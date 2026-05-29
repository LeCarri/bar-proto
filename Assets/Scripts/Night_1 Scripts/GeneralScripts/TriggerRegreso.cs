using UnityEngine;
using System.Collections; // Necesario para IEnumerator

public class TriggerRegreso : MonoBehaviour
{
    private bool activado = false;

    [Header("Animación de la mujer")]
    // Nombre de la animación tal cual está en el Animator Controller
    public string nombreAnimacion = "mixamo_com";

    [Header("Objetos a desactivar durante este acto")]
    [Tooltip("Arrastrar acá el cartel o GameObject del servicio de bebidas para que no confunda al jugador.")]
    [SerializeField] private GameObject cartelServicioBebidas;

    void OnTriggerEnter(Collider other)
    {
        // 1. Verificamos que sea el Player y que el trigger no se haya activado antes
        if (other.CompareTag("Player") && !activado)
        {
            activado = true;

            // Desactivamos el cartel/pedido de servicio de bebidas para evitar confusión
            DesactivarCartelServicioBebidas();

            StartCoroutine(AparicionMujer());
        }
    }

    private void DesactivarCartelServicioBebidas()
    {
        if (cartelServicioBebidas != null)
        {
            cartelServicioBebidas.SetActive(false);
            Debug.Log("[TriggerRegreso] Cartel de servicio de bebidas desactivado para evitar confusión con la botella especial.");
        }
        else
        {
            Debug.LogWarning("[TriggerRegreso] No se asignó el cartelServicioBebidas en el Inspector.");
        }
    }

    IEnumerator AparicionMujer()
    {
        // Obtenemos el Manager
        Act1Manager manager = Object.FindFirstObjectByType<Act1Manager>();

        // --- 2. EL MOMENTO DEL PARPADEO ---
        // Iniciamos el efecto visual para ocultar la aparición
        if (manager != null && manager.effectoParpadeo != null)
        {
            manager.effectoParpadeo.IniciarParpadeo();
        }

        if (ParanoiaSystem.Instance != null)
        {
            ParanoiaSystem.Instance.AddParanoia(20f);
        }
        else
        {
            Debug.LogWarning("[TriggerRegreso] ParanoiaSystem.Instance es NULL.");
        }

        // Esperamos un momento breve mientras las luces están titilando/apagadas
        yield return new WaitForSeconds(1f);

        // 🔥 CAMBIO CLAVE: Activamos las luces creepy de la barra para la mujer
        if (manager != null)
        {
            manager.CambiarIluminacion("Creepy");
        }

        // --- 3. CONFIGURAR Y ACTIVAR LA ANIMACIÓN ---
        if (manager != null && manager.objetoMujer != null)
        {
            // Activamos primero el GameObject para asegurar que el Animator esté activo
            manager.objetoMujer.SetActive(true);

            // Obtenemos el componente Animator de la mujer
            Animator animatorMujer = manager.objetoMujer.GetComponent<Animator>();

            if (animatorMujer != null)
            {
                // Forzamos la reproducción de la animación desde el principio (time 0)
                animatorMujer.Play(nombreAnimacion, 0, 0f);
            }
            else
            {
                Debug.LogWarning("[TriggerRegreso] El objetoMujer no tiene Animator.");
            }
        }
        else
        {
            Debug.LogWarning("[TriggerRegreso] No se encontró Act1Manager o no está asignado objetoMujer.");
        }

        if (manager != null)
        {
            // --- 4. EL DIÁLOGO DE CIERRE DE ACTO ---

            // 1. Ella habla
            manager.MostrarDialogo("Mujer: Lucas... ¿Todavía servís lo mismo de siempre?");
            yield return new WaitForSeconds(5f);

            // 2. Lucas duda
            manager.MostrarDialogo("Lucas: Esa voz... ¿Nos conocemos de algún lado?");
            yield return new WaitForSeconds(5f);

            manager.MostrarDialogo("Mujer: No lo sé. Quizás el tiempo borró más que solo los nombres.");
            yield return new WaitForSeconds(5f);

            // 3. Lucas intenta volver a la normalidad
            manager.MostrarDialogo("Lucas: Tengo una botella especial en la cocina. Es... de la casa. Ya vuelvo.");

            // 4. Habilitamos la cocina de nuevo para que vaya a buscar la botella del combate
            manager.HabilitarTriggerCocinaFinal();
        }
        else
        {
            Debug.LogWarning("[TriggerRegreso] No se encontró Act1Manager. No se pudo mostrar el diálogo ni habilitar la cocina final.");
        }
    }
}
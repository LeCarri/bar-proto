using UnityEngine;
using System.Collections;

/// <summary>
/// Extiende el efecto de parpadeo del Acto 1 para cambiar elementos del bar
/// entre cada parpadeo a negro, creando la sensación de que la realidad se fisura.
/// Colocar en el mismo GameObject que EffectoParpadeo, o en uno separado.
/// Arrastrar los grupos de cambios en el Inspector.
/// </summary>
public class ParpadeoBarCambio : MonoBehaviour
{
    [Header("Pantalla Negra")]
    public GameObject pantallaNegra;
    public AudioSource audioParpadeo;

    [Header("Grupos de Cambios del Bar")]
    [Tooltip("Cada elemento de este array es un 'estado' del bar que se activa tras un parpadeo")]
    public CambioEstadoBar[] cambiosSecuenciales;

    [Header("Configuración")]
    public float velocidadParpadeo = 0.12f;
    public int cantidadParpadeos = 3;

    [Header("Música de Suspenso (Último Cambio)")]
    [Tooltip("AudioSource que reproducirá la música de suspenso en el último parpadeo")]
    public AudioSource audioMusicaSuspenso;
    [Tooltip("Clip de música de suspenso")]
    public AudioClip clipSuspenso;

    /// <summary>
    /// Ejecuta un parpadeo y luego aplica el cambio del índice dado.
    /// Llamar desde Act2Manager en orden: 0, 1, 2.
    /// </summary>
    public void EjecutarParpadeoConCambio(int indiceCambio)
    {
        StartCoroutine(SecuenciaParpadeoYCambio(indiceCambio));
    }

    IEnumerator SecuenciaParpadeoYCambio(int indiceCambio)
    {
        // Parpadeos rápidos
        for (int i = 0; i < cantidadParpadeos; i++)
        {
            MostrarPantallaNegra(true);
            if (audioParpadeo != null) audioParpadeo.Play();
            yield return new WaitForSeconds(velocidadParpadeo);

            MostrarPantallaNegra(false);
            yield return new WaitForSeconds(velocidadParpadeo * 0.8f);
        }

        // Apagón final para el cambio
        MostrarPantallaNegra(true);
        yield return new WaitForSeconds(0.4f);

        // Aplica el cambio del bar en la oscuridad
        AplicarCambio(indiceCambio);

        // Vuelta a la realidad
        yield return new WaitForSeconds(0.1f);
        MostrarPantallaNegra(false);
    }

    /// <summary>
    /// NUEVO: aplica todos los cambios de escenario en orden, instantáneamente.
    /// Solo para preparar el estado final del bar en el DEBUG post-jumpscare.
    /// No reproduce parpadeos, sonidos de parpadeo ni música de suspenso.
    /// </summary>
    public void AplicarTodosLosCambiosSinParpadeo()
    {
        // Si el debug se activa en mitad de una secuencia, evitar que una
        // corrutina pendiente revierta o mezcle los estados visuales.
        StopAllCoroutines();
        MostrarPantallaNegra(false);
        if (audioParpadeo != null) audioParpadeo.Stop();
        if (audioMusicaSuspenso != null) audioMusicaSuspenso.Stop();

        if (cambiosSecuenciales == null || cambiosSecuenciales.Length == 0)
        {
            Debug.LogWarning("[ParpadeoBarCambio] No hay cambiosSecuenciales configurados: el debug no puede desordenar el bar.");
            return;
        }

        for (int i = 0; i < cambiosSecuenciales.Length; i++)
            AplicarCambio(i, false);

        Debug.Log($"[ParpadeoBarCambio] DEBUG: aplicados {cambiosSecuenciales.Length} cambios. Bar en estado final, sin parpadeos ni música de suspenso.");
    }

    // Segundo parámetro NUEVO: permite omitir música al aplicar cambios en debug.
    void AplicarCambio(int indice, bool reproducirMusica = true)
    {
        if (cambiosSecuenciales == null || indice < 0 || indice >= cambiosSecuenciales.Length) return;

        CambioEstadoBar cambio = cambiosSecuenciales[indice];
        if (cambio == null) return;

        // Activa los objetos nuevos (cuadros torcidos, manchas, sillas desplazadas)
        if (cambio.objetosAActivar != null)
        {
            foreach (GameObject obj in cambio.objetosAActivar)
                if (obj != null) obj.SetActive(true);
        }

        // Desactiva los objetos del estado normal
        if (cambio.objetosADesactivar != null)
        {
            foreach (GameObject obj in cambio.objetosADesactivar)
                if (obj != null) obj.SetActive(false);
        }

        // Solo la secuencia normal reproduce música en el último cambio.
        if (reproducirMusica && indice == cambiosSecuenciales.Length - 1)
            ReproducirMusicaSuspenso();
    }

    void ReproducirMusicaSuspenso()
    {
        if (audioMusicaSuspenso != null)
        {
            if (clipSuspenso != null)
                audioMusicaSuspenso.clip = clipSuspenso;

            audioMusicaSuspenso.loop = true;
            if (!audioMusicaSuspenso.isPlaying)
                audioMusicaSuspenso.Play();
        }
        else
        {
            Debug.LogWarning("[ParpadeoBarCambio] No se asignó el AudioSource para la música de suspenso.");
        }
    }

    void MostrarPantallaNegra(bool mostrar)
    {
        if (pantallaNegra != null) pantallaNegra.SetActive(mostrar);
    }
}

/// <summary>
/// Define qué GameObjects se activan/desactivan en un estado de cambio del bar.
/// Crear en el Inspector para cada parpadeo: cuadros torcidos, manchas, sillas movidas, etc.
/// </summary>
[System.Serializable]
public class CambioEstadoBar
{
    [Tooltip("Objetos que aparecen tras este parpadeo (cuadro torcido, mancha, etc.)")]
    public GameObject[] objetosAActivar;

    [Tooltip("Objetos del estado normal que desaparecen tras este parpadeo")]
    public GameObject[] objetosADesactivar;
}

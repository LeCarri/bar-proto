using UnityEngine;
using System.Collections;

/// <summary>
/// Gestiona el combate grupal de sombras en el Acto 2.
/// Instancia las sombras desde puntos de spawn predefinidos, activa
/// bloqueadores de salidas y notifica al Act2Manager cuando una sombra muere.
///
/// SETUP:
///  - Colocar en un GameObject vacío "SombrasCombate".
///  - prefabSombra: arrastrar el prefab "Shadows" (de PrototypePrefabs/EnemyPrefabs/).
///  - puntosDeSombra: array de Transforms vacíos distribuidos por el bar, cerca de salidas.
///  - bloqueadoresSalidas: array de GameObjects (paredes invisibles) que bloquean puertas.
///  - Las sombras usan EnemyCore_Act2 que reporta muertes a Act2Manager.
/// </summary>
public class SombrasCombate : MonoBehaviour
{
    [Header("Sombras")]
    [Tooltip("Prefab del enemigo Sombra (debe tener EnemyCore_Act2 en vez de EnemyCore)")]
    public GameObject prefabSombra;

    [Tooltip("Transforms que marcan dónde aparecen las sombras (cerca de salidas del bar)")]
    public Transform[] puntosDeSombra;

    [Header("Bloqueadores de Salidas")]
    [Tooltip("GameObjects (colisionadores invisibles) que cierran las puertas de salida")]
    public GameObject[] bloqueadoresSalidas;

    [Header("Configuración")]
    [Tooltip("Delay entre aparición de cada sombra (en segundos)")]
    public float delayEntreSombras = 0.6f;

    [Header("Audio")]
    public AudioSource sonidoAparicion;
    public AudioSource ambienceCombate;

    private bool combateActivo = false;

    /// <summary>
    /// Llamado por Act2Manager al iniciar el estado de psicosis.
    /// </summary>
    public void IniciarCombate()
    {
        if (combateActivo) return;
        combateActivo = true;

        // Bloquear salidas
        foreach (GameObject bloqueador in bloqueadoresSalidas)
            if (bloqueador != null) bloqueador.SetActive(true);

        if (ambienceCombate != null) ambienceCombate.Play();

        StartCoroutine(SpawnSecuencial());
    }

    IEnumerator SpawnSecuencial()
    {
        foreach (Transform punto in puntosDeSombra)
        {
            if (punto == null || prefabSombra == null) continue;

            GameObject sombra = Instantiate(prefabSombra, punto.position, punto.rotation);

            // Si el prefab tiene EnemyCore_Act2, lo activará automáticamente
            sombra.SetActive(true);

            if (sonidoAparicion != null) sonidoAparicion.Play();

            yield return new WaitForSeconds(delayEntreSombras);
        }
    }

    /// <summary>
    /// Llamado por EnemyCore_Act2.Die() — solo para logging interno del combate.
    /// Act2Manager ya es notificado directamente desde EnemyCore_Act2.Die().
    /// </summary>
    public void SombraEliminada()
    {
        Debug.Log("[SombrasCombate] Una sombra fue eliminada.");
    }

    /// <summary>
    /// Limpia todo al finalizar el acto (cuando la psicosis termina abruptamente).
    /// </summary>
    public void DesactivarTodo()
    {
        combateActivo = false;

        // Desbloquear salidas
        foreach (GameObject bloqueador in bloqueadoresSalidas)
            if (bloqueador != null) bloqueador.SetActive(false);

        if (ambienceCombate != null) ambienceCombate.Stop();

        // Destruir sombras activas
        EnemyCore_Act2[] sombrasBuscadas = FindObjectsByType<EnemyCore_Act2>(FindObjectsSortMode.None);
        foreach (EnemyCore_Act2 s in sombrasBuscadas)
            if (s != null) Destroy(s.gameObject);
    }
}

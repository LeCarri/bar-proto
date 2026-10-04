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

    // ================= REESTRUCTURA NOCHE 2 =================
    [Header("Reestructura Noche 2 — Sombras que siguen viniendo por detrás")]
    [Tooltip("\"Siguen apareciendo sombras que vienen por detrás.\" Después de la primera tanda siguen apareciendo.")]
    public bool spawnContinuo = true;
    public float intervaloSpawnContinuo = 4f;
    [Tooltip("Máximo de sombras vivas al mismo tiempo.")]
    public int maxSombrasVivas = 6;
    [Tooltip("Si es true, las sombras continuas salen del punto que quede DETRÁS del jugador.")]
    public bool aparecerDetrasDelJugador = true;
    [Tooltip("Si está marcado, NO se activan los bloqueadores de salidas (recomendado: el jugador tiene que poder llegar al sótano).")]
    public bool ignorarBloqueadores = true;

    private readonly System.Collections.Generic.List<GameObject> sombrasCreadas = new System.Collections.Generic.List<GameObject>();
    // =========================================================

    private bool combateActivo = false;

    /// <summary>
    /// Llamado por Act2Manager al iniciar el estado de psicosis.
    /// </summary>
    public void IniciarCombate()
    {
        if (combateActivo) return;
        combateActivo = true;

        // Bloquear salidas
        // REESTRUCTURA: opcional (ignorarBloqueadores) para que el jugador pueda llegar a la puerta del sótano
        if (!ignorarBloqueadores && bloqueadoresSalidas != null)
            foreach (GameObject bloqueador in bloqueadoresSalidas)
                if (bloqueador != null) bloqueador.SetActive(true);

        if (ambienceCombate != null) ambienceCombate.Play();

        if (sonidoAparicion != null) sonidoAparicion.Play();
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
            sombrasCreadas.Add(sombra);   // REESTRUCTURA: para poder borrarlas todas al cierre

            yield return new WaitForSeconds(delayEntreSombras);
        }

        // REESTRUCTURA: siguen apareciendo por detrás
        if (spawnContinuo) StartCoroutine(SpawnContinuo());
    }

    // ================= REESTRUCTURA NOCHE 2 =================
    IEnumerator SpawnContinuo()
    {
        while (combateActivo)
        {
            yield return new WaitForSeconds(intervaloSpawnContinuo);
            if (!combateActivo) yield break;

            sombrasCreadas.RemoveAll(s => s == null);
            if (sombrasCreadas.Count >= maxSombrasVivas) continue;

            Transform punto = ElegirPunto();
            if (punto == null || prefabSombra == null) continue;

            GameObject sombra = Instantiate(prefabSombra, punto.position, punto.rotation);
            sombra.SetActive(true);
            sombrasCreadas.Add(sombra);
            if (sonidoAparicion != null) sonidoAparicion.Play();
        }
    }

    Transform ElegirPunto()
    {
        if (puntosDeSombra == null || puntosDeSombra.Length == 0) return null;

        Camera cam = Camera.main;
        if (aparecerDetrasDelJugador && cam != null)
        {
            Transform mejor = null;
            float mejorDot = 0f;
            Vector3 mirada = cam.transform.forward; mirada.y = 0f; mirada.Normalize();
            foreach (Transform p in puntosDeSombra)
            {
                if (p == null) continue;
                Vector3 hacia = p.position - cam.transform.position; hacia.y = 0f;
                float dot = Vector3.Dot(mirada, hacia.normalized);   // negativo = detrás
                if (dot < mejorDot) { mejorDot = dot; mejor = p; }
            }
            if (mejor != null) return mejor;
        }

        return puntosDeSombra[Random.Range(0, puntosDeSombra.Length)];
    }
    // =========================================================

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

        // REESTRUCTURA: el prefab Shadow_1 usa PersecutionEnemy (no EnemyCore_Act2),
        // así que también borramos todas las que creó este script.
        StopAllCoroutines();
        foreach (GameObject s in sombrasCreadas)
            if (s != null) Destroy(s);
        sombrasCreadas.Clear();
    }
}

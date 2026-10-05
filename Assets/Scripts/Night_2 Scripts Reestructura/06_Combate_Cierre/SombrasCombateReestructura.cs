using UnityEngine;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// NOCHE 2 REESTRUCTURADA — Copia de SombrasCombate.cs adaptada a la reestructura.
/// (El original sigue intacto para la demo.)
///
/// Combate con las Sombras en el salón (después de la última aparición del Vigilante):
///  1. Aparece una sombra en cada "Punto De Sombra", de a una.
///  2. "Siguen apareciendo sombras que vienen por detrás": cada tanto aparece otra desde el punto
///     que quede detrás del jugador (hasta un máximo de sombras vivas).
///  3. Al abrir la puerta del sótano, el Manager llama a DesactivarTodo() y desaparecen todas.
///
/// SETUP:
///  - GameObject vacío "SombrasCombate" con este script.
///  - prefabSombra: el prefab de la sombra (el mismo que usa la demo).
///  - puntosDeSombra: Transforms vacíos repartidos por el salón (mínimo 4, algunos lejos de la puerta del sótano).
/// </summary>
public class SombrasCombateReestructura : MonoBehaviour
{
    [Header("Sombras")]
    [Tooltip("Prefab del enemigo Sombra.")]
    public GameObject prefabSombra;
    [Tooltip("Transforms que marcan dónde aparecen las sombras.")]
    public Transform[] puntosDeSombra;
    [Tooltip("Delay entre la aparición de cada sombra de la primera tanda (segundos).")]
    public float delayEntreSombras = 0.6f;

    [Header("Sombras que siguen viniendo por detrás")]
    [Tooltip("\"Siguen apareciendo sombras que vienen por detrás.\"")]
    public bool spawnContinuo = true;
    public float intervaloSpawnContinuo = 4f;
    [Tooltip("Máximo de sombras vivas al mismo tiempo.")]
    public int maxSombrasVivas = 6;
    [Tooltip("Si es true, las sombras nuevas salen del punto que quede DETRÁS del jugador.")]
    public bool aparecerDetrasDelJugador = true;

    [Header("Bloqueadores de salidas (opcional)")]
    [Tooltip("Paredes invisibles que se activan durante el combate. Dejalo VACÍO si no querés bloquear nada: el jugador tiene que poder llegar a la puerta del sótano.")]
    public GameObject[] bloqueadoresSalidas;

    [Header("Audio")]
    public AudioSource sonidoAparicion;
    public AudioSource ambienceCombate;

    private readonly List<GameObject> sombrasCreadas = new List<GameObject>();
    private bool combateActivo;

    public bool CombateActivo => combateActivo;

    /// <summary>Lo llama el Act2ManagerReestructura al empezar el combate.</summary>
    public void IniciarCombate()
    {
        if (combateActivo) return;
        combateActivo = true;

        if (prefabSombra == null) Debug.LogError("[SombrasCombateReestructura] Falta asignar 'Prefab Sombra'.");
        if (puntosDeSombra == null || puntosDeSombra.Length == 0) Debug.LogError("[SombrasCombateReestructura] No hay 'Puntos De Sombra'.");

        if (bloqueadoresSalidas != null)
            foreach (GameObject b in bloqueadoresSalidas)
                if (b != null) b.SetActive(true);

        if (ambienceCombate != null) ambienceCombate.Play();
        if (sonidoAparicion != null) sonidoAparicion.Play();

        StartCoroutine(SpawnSecuencial());
    }

    IEnumerator SpawnSecuencial()
    {
        if (puntosDeSombra != null)
        {
            foreach (Transform punto in puntosDeSombra)
            {
                if (!combateActivo) yield break;
                if (punto == null || prefabSombra == null) continue;

                CrearSombra(punto);
                yield return new WaitForSeconds(delayEntreSombras);
            }
        }

        if (spawnContinuo) StartCoroutine(SpawnContinuo());
    }

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

            CrearSombra(punto);
            if (sonidoAparicion != null) sonidoAparicion.Play();
        }
    }

    void CrearSombra(Transform punto)
    {
        GameObject sombra = Instantiate(prefabSombra, punto.position, punto.rotation);
        sombra.SetActive(true);
        sombrasCreadas.Add(sombra);
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

    /// <summary>Limpia todo (lo llama el Manager cuando se abre la puerta del sótano).</summary>
    public void DesactivarTodo()
    {
        combateActivo = false;
        StopAllCoroutines();

        if (bloqueadoresSalidas != null)
            foreach (GameObject b in bloqueadoresSalidas)
                if (b != null) b.SetActive(false);

        if (ambienceCombate != null) ambienceCombate.Stop();

        foreach (GameObject s in sombrasCreadas)
            if (s != null) Destroy(s);
        sombrasCreadas.Clear();

        // Por las dudas, también las sombras con EnemyCore_Act2 que no haya creado este script
        foreach (EnemyCore_Act2 s in FindObjectsByType<EnemyCore_Act2>())
            if (s != null) Destroy(s.gameObject);
    }
}

using UnityEngine;
using System.Collections;

/// <summary>
/// La figura pequeña del hijo/a del protagonista.
/// Aparece sentada en una mesa durante el combate de sombras.
/// Al acercarse el jugador, desaparece y la voz se escucha desde el sótano.
///
/// SETUP:
///  - Colocar el modelo del niño como hijo de este GameObject, desactivado al inicio.
///  - Ajustar distanciaDesaparicion a unos 2-3 metros.
///  - El AudioSource "vozNino" debe tener el clip de voz del niño llamando al barman.
///  - El AudioSource "vozSotano" debe ser una fuente de audio posicionada cerca del sótano.
/// </summary>
public class FiguraNino : MonoBehaviour
{
    [Header("Referencias")]
    [Tooltip("El modelo 3D del niño (hijo de este GameObject)")]
    public GameObject modeloNino;



    [Header("Audio")]
    [Tooltip("Voz del niño llamando al protagonista (cerca del jugador)")]
    public AudioSource vozNino;
    [Tooltip("Clip de audio: el niño llama ('Papá...', 'Lucas...', etc.)")]
    public AudioClip clipVozNino;

    [Tooltip("Fuente de audio posicionada cerca de la puerta del sótano para la voz lejana")]
    public AudioSource vozSotano;
    [Tooltip("Clip de audio: misma voz pero desde el sótano")]
    public AudioClip clipVozSotano;

    [Header("Comportamiento")]
    [Tooltip("Distancia en metros a la que el niño desaparece cuando el jugador se acerca")]
    public float distanciaDesaparicion = 2.5f;

    [Tooltip("Segundos de espera después de aparecer antes de empezar a detectar al jugador")]
    public float delayDeteccion = 2f;

    private bool aparecido = false;
    private bool desaparecido = false;
    private Transform jugador;
    private float tiempoAparicion;

    void Start()
    {
        if (modeloNino != null) modeloNino.SetActive(false);

        // Busca el jugador por tag
        GameObject obj = GameObject.FindGameObjectWithTag("Player");
        if (obj != null) jugador = obj.transform;
    }

    void Update()
    {
        if (!aparecido || desaparecido || jugador == null) return;

        // Solo detecta proximidad después del delay
        if (Time.time - tiempoAparicion < delayDeteccion) return;

        float distancia = Vector3.Distance(transform.position, jugador.position);
        if (distancia <= distanciaDesaparicion)
        {
            StartCoroutine(DesapareceYVozSotano());
        }
    }

    /// <summary>
    /// Llamado por Act2Manager cuando corresponde mostrar la figura.
    /// </summary>
    public void Aparecer()
    {
        if (aparecido) return;
        aparecido = true;
        tiempoAparicion = Time.time;


        if (modeloNino != null) modeloNino.SetActive(true);

        // Reproducir voz del niño llamando
        if (vozNino != null && clipVozNino != null)
        {
            vozNino.clip = clipVozNino;
            vozNino.Play();
        }
    }

    IEnumerator DesapareceYVozSotano()
    {
        desaparecido = true;

        // El niño desaparece instantáneamente (más perturbador)
        if (modeloNino != null) modeloNino.SetActive(false);
        if (vozNino != null) vozNino.Stop();

        // Pausa breve de silencio
        yield return new WaitForSeconds(0.5f);

        // La misma voz suena desde el sótano
        if (vozSotano != null && clipVozSotano != null)
        {
            vozSotano.clip = clipVozSotano;
            vozSotano.Play();
        }

        Act2Manager.Instance?.MostrarDialogo("Lucas: ¿...desde el sótano?");
    }
}

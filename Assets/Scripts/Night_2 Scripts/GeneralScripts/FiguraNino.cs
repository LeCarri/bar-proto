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

    // ================= REESTRUCTURA NOCHE 2 =================
    // Guion: "Hay una niña sentada. Quietísima. Tiene Sombras alrededor, pero no la atacan.
    //  Lucas: 'Pilar… ¿Sos vos?'. Al acercarnos la niña desaparece y en su lugar queda el dibujo
    //  familiar de la Noche 1."
    [Header("Reestructura Noche 2")]
    [Tooltip("Sombras decorativas alrededor de Pilar (no atacan). Empiezan desactivadas.")]
    public GameObject sombrasAlrededor;
    [Tooltip("El dibujo familiar de la Noche 1 que queda en su lugar. Empieza desactivado.")]
    public GameObject dibujoQueQueda;
    [TextArea] public string dialogoAlVerla = "Lucas: Pilar… ¿Sos vos?";
    public float distanciaDialogo = 6f;
    public float distanciaDesaparicion = 2.5f;

    private bool dialogoMostrado = false;
    private bool desaparecida = false;
    private Transform jugador;
    // =========================================================

    private bool aparecido = false;

    void Start()
    {
        if (modeloNino != null) modeloNino.SetActive(false);

        // REESTRUCTURA
        if (sombrasAlrededor != null) sombrasAlrededor.SetActive(false);
        if (dibujoQueQueda != null) dibujoQueQueda.SetActive(false);
        GameObject p = GameObject.FindGameObjectWithTag("Player");
        if (p != null) jugador = p.transform;
    }

    void Update()
    {
        // REESTRUCTURA: diálogo y desaparición por cercanía
        if (!aparecido || desaparecida || jugador == null) return;

        Vector3 centro = modeloNino != null ? modeloNino.transform.position : transform.position;
        float d = Vector3.Distance(jugador.position, centro);

        if (!dialogoMostrado && d <= distanciaDialogo)
        {
            dialogoMostrado = true;
            if (!string.IsNullOrEmpty(dialogoAlVerla)) Act2Manager.Instance?.MostrarDialogo(dialogoAlVerla);
        }

        if (dialogoMostrado && d <= distanciaDesaparicion)
            Desaparecer();
    }

    /// <summary>REESTRUCTURA: la niña desaparece y queda el dibujo familiar.</summary>
    public void Desaparecer()
    {
        if (desaparecida || !aparecido) return;   // no puede desaparecer si todavía no apareció
        desaparecida = true;

        if (modeloNino != null) modeloNino.SetActive(false);
        if (vozNino != null) vozNino.Stop();
        if (sombrasAlrededor != null) sombrasAlrededor.SetActive(false);
        if (dibujoQueQueda != null) dibujoQueQueda.SetActive(true);

        Act2Manager.Instance?.PilarDesaparecio();
    }

    public bool YaDesaparecio => desaparecida;

    /// <summary>
    /// Llamado por Act2Manager cuando corresponde mostrar la figura.
    /// </summary>
    public void Aparecer()
    {
        if (aparecido) return;
        aparecido = true;


        if (modeloNino != null) modeloNino.SetActive(true);
        if (sombrasAlrededor != null) sombrasAlrededor.SetActive(true);   // REESTRUCTURA

        // Reproducir voz del niño llamando
        if (vozNino != null && clipVozNino != null)
        {
            vozNino.clip = clipVozNino;
            vozNino.Play();
        }
    }

    public IEnumerator DesapareceYVozSotano()
    {

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
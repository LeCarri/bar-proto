using UnityEngine;

/// <summary>
/// NOCHE 2 REESTRUCTURADA — Copia de FiguraNino.cs adaptada a la reestructura.
/// (El original sigue intacto para la demo.)
///
/// Guion: "Durante el combate, el jugador pasa por una de las mesas. Hay una niña sentada.
/// Quietísima. Tiene Sombras alrededor, pero no la atacan. Lucas: 'Pilar… ¿Sos vos?'.
/// Al acercarnos la niña desaparece y en su lugar queda el dibujo familiar de la Noche 1."
///
/// - El Manager la hace aparecer al empezar el combate.
/// - Cuando el jugador se acerca a "distanciaDialogo" → Lucas dice la frase.
/// - Cuando se acerca a "distanciaDesaparicion" → desaparece, queda el dibujo y el Manager programa
///   que la linterna se quede sin pilas.
///
/// SETUP: GameObject vacío en la silla de la mesa con este script. Como HIJOS: el modelo de Pilar,
/// las sombras decorativas y el dibujo. Asignalos en los campos (los tres empiezan apagados solos).
/// </summary>
public class FiguraNinoReestructura : MonoBehaviour
{
    [Header("Referencias")]
    [Tooltip("El modelo 3D de Pilar (hijo de este GameObject).")]
    public GameObject modeloNino;
    [Tooltip("Sombras decorativas alrededor de Pilar (no atacan). Se prenden con ella.")]
    public GameObject sombrasAlrededor;
    [Tooltip("El dibujo familiar de la Noche 1 que queda en su lugar.")]
    public GameObject dibujoQueQueda;

    [Header("Diálogo y distancias")]
    [TextArea] public string dialogoAlVerla = "Lucas: Pilar… ¿Sos vos?";
    public float distanciaDialogo = 6f;
    public float distanciaDesaparicion = 2.5f;

    [Header("Audio (opcional)")]
    [Tooltip("Sonido mientras Pilar está (respiración, tarareo...).")]
    public AudioSource vozNino;
    public AudioClip clipVozNino;
    [Tooltip("Sonido al desaparecer.")]
    public AudioSource sonidoDesaparicion;

    private bool aparecido;
    private bool dialogoMostrado;
    private bool desaparecida;
    private Transform jugador;

    public bool YaDesaparecio => desaparecida;

    void Start()
    {
        if (modeloNino != null) modeloNino.SetActive(false);
        if (sombrasAlrededor != null) sombrasAlrededor.SetActive(false);
        if (dibujoQueQueda != null) dibujoQueQueda.SetActive(false);

        GameObject p = GameObject.FindGameObjectWithTag("Player");
        if (p != null) jugador = p.transform;
    }

    void Update()
    {
        if (!aparecido || desaparecida || jugador == null) return;

        Vector3 centro = modeloNino != null ? modeloNino.transform.position : transform.position;
        float d = Vector3.Distance(jugador.position, centro);

        if (!dialogoMostrado && d <= distanciaDialogo)
        {
            dialogoMostrado = true;
            if (!string.IsNullOrEmpty(dialogoAlVerla)) Act2ManagerReestructura.Instance?.MostrarDialogo(dialogoAlVerla);
        }

        if (dialogoMostrado && d <= distanciaDesaparicion)
            Desaparecer();
    }

    /// <summary>La llama el Manager al empezar el combate.</summary>
    public void Aparecer()
    {
        if (aparecido) return;
        aparecido = true;

        if (modeloNino != null) modeloNino.SetActive(true);
        if (sombrasAlrededor != null) sombrasAlrededor.SetActive(true);

        if (vozNino != null && clipVozNino != null)
        {
            vozNino.clip = clipVozNino;
            vozNino.Play();
        }
    }

    /// <summary>La niña desaparece y queda el dibujo familiar.</summary>
    public void Desaparecer()
    {
        if (desaparecida || !aparecido) return;   // no puede desaparecer si todavía no apareció
        desaparecida = true;

        if (modeloNino != null) modeloNino.SetActive(false);
        if (sombrasAlrededor != null) sombrasAlrededor.SetActive(false);
        if (dibujoQueQueda != null) dibujoQueQueda.SetActive(true);
        if (vozNino != null) vozNino.Stop();
        if (sonidoDesaparicion != null) sonidoDesaparicion.Play();

        Act2ManagerReestructura.Instance?.PilarDesaparecio();
    }

    /// <summary>Apaga todo de golpe (la usa el Manager en el cierre: "todo vuelve a la normalidad").</summary>
    public void OcultarTodo()
    {
        if (modeloNino != null) modeloNino.SetActive(false);
        if (sombrasAlrededor != null) sombrasAlrededor.SetActive(false);
        if (dibujoQueQueda != null) dibujoQueQueda.SetActive(false);
        if (vozNino != null) vozNino.Stop();
        desaparecida = true;
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, distanciaDialogo);
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, distanciaDesaparicion);
    }
}

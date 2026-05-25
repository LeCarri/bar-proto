using UnityEngine;

/// <summary>
/// Comportamiento de "mirada perturbadora" para las sombras del Acto 2.
///
/// Al aparecer, la sombra se queda quieta y rota LENTAMENTE para mirar al jugador
/// durante unos segundos antes de atacar. El movimiento lento y constante es lo que
/// genera el miedo: el jugador siente que la sombra "lo está viendo".
///
/// SETUP:
///  1. Agregar este componente al prefab de la sombra.
///  2. En "componentesDeAtaque", arrastrar el script de comportamiento de ataque
///     (PersecutionEnemy, BlameEnemy, StaticEnemy, etc.) — estarán DESACTIVADOS
///     durante la fase de mirada y se activan solos al terminar.
///  3. Opcional: asignar "audioRespiracion" con un sonido de respiración pesada.
/// </summary>
public class SombraLookAt : MonoBehaviour
{
    [Header("Fase de Mirada")]
    [Tooltip("Segundos que la sombra mira al jugador antes de atacar. 3-5s es ideal.")]
    public float duracionMirada = 4f;

    [Tooltip("Grados por segundo de rotación. MÁS LENTO = más perturbador. Recomendado: 25-50.")]
    public float velocidadRotacion = 35f;

    [Tooltip("Si true, la sombra da un pequeño paso hacia adelante al terminar de mirar")]
    public bool avanzarAlAtacar = true;
    public float distanciaAvance = 0.4f;

    [Header("Componentes que se activan DESPUÉS de mirar")]
    [Tooltip("Arrastrar aquí: PersecutionEnemy, BlameEnemy, NavMeshAgent, etc.")]
    public MonoBehaviour[] componentesDeAtaque;

    [Header("Audio")]
    [Tooltip("Sonido de respiración pesada durante la fase de mirada (loop)")]
    public AudioSource audioRespiracion;

    [Tooltip("Sonido de 'arranque' al pasar de mirar a atacar")]
    public AudioSource audioAtaque;

    // -------------------------------------------------------
    private Transform jugador;
    private float tiempoMirando = 0f;
    private bool faseAtaque = false;

    void OnEnable()
    {
        // Reiniciar estado cada vez que la sombra se activa (spawn)
        tiempoMirando = 0f;
        faseAtaque = false;

        // Desactivar comportamiento de ataque mientras mira
        foreach (var comp in componentesDeAtaque)
            if (comp != null) comp.enabled = false;

        // Buscar al jugador
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null) jugador = playerObj.transform;

        // Empezar respiración
        if (audioRespiracion != null)
        {
            audioRespiracion.loop = true;
            audioRespiracion.Play();
        }
    }

    void OnDisable()
    {
        if (audioRespiracion != null) audioRespiracion.Stop();
    }

    void Update()
    {
        if (faseAtaque || jugador == null) return;

        tiempoMirando += Time.deltaTime;

        RotarHaciaJugador();

        if (tiempoMirando >= duracionMirada)
            IniciarAtaque();
    }

    void RotarHaciaJugador()
    {
        // Calcula la dirección hacia el jugador ignorando el eje Y (no se inclina)
        Vector3 direccion = jugador.position - transform.position;
        direccion.y = 0f;

        if (direccion.sqrMagnitude < 0.01f) return;

        Quaternion rotacionObjetivo = Quaternion.LookRotation(direccion);

        // Rotación suave — la lentitud es lo que da miedo
        transform.rotation = Quaternion.RotateTowards(
            transform.rotation,
            rotacionObjetivo,
            velocidadRotacion * Time.deltaTime
        );
    }

    void IniciarAtaque()
    {
        faseAtaque = true;

        // Detener respiración, reproducir sonido de arranque
        if (audioRespiracion != null) audioRespiracion.Stop();
        if (audioAtaque != null)      audioAtaque.Play();

        // Pequeño avance intimidatorio antes de activar el AI
        if (avanzarAlAtacar && jugador != null)
        {
            Vector3 dirAvance = (jugador.position - transform.position).normalized;
            dirAvance.y = 0f;
            transform.position += dirAvance * distanciaAvance;
        }

        // Activar el comportamiento de ataque/chase
        foreach (var comp in componentesDeAtaque)
            if (comp != null) comp.enabled = true;
    }

    // Dibuja en el editor la dirección hacia el jugador (útil para debugging)
    void OnDrawGizmosSelected()
    {
        if (jugador == null) return;
        Gizmos.color = Color.red;
        Gizmos.DrawLine(transform.position, jugador.position);
        Gizmos.DrawWireSphere(transform.position, 0.3f);
    }
}

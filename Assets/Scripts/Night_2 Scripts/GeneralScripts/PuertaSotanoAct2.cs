using UnityEngine;
using System.Collections;

/// <summary>
/// La puerta del sótano en el Acto 2.
/// Comportamientos:
///  1. Golpes rítmicos desde adentro (audio loop)
///  2. Bloquea el paso hasta el cierre del acto
///  3. Al usar la llave: secuencia de "crack" + apertura sola al final
///  4. Se abre sola en el fundido final del acto
///
/// SETUP:
///  - Colocar en el GameObject de la puerta del sótano.
///  - Tiene su propio IInteractable para que el jugador intente abrirla con la llave.
///  - sonidoGolpes: loop de golpes rítmicos (madera siendo golpeada por puños).
///  - Configurar el Animator o usar transform para la animación de apertura.
/// </summary>
public class PuertaSotanoAct2 : MonoBehaviour, IInteractable
{
    [Header("Estado")]
    private bool golpesActivos = false;
    private bool abierta = false;

    [Header("Audio")]
    [Tooltip("Loop de golpes rítmicos desde adentro del sótano")]
    public AudioSource sonidoGolpesRitmicos;

    [Tooltip("Sonido de cerradura al intentar usar la llave (crack metálico)")]
    public AudioSource sonidoCerradura;

    [Header("Animación de Apertura")]
    [Tooltip("Animator de la puerta (si tiene). Disparar trigger 'Abrir'.")]
    public Animator animadorPuerta;
    [Tooltip("Si no hay Animator, la puerta gira este ángulo en Y para abrirse")]
    public float anguloApertura = 90f;
    [Tooltip("Velocidad de apertura suave (grados por segundo)")]
    public float velocidadApertura = 30f;

    private bool abriendoSuave = false;
    private Quaternion rotacionObjetivo;

    void Update()
    {
        if (abriendoSuave && animadorPuerta == null)
        {
            transform.rotation = Quaternion.RotateTowards(
                transform.rotation,
                rotacionObjetivo,
                velocidadApertura * Time.deltaTime
            );

            if (Quaternion.Angle(transform.rotation, rotacionObjetivo) < 0.5f)
                abriendoSuave = false;
        }
    }

    /// <summary>
    /// Activa los golpes rítmicos. Llamado por Act2Manager tras encontrar los zapatos.
    /// </summary>
    public void ActivarGolpes()
    {
        golpesActivos = true;
        if (sonidoGolpesRitmicos != null)
        {
            sonidoGolpesRitmicos.loop = true;
            sonidoGolpesRitmicos.Play();
        }
    }

    public void DesactivarGolpes()
    {
        golpesActivos = false;
        if (sonidoGolpesRitmicos != null) sonidoGolpesRitmicos.Stop();
    }

    public void Interact()
    {
        if (abierta) return;

        Act2Manager manager = Act2Manager.Instance;
        if (manager == null) return;

        if (manager.TieneLlave())
        {
            // El manager maneja la secuencia completa (crack + normalización + apertura)
            manager.UsarLlave();
        }
        else
        {
            // Sin llave: el jugador escucha los golpes y no puede abrir
            manager.MostrarDialogo("Lucas: Está cerrada... y hay algo golpeando desde adentro...");
            if (sonidoGolpesRitmicos != null && !sonidoGolpesRitmicos.isPlaying)
                sonidoGolpesRitmicos.PlayOneShot(sonidoGolpesRitmicos.clip);
        }
    }

    /// <summary>
    /// La puerta se abre sola, suavemente. Llamado por Act2Manager al final del acto.
    /// </summary>
    public void AbrirSola()
    {
        if (abierta) return;
        abierta = true;

        DesactivarGolpes();

        if (animadorPuerta != null)
        {
            animadorPuerta.SetTrigger("Abrir");
        }
        else
        {
            // Apertura manual con rotación
            rotacionObjetivo = transform.rotation * Quaternion.Euler(0f, anguloApertura, 0f);
            abriendoSuave = true;
        }
    }

    public string GetDescription()
    {
        if (abierta) return "";

        Act2Manager manager = Act2Manager.Instance;
        if (manager != null && manager.TieneLlave())
            return "Presiona [E] para usar la llave";

        return golpesActivos
            ? "La puerta del sótano — hay algo golpeando adentro"
            : "Puerta del sótano — cerrada";
    }
}

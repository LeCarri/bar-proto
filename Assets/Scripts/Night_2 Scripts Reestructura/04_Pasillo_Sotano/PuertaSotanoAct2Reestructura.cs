using UnityEngine;

/// <summary>
/// NOCHE 2 REESTRUCTURADA — Copia de PuertaSotanoAct2.cs adaptada al Act2ManagerReestructura.
/// (El original sigue intacto para la demo.)
///
/// La puerta del sótano:
///  1. Golpes rítmicos desde adentro (después de los zapatos). Se apagan al acercarse
///     (TriggerZonaReestructura "SilenciarGolpesSotano").
///  2. Cerrada con candado hasta tener la llave.
///  3. Con la llave: [E] (o el TriggerCierreSotanoReestructura) → "Lucas introduce la llave y la puerta se abre".
///
/// SETUP: en el GameObject de cada hoja de la puerta del sótano (Collider + capa "Interactable").
/// Si la puerta tiene dos hojas, poné este script en las dos y asignalas en "Puerta Sotano L" y
/// "Puerta Sotano R" del Act2ManagerReestructura.
/// </summary>
public class PuertaSotanoAct2Reestructura : MonoBehaviour, IInteractable
{
    [Header("Audio")]
    [Tooltip("Loop de golpes rítmicos desde adentro del sótano (opcional: el Manager tiene otro).")]
    public AudioSource sonidoGolpesRitmicos;
    [Tooltip("Sonido de cerradura (opcional).")]
    public AudioSource sonidoCerradura;

    [Header("Apertura")]
    [Tooltip("Animator de la puerta (si tiene). Se dispara el trigger 'Abrir'.")]
    public Animator animadorPuerta;
    [Tooltip("Si no hay Animator, la puerta gira este ángulo para abrirse. Si se abre para el lado equivocado, poné el número en negativo.")]
    public float anguloApertura = 90f;
    [Tooltip("Eje (local) sobre el que gira la puerta. El original usa Z (0,0,1). Si la puerta gira 'acostada', probá Y (0,1,0).")]
    public Vector3 ejeApertura = new Vector3(0f, 0f, 1f);
    [Tooltip("Velocidad de apertura (grados por segundo).")]
    public float velocidadApertura = 30f;

    private bool golpesActivos;
    private bool abierta;
    private bool abriendoSuave;
    private Quaternion rotacionObjetivo;

    void Update()
    {
        if (!abriendoSuave || animadorPuerta != null) return;

        transform.rotation = Quaternion.RotateTowards(transform.rotation, rotacionObjetivo, velocidadApertura * Time.deltaTime);
        if (Quaternion.Angle(transform.rotation, rotacionObjetivo) < 0.5f) abriendoSuave = false;
    }

    /// <summary>Golpes rítmicos (los activa el Manager después de los zapatos).</summary>
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

    public bool CanInteract() => !abierta;

    public void Interact()
    {
        if (abierta) return;

        Act2ManagerReestructura manager = Act2ManagerReestructura.Instance;
        if (manager == null) return;

        if (manager.PuedeUsarLlave())
        {
            if (sonidoCerradura != null) sonidoCerradura.Play();
            manager.UsarLlave();   // el Manager hace toda la secuencia de apertura
        }
        else if (golpesActivos && sonidoGolpesRitmicos != null && !sonidoGolpesRitmicos.isPlaying && sonidoGolpesRitmicos.clip != null)
        {
            sonidoGolpesRitmicos.PlayOneShot(sonidoGolpesRitmicos.clip);
        }
    }

    /// <summary>La puerta se abre sola, suavemente (la llama el Manager).</summary>
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
            Vector3 eje = ejeApertura.sqrMagnitude > 0.0001f ? ejeApertura.normalized : Vector3.forward;
            rotacionObjetivo = transform.rotation * Quaternion.AngleAxis(anguloApertura, eje);
            abriendoSuave = true;
        }
    }

    public string GetDescription()
    {
        if (abierta) return "";

        Act2ManagerReestructura manager = Act2ManagerReestructura.Instance;
        if (manager != null && manager.PuedeUsarLlave())
            return "Presiona [E] para usar la llave";

        return golpesActivos
            ? "La puerta del sótano — hay algo golpeando adentro"
            : "Puerta del sótano — cerrada con candado";
    }
}

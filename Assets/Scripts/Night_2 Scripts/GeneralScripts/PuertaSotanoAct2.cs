using System.Collections;
using UnityEngine;

/// <summary>
/// Controla la puerta del sótano en el Acto 2.
/// Maneja la interacción con IInteractable, los golpes rítmicos desde adentro,
/// la caída del candado por física y el cierre de la noche.
/// </summary>
public class PuertaSotanoAct2 : MonoBehaviour, IInteractable
{
    [Header("Estado")]
    private bool golpesActivos = false;
    private bool abierta = false;
    private bool yaSeUso = false;

    [Header("Referencias del Candado y Puerta")]
    [Tooltip("El GameObject del candado o cadena en la puerta")]
    public GameObject candadoObjeto;
    [Tooltip("Rigidbody del candado para hacerlo caer físicamente (opcional)")]
    public Rigidbody candadoRigidbody;

    [Header("Animación de Apertura")]
    [Tooltip("Animator de la puerta (Trigger 'Abrir')")]
    public Animator animadorPuerta;
    [Tooltip("Si no hay Animator, la puerta gira este ángulo en Y")]
    public float anguloApertura = 90f;
    [Tooltip("Velocidad de apertura suave (grados por segundo)")]
    public float velocidadApertura = 30f;

    [Header("Audio")]
    [Tooltip("AudioSource para efectos generales (cerradura, rechinado, candado)")]
    public AudioSource audioSourcePuerta;
    [Tooltip("AudioSource exclusivo para el loop de golpes rítmicos")]
    public AudioSource sonidoGolpesRitmicos;

    [Space(5)]
    public AudioClip sonidoLlaveGirar;
    public AudioClip sonidoCandadoCaer;
    public AudioClip sonidoPuertaRechinado;

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

    #region Implementación de IInteractable

    public bool CanInteract()
    {
        return !abierta && !yaSeUso;
    }

    public void Interact()
    {
        if (abierta || yaSeUso) return;

        Act2ManagerDemo manager = Act2ManagerDemo.Instance;
        if (manager == null)
        {
            Debug.LogError("[PuertaSotanoAct2] Act2ManagerDemo.Instance es NULL.");
            return;
        }

        // Verificamos si el jugador ya posee la llave
        if (manager.llaveTenida)
        {
            yaSeUso = true;
            StartCoroutine(SecuenciaAbrirSotanoYCierre());
        }
        else
        {
            // Sin llave: el jugador percibe los golpes y la puerta trabada
            Debug.Log("[PuertaSotanoAct2] El jugador intentó abrir pero no tiene la llave.");
            
            if (sonidoGolpesRitmicos != null && !sonidoGolpesRitmicos.isPlaying)
            {
                sonidoGolpesRitmicos.PlayOneShot(sonidoGolpesRitmicos.clip);
            }
        }
    }

    public string GetDescription()
    {
        if (abierta) return "";

        Act2ManagerDemo manager = Act2ManagerDemo.Instance;
        if (manager != null && manager.llaveTenida)
        {
            return "Presiona [E] para usar la llave";
        }

        return golpesActivos
            ? "La puerta del sótano — hay algo golpeando adentro"
            : "Puerta del sótano — cerrada con candado";
    }

    #endregion

    #region Secuencia de Apertura y Cierre

    private IEnumerator SecuenciaAbrirSotanoYCierre()
    {
        Debug.Log("[PuertaSotanoAct2] Iniciando secuencia de apertura del sótano...");

        DesactivarGolpes();

        // 1. Giro de llave / Destrabe
        if (audioSourcePuerta != null && sonidoLlaveGirar != null)
        {
            audioSourcePuerta.PlayOneShot(sonidoLlaveGirar);
        }

        yield return new WaitForSeconds(1.0f);

        // 2. Caída del candado
        if (candadoRigidbody != null)
        {
            candadoRigidbody.isKinematic = false;
            candadoRigidbody.AddForce(Vector3.down * 2f, ForceMode.Impulse);
        }

        if (audioSourcePuerta != null && sonidoCandadoCaer != null)
        {
            audioSourcePuerta.PlayOneShot(sonidoCandadoCaer);
        }

        yield return new WaitForSeconds(0.8f);

        // 3. Apertura de puerta
        EjecutarAnimacionApertura();

        // 4. Pausa dramática observando la oscuridad del sótano
        yield return new WaitForSeconds(2.5f);

        // 5. Cierre de la noche / Transición al Acto/Noche 3
        if (Act2ManagerDemo.Instance != null)
        {
            Debug.Log("[PuertaSotanoAct2] Disparando SecuenciaCierre en Act2ManagerDemo...");
            Act2ManagerDemo.Instance.StartCoroutine(Act2ManagerDemo.Instance.SecuenciaCierre());
        }
    }

    private void EjecutarAnimacionApertura()
    {
        if (abierta) return;
        abierta = true;

        if (animadorPuerta != null)
        {
            animadorPuerta.SetTrigger("Abrir");
        }
        else
        {
            // Rotación suave en Y
            rotacionObjetivo = transform.rotation * Quaternion.Euler(0f, anguloApertura, 0f);
            abriendoSuave = true;
        }

        if (audioSourcePuerta != null && sonidoPuertaRechinado != null)
        {
            audioSourcePuerta.PlayOneShot(sonidoPuertaRechinado);
        }
    }

    #endregion

    #region Métodos Invocados por Act2ManagerDemo

    /// <summary>
    /// Activa el loop de golpes desde adentro.
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

    /// <summary>
    /// Desactiva el audio de golpes.
    /// </summary>
    public void DesactivarGolpes()
    {
        golpesActivos = false;
        if (sonidoGolpesRitmicos != null && sonidoGolpesRitmicos.isPlaying)
        {
            sonidoGolpesRitmicos.Stop();
        }
    }

    /// <summary>
    /// Método requerido por el Manager para forzar la apertura de la puerta.
    /// </summary>
    public void AbrirSola()
    {
        DesactivarGolpes();
        EjecutarAnimacionApertura();
    }

    #endregion
}
using System.Collections;
using UnityEngine;

/// <summary>
/// Pilar sentada se muestra animada y EN SILENCIO al recoger la llave.
/// Al ENTRAR al trigger del pasillo (despues de Naniela), empieza el llanto 2D,
/// Pilar sentada SIGUE VISIBLE mientras la otra Pilar corre.
/// Solo se oculta cuando se activa TriggerPilarDemon.
/// </summary>
[RequireComponent(typeof(BoxCollider))]
public class EventoPilarPasillo : MonoBehaviour
{
    [Header("Pilar que corre")]
    [SerializeField] private GameObject pilar;
    [SerializeField] private Animator animatorPilar;
    [Tooltip("Estado del Animator para correr. Dejalo vacío si es el predeterminado.")]
    [SerializeField] private string animacionCorrer = "Run";

    [Header("Pilar sentada - aparece al recoger la llave")]
    [SerializeField] private GameObject pilarSentada;
    [Tooltip("Animator del modelo PilarChild SitCry. Si no se asigna, se busca entre sus hijos al activarla.")]
    [SerializeField] private Animator animatorPilarSentada;
    [Tooltip("Nombre EXACTO del estado de la animación de sentada/llanto. Vacío = usar el estado predeterminado del Animator.")]
    [SerializeField] private string animacionSentada = "";

    [Header("Recorrido")]
    [Tooltip("El primer punto es donde aparece. Los siguientes marcan por dónde corre.")]
    [SerializeField] private Transform[] puntosRecorrido;
    [Min(0.1f)] [SerializeField] private float velocidad = 4.5f;
    [Min(0f)] [SerializeField] private float suavidadGiro = 12f;
    [Min(0f)] [SerializeField] private float esperaFinal = 0.1f;

    [Header("Risa")]
    [Tooltip("AudioSource preferentemente 3D, hijo de la Pilar que corre.")]
    [SerializeField] private AudioSource audioRisa;
    [SerializeField] private bool repetirRisaMientrasCorre = true;
    [Range(0f, 3f)] [SerializeField] private float duracionFadeOutRisa = 0.8f;

    [Header("Llanto 2D - Pilar sentada")]
    [Tooltip("AudioSource del llanto 2D; comienza al ENTRAR al trigger del pasillo, NO con la llave.")]
    [SerializeField] private AudioSource audioLlanto;
    [SerializeField] private bool repetirLlanto = true;

    private bool llantoIniciado;
    private bool habilitado;
    private bool ejecutado;
    private Coroutine rutina;
    public bool RecorridoFinalizado { get; private set; }

    private void Awake()
    {
        PrepararEvento();
    }

    public void PrepararEvento()
    {
        if (rutina != null)
        {
            StopCoroutine(rutina);
            rutina = null;
        }

        habilitado = false;
        ejecutado = false;
        RecorridoFinalizado = false;
        // Pilar sentada debe comenzar oculta hasta recoger la llave.
        OcultarPilarSentada();

        if (audioLlanto != null)
        {
            audioLlanto.playOnAwake = false;
            audioLlanto.spatialBlend = 0f;
            if (pilar != null && audioLlanto.transform.IsChildOf(pilar.transform))
                Debug.LogWarning("[Pilar] El AudioSource del llanto es hijo de Pilar que corre; movelo al modelo sentado o a un objeto independiente.");
        }

        if (audioRisa != null) audioRisa.Stop();
        if (pilar != null) pilar.SetActive(false);
    }

    /// <summary>Lo llama Act2ManagerDemo al recoger la llave del baño.</summary>
    public void MostrarPilarSentada()
    {
        if (ejecutado) return;

        if (pilarSentada == null)
        {
            Debug.LogWarning("[Pilar] Asigná 'Pilar Sentada' (PilarChild SitCry) en EventoPilarPasillo.");
            return;
        }

        pilarSentada.SetActive(true);
        if (!pilarSentada.activeInHierarchy)
        {
            Debug.LogWarning("[Pilar] Pilar sentada tiene un padre desactivado en la Hierarchy. Activá su padre y dejá desactivada solo PilarChild SitCry al inicio.");
            return;
        }

        // Forzamos al Animator a tomar inmediatamente la pose de la animación,
        // en lugar de mostrar el modelo unos frames con pose T.
        Animator anim = animatorPilarSentada;
        if (anim == null)
            anim = pilarSentada.GetComponentInChildren<Animator>(true);

        if (anim != null)
        {
            anim.enabled = true;
            if (anim.runtimeAnimatorController != null)
            {
                anim.Rebind();
                if (!string.IsNullOrEmpty(animacionSentada))
                    anim.Play(animacionSentada, 0, 0f);
                anim.Update(0f);
            }
            else
            {
                Debug.LogWarning("[Pilar] Pilar sentada tiene un Animator pero no tiene Animator Controller asignado.");
            }
        }
        else
        {
            Debug.LogWarning("[Pilar] No encontré Animator en PilarChild SitCry ni en sus hijos. Asignalo en 'Animator Pilar Sentada'.");
        }

        // Llave: SOLO modelo sentado con su Animator. El audio espera al trigger.
        DetenerLlanto3D();
        Debug.Log("[Pilar] Llave recogida: Pilar sentada animada, SIN llanto.");
    }

    public void OcultarPilarSentada()
    {
        // Ocultar solo al reiniciar, al cerrar el acto o al activar Pilar Demon.
        DetenerLlanto3D(); // Se conserva el nombre para los otros scripts.
        if (pilarSentada != null) pilarSentada.SetActive(false);
    }

    public void HabilitarEvento()
    {
        if (!ejecutado) habilitado = true;
    }

    private void OnTriggerEnter(Collider otro)
    {
        if (!habilitado || ejecutado) return;

        bool esJugador = otro.CompareTag("Player") || otro.transform.root.CompareTag("Player");
        if (!esJugador) return;

        if (Act2ManagerDemo.Instance == null ||
            Act2ManagerDemo.Instance.estadoActual != Act2ManagerDemo.Act2State.Psicosis)
            return;

        if (pilar == null || puntosRecorrido == null || puntosRecorrido.Length < 2)
        {
            Debug.LogError("[Pilar] Asigná el modelo de Pilar que corre y al menos 2 puntos de recorrido.");
            return;
        }

        for (int i = 0; i < puntosRecorrido.Length; i++)
        {
            if (puntosRecorrido[i] == null)
            {
                Debug.LogError("[Pilar] Hay un punto de recorrido sin asignar: " + i);
                return;
            }
        }

        habilitado = false;
        ejecutado = true;

        // Al entrar: comienza el llanto y Pilar corre, pero la Pilar sentada
        // PERMANECE visible y animada hasta el trigger de Pilar Demon.
        IniciarLlanto2D();
        rutina = StartCoroutine(CorrerPorPasillo());
    }

    private void IniciarLlanto2D()
    {
        if (llantoIniciado || audioLlanto == null) return;
        if (!audioLlanto.gameObject.activeInHierarchy)
        {
            Debug.LogWarning("[Pilar] El AudioSource del llanto está en un objeto desactivado. Comprobá su jerarquía.");
            return;
        }

        audioLlanto.playOnAwake = false;
        audioLlanto.spatialBlend = 0f;
        audioLlanto.loop = repetirLlanto;
        audioLlanto.volume = 1f;
        audioLlanto.Play();
        llantoIniciado = true;
    }

    // Se mantiene el nombre utilizado por otros scripts aunque el audio ahora es 2D.
    public void DetenerLlanto3D()
    {
        if (audioLlanto != null) audioLlanto.Stop();
        llantoIniciado = false;
    }

    private IEnumerator CorrerPorPasillo()
    {
        pilar.transform.position = puntosRecorrido[0].position;
        OrientarHacia(puntosRecorrido[1].position, true);
        pilar.SetActive(true);

        if (animatorPilar != null)
        {
            animatorPilar.applyRootMotion = false;
            if (!string.IsNullOrEmpty(animacionCorrer))
                animatorPilar.Play(animacionCorrer, 0, 0f);
        }

        if (audioRisa != null)
        {
            audioRisa.loop = repetirRisaMientrasCorre;
            audioRisa.Play();
        }

        for (int i = 1; i < puntosRecorrido.Length; i++)
        {
            Vector3 destino = puntosRecorrido[i].position;
            while (Vector3.Distance(pilar.transform.position, destino) > 0.03f)
            {
                OrientarHacia(destino, false);
                pilar.transform.position = Vector3.MoveTowards(
                    pilar.transform.position, destino, velocidad * Time.deltaTime);
                yield return null;
            }
            pilar.transform.position = destino;
        }

        // Avisar inmediatamente al manager para cambiar a BaseEnemy2.
        RecorridoFinalizado = true;
        if (Act2ManagerDemo.Instance != null)
            Act2ManagerDemo.Instance.NotificarFinRecorridoPilar();

        if (esperaFinal > 0f)
            yield return new WaitForSeconds(esperaFinal);

        if (audioRisa != null)
            yield return StartCoroutine(FadeOutRisa());

        pilar.SetActive(false);
        rutina = null;
    }

    private IEnumerator FadeOutRisa()
    {
        if (audioRisa == null || !audioRisa.isPlaying)
            yield break;

        float volumenInicial = audioRisa.volume;
        float tiempo = 0f;
        while (tiempo < duracionFadeOutRisa)
        {
            tiempo += Time.deltaTime;
            audioRisa.volume = Mathf.Lerp(
                volumenInicial, 0f,
                Mathf.Clamp01(tiempo / Mathf.Max(0.01f, duracionFadeOutRisa)));
            yield return null;
        }

        audioRisa.Stop();
        audioRisa.volume = volumenInicial;
    }

    private void OrientarHacia(Vector3 destino, bool instantaneo)
    {
        Vector3 direccion = destino - pilar.transform.position;
        direccion.y = 0f;
        if (direccion.sqrMagnitude < 0.001f) return;

        Quaternion objetivo = Quaternion.LookRotation(direccion.normalized, Vector3.up);
        pilar.transform.rotation = instantaneo
            ? objetivo
            : Quaternion.Slerp(pilar.transform.rotation, objetivo, suavidadGiro * Time.deltaTime);
    }

    [ContextMenu("TEST: Reiniciar evento Pilar")]
    private void ReiniciarEventoDesdeInspector()
    {
        PrepararEvento();
        HabilitarEvento();
        Debug.Log("[Pilar] Evento rearmado. Cruzá el trigger.");
    }
}

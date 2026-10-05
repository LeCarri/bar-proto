using UnityEngine;
using System.Collections;

/// <summary>
/// NOCHE 2 REESTRUCTURADA — Copia de ClienteCorrupto.cs adaptada a la reestructura.
/// (El original sigue intacto para la demo.)
///
/// Guion: "Está sentado solo. Su postura es demasiado rígida. No mira a Lucas." Es el ÚLTIMO cliente:
/// solo se puede atender cuando el Act2ManagerReestructura le da el turno (después de los normales).
/// Al interactuar, el Manager hace toda la escena: bloquea a Lucas → "¿Qué le sirvo?" → glitch
/// (gira la cabeza, todos los clientes miran, estática, respiración, sacudida) → "Traeme lo más
/// fuerte que tengas." → empieza la fase del Pasillo.
///
/// SETUP: en el cliente corrupto (con Collider + capa "Interactable"). Los diálogos se escriben
/// en el Act2ManagerReestructura (sección "2. SERVICIO").
/// </summary>
public class ClienteCorruptoReestructura : MonoBehaviour, IInteractable
{
    public enum EstadoCliente { EsperandoAtencion, EsperandoPedido, Atendido }

    [Header("Estado")]
    public EstadoCliente estadoActual = EstadoCliente.EsperandoAtencion;
    [Tooltip("Lo maneja el Act2ManagerReestructura (le da el turno después de los clientes normales). No tocar.")]
    public bool esSuTurno = false;

    [Header("Glitch")]
    [Tooltip("Parte que gira lentamente hacia Lucas durante el glitch (la cabeza). Vacío = todo el cliente.")]
    public Transform cabeza;
    [Tooltip("Si el modelo queda de costado o de espaldas al girar, corregilo acá (90, -90, 180).")]
    public float correccionGiroY = 0f;

    [Header("Visual (opcional)")]
    [Tooltip("Rastro de líquido que deja en el suelo. Se activa al empezar.")]
    public GameObject rastroLiquido;

    [Header("Voz distorsionada (opcional)")]
    [Tooltip("AudioSource para la voz distorsionada del pedido.")]
    public AudioSource vozDistorsionada;
    public AudioClip clipPedido;
    public float pitchDistorsion = 0.65f;

    void Start()
    {
        if (rastroLiquido != null) rastroLiquido.SetActive(true);
    }

    public bool CanInteract()
    {
        return esSuTurno && estadoActual == EstadoCliente.EsperandoAtencion;
    }

    public string GetDescription()
    {
        return CanInteract() ? "Presiona [E] para atender al cliente" : "";
    }

    public void Interact()
    {
        if (!CanInteract()) return;

        Act2ManagerReestructura m = Act2ManagerReestructura.Instance;
        if (m == null)
        {
            Debug.LogError("[ClienteCorruptoReestructura] No hay Act2ManagerReestructura en la escena.");
            return;
        }

        estadoActual = EstadoCliente.EsperandoPedido;
        m.IniciarSecuenciaClienteCorrupto(this);
    }

    /// <summary>"El cliente gira lentamente la cabeza hacia Lucas" (la usa el Manager).</summary>
    public IEnumerator GirarHaciaJugador(Transform jugador, float duracion)
    {
        Transform t = cabeza != null ? cabeza : transform;
        if (jugador == null) yield break;

        Quaternion inicio = t.rotation;
        Vector3 dir = jugador.position - t.position;
        dir.y = 0f;
        if (dir.sqrMagnitude < 0.001f) yield break;
        Quaternion fin = Quaternion.LookRotation(dir) * Quaternion.Euler(0f, correccionGiroY, 0f);

        float tiempo = 0f;
        while (tiempo < duracion)
        {
            tiempo += Time.deltaTime;
            t.rotation = Quaternion.Slerp(inicio, fin, Mathf.SmoothStep(0f, 1f, tiempo / duracion));
            yield return null;
        }
        t.rotation = fin;
    }

    /// <summary>Reproduce la voz distorsionada (si tiene clip asignado).</summary>
    public void ReproducirVozDistorsionada()
    {
        if (vozDistorsionada == null || clipPedido == null) return;
        vozDistorsionada.pitch = pitchDistorsion;
        vozDistorsionada.clip = clipPedido;
        vozDistorsionada.Play();
    }
}

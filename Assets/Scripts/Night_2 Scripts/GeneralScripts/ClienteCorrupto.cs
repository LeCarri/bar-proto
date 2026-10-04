using UnityEngine;
using System.Collections;

/// <summary>
/// Cliente corrupto del Acto 2. Apariencia deformada, voz distorsionada.
/// Uno de ellos deja un rastro de agua/líquido espeso al caminar.
/// Interacción: el jugador toma el pedido → debe ir a cocina → vuelve y entrega.
/// Colocar en cada NPC cliente del Acto 2. Implementa IInteractable.
/// </summary>
public class ClienteCorrupto : MonoBehaviour, IInteractable
{
    public enum EstadoCliente { EsperandoAtencion, EsperandoPedido, Atendido }

    [Header("Estado")]
    public EstadoCliente estadoActual = EstadoCliente.EsperandoAtencion;

    [Tooltip("Si es false, este cliente es decorativo y no tiene interacción")]
    public bool hacePedido = true;

    // ---- REESTRUCTURA NOCHE 2 ----
    [Tooltip("Lo maneja el Act2Manager: el corrupto es el ÚLTIMO cliente, solo se puede atender cuando le toca.")]
    public bool esSuTurno = true;
    [Tooltip("Parte que gira lentamente hacia Lucas durante el glitch (la cabeza). Vacío = todo el cliente.")]
    public Transform cabeza;
    [Tooltip("Si el modelo queda de costado o de espaldas al girar, corregilo acá (90, -90, 180).")]
    public float correccionGiroY = 0f;

    [Header("Diálogo (distorsionado)")]
    public string nombreCliente = "???";
    [TextArea] public string dialogoPedido = "Servime lo más fuerte que tengas...";
    [TextArea] public string dialogoEspera = "...";
    [TextArea] public string dialogoGracias = "Esto... es lo que necesitaba.";

    [Header("Visual Corrupto")]
    public GameObject indicadorVioleta;
    [Tooltip("Rastro de líquido que deja en el suelo (solo en el cliente empapado)")]
    public GameObject rastroLiquido;
    [Tooltip("Material distorsionado — arrastrar aquí el material de shader de corrupción")]
    public Renderer rendererCliente;

    [Header("Audio Distorsionado")]
    [Tooltip("AudioSource con pitch alterado para la voz distorsionada")]
    public AudioSource vozDistorsionada;
    public AudioClip clipPedido;

    [Header("Efecto al Tomar Pedido")]
    public float pitchDistorsion = 0.65f;

    void Start()
    {
        if (rastroLiquido != null) rastroLiquido.SetActive(true);
    }

    public void Interact()
    {
        if (!hacePedido) return;
        if (estadoActual != EstadoCliente.EsperandoAtencion) return;
        if (!esSuTurno) return;

        // ---- VERSIÓN ANTERIOR (comentada en la reestructura) ----
        // TomarPedido();

        // ---- REESTRUCTURA NOCHE 2 ----
        // El Act2Manager maneja toda la escena: "¿Qué le sirvo?" → glitch → "Traeme lo más fuerte que tengas."
        estadoActual = EstadoCliente.EsperandoPedido;
        if (Act2Manager.Instance != null)
            Act2Manager.Instance.IniciarSecuenciaClienteCorrupto(this);
    }
    public bool CanInteract()
    {
        // ---- VERSIÓN ANTERIOR ----
        // return true;
        return hacePedido && esSuTurno && estadoActual == EstadoCliente.EsperandoAtencion;
    }

    // ---- REESTRUCTURA NOCHE 2 ----
    /// <summary>"El cliente gira lentamente la cabeza hacia Lucas" (la usa el Act2Manager).</summary>
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
        if (vozDistorsionada != null && clipPedido != null)
        {
            vozDistorsionada.pitch = pitchDistorsion;
            vozDistorsionada.clip = clipPedido;
            vozDistorsionada.Play();
        }
    }
    void TomarPedido()
    {
        Act2Manager manager = Act2Manager.Instance;
        if (manager == null) return;

        // Reproducir voz distorsionada
        if (vozDistorsionada != null && clipPedido != null)
        {
            vozDistorsionada.pitch = pitchDistorsion;
            vozDistorsionada.clip = clipPedido;
            vozDistorsionada.Play();
        }

        manager.MostrarDialogo(nombreCliente + ": " + dialogoPedido);
        estadoActual = EstadoCliente.EsperandoPedido;

        // El barman se dirige a la cocina — los clientes desaparecen al entrar
        StartCoroutine(DelayIrACocina());
    }

    IEnumerator DelayIrACocina()
    {
        yield return new WaitForSeconds(2f);
        Act2Manager.Instance?.IrACocina();
    }

    IEnumerator DesvanecerCliente()
    {
        yield return new WaitForSeconds(1.5f);
        // Desvanecer el renderer si hay material con alpha
        if (rendererCliente != null)
        {
            float t = 0f;
            Color colorOriginal = rendererCliente.material.color;
            while (t < 1f)
            {
                t += Time.deltaTime * 0.5f;
                Color c = colorOriginal;
                c.a = Mathf.Lerp(1f, 0f, t);
                rendererCliente.material.color = c;
                yield return null;
            }
        }
        gameObject.SetActive(false);
    }

    public string GetDescription()
    {
        if (!hacePedido) return "";
        if (!esSuTurno) return "";
        if (estadoActual == EstadoCliente.EsperandoAtencion) return "Presiona [E] para atender al cliente";
        return "";
    }
}

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

        TomarPedido();
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
        if (estadoActual == EstadoCliente.EsperandoAtencion) return "Presiona [E] para atender al cliente";
        return "";
    }
}

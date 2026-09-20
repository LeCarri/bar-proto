using UnityEngine;

public class ServicioWhiskyFPS : MonoBehaviour
{
    [Header("Animación")]
    [SerializeField] private Animator animator;
    [SerializeField] private string nombreAnimacion = "WhiskyService_R";

    [Header("Brazos")]
    [SerializeField] private GameObject mallaBrazos;

    [Header("Botellas")]
    [SerializeField] private GameObject botellaMundo;
    [SerializeField] private GameObject botellaMano;
    [SerializeField] private Transform bottleSocket;
    [SerializeField] private ServicioWhiskyVisual visualWhisky;
    [Header("Resultado del servicio")]
    [SerializeField] private ItemSO itemWhiskyServido;


    private bool reproduciendo;

    public bool EstaSirviendo => reproduciendo;

    private void Start()
    {
        if (mallaBrazos != null)
            mallaBrazos.SetActive(false);

        if (botellaMundo != null)
            botellaMundo.SetActive(true);

        if (botellaMano != null)
            botellaMano.SetActive(false);
    }

    public void IniciarServicio()
    {
        if (reproduciendo)
            return;

        reproduciendo = true;

        Debug.Log("INICIO SERVICIO WHISKY");

        // Mostrar los brazos FPS.
        if (mallaBrazos != null)
            mallaBrazos.SetActive(true);

        // Estado inicial.
        if (botellaMundo != null)
            botellaMundo.SetActive(true);

        if (botellaMano != null)
            botellaMano.SetActive(false);

        // Reproducimos la animación desde el principio.
        animator.Play(nombreAnimacion, 0, 0f);
        animator.Update(0f);
    }

    // Animation Event
    public void AgarrarBotella()
{
    Debug.Log("EVENTO: AGARRAR BOTELLA");

    if (botellaMundo != null)
        botellaMundo.SetActive(false);

    if (botellaMano != null)
        botellaMano.SetActive(true);
}

    // Animation Event
public void FinalizarServicio()
{
    Debug.Log("EVENTO: FINALIZAR SERVICIO WHISKY");

    if (botellaMano != null)
        botellaMano.SetActive(false);

    if (botellaMundo != null)
        botellaMundo.SetActive(true);

    if (mallaBrazos != null)
        mallaBrazos.SetActive(false);

    reproduciendo = false;

    // Dejamos el Animator nuevamente en reposo.
    animator.Play("Idle", 0, 0f);

    if (visualWhisky != null)
        visualWhisky.OcultarVaso();

    // Entregamos el vaso de whisky al jugador.
    if (ControladorMano3D.Instance != null && itemWhiskyServido != null)
    {
        ControladorMano3D.Instance.EquiparItem(itemWhiskyServido);

        Debug.Log("WHISKY: vaso equipado en la mano.");
    }
    else
    {
        Debug.LogWarning(
            "WHISKY: falta ControladorMano3D o Item Whisky Servido."
        );
    }
}

    public void MostrarVaso()
{
    Debug.Log("EVENTO: MOSTRAR VASO");

    if (visualWhisky != null)
        visualWhisky.MostrarVaso();
    else
        Debug.LogError("visualWhisky NO ESTÁ ASIGNADO");
}

    public void IniciarVertido()
    {
        Debug.Log("EVENTO: INICIAR VERTIDO");

        if (visualWhisky != null)
            visualWhisky.IniciarVertido();
        else
            Debug.LogError("visualWhisky NO ESTÁ ASIGNADO");
    }

    public void DetenerVertido()
    {
        Debug.Log("EVENTO: DETENER VERTIDO");

        if (visualWhisky != null)
            visualWhisky.DetenerVertido();
        else
            Debug.LogError("visualWhisky NO ESTÁ ASIGNADO");
    }
}
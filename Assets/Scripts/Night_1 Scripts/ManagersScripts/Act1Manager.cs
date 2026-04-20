using UnityEngine;
using TMPro;
using System.Collections;
using JetBrains.Annotations;
using Unity.VisualScripting; // Necesario para usar Corrutinas (IEnumerator)

public class Act1Manager : MonoBehaviour
{
    public enum ActoState { Limpieza, Servicio, ElQuiebre }
    public ActoState estadoActual = ActoState.Limpieza;

    [Header("UI y Diálogos")]
    public TextMeshProUGUI textoSubtitulos;

    [Header("Referencias de Escena")]
    public GameObject grupoClientes; 
    public AudioSource sonidoGolpeSuelo; 

    [Header("Configuración")]
    public int totalSillas = 3; 
    private int sillasAcomodadas = 0;

    [Header("Efectos")]

    public EffectoParpadeo effectoParpadeo; //arrastra el objeto con el sccript 

    [Header("Progreso de servicio")]
    public int ClientesParaAtender = 2;
    private int ClientesAtendidos = 0;
    public GameObject puertaSotano; //referencia a la puerta para bloquearla

    public void ClienteAtendido()
    {
        ClientesAtendidos ++;
    }

    public bool ListoParaSotano()
    {
        return ClientesAtendidos >= ClientesParaAtender;
    }

    public void HabilitarSotano()
{
    // cambiar el estado o simplemente permitir la interacción con la puerta
    Debug.Log("Lucas ya puede entrar a la cocina/sótano.");
}
        
    
    void Start()
    {
        estadoActual = ActoState.Limpieza;
        if (grupoClientes != null) grupoClientes.SetActive(false);
        MostrarDialogo("Hay que dejar todo listo antes de abrir...");
    }

    public void SillaCompletada()
    {
        sillasAcomodadas++;
        Debug.Log("Sillas: " + sillasAcomodadas + "/" + totalSillas);

        if (sillasAcomodadas >= totalSillas && estadoActual == ActoState.Limpieza)
        {
            StartCoroutine(SecuenciaTransicionSuelo());
        }
    }

    // Usamos un IEnumerator para manejar los tiempos de forma secuencial
    IEnumerator SecuenciaTransicionSuelo()
{
    yield return new WaitForSeconds(4f);
    // 1. Empieza el sonido del golpe sordo
    if (sonidoGolpeSuelo != null)
    {
        sonidoGolpeSuelo.Play();

        yield return new WaitForSeconds(2f);

        // Lucas reacciona
        MostrarDialogo("¿Qué fue eso? Estas cañerías están cada vez peor...");

        // Esperamos un segundo de silencio tenso después del golpe
        yield return new WaitForSeconds(2f);
    }

    yield return new WaitForSeconds(4f);

    // 2. Lanzamos el efecto de parpadeo (que dura X segundos)
    if (effectoParpadeo != null)
    {
        effectoParpadeo.IniciarParpadeo();
    }

    // Esperamos a que el parpadeo esté por terminar (por ejemplo, antes del apagón final)
    // Si el parpadeo dura 1.5s en total, esperamos 1s
    yield return new WaitForSeconds(1f);

    // 3. Activamos a los clientes MIENTRAS la luz está parpadeando
    if (grupoClientes != null) grupoClientes.SetActive(true);

    // Esperamos un poquito más para asegurar que el parpadeo terminó
    yield return new WaitForSeconds(0.5f);

    // 4. Cambiamos de estado y Lucas habla
    estadoActual = ActoState.Servicio;
    MostrarDialogo("Clientes ?... a trabajar.");
}

    void AparecerClientes()
    {
        estadoActual = ActoState.Servicio;
        if (grupoClientes != null) grupoClientes.SetActive(true);
        
        // Diálogo de inicio de jornada
        MostrarDialogo("Clientes ?... a trabajar.");
    }

    public void MostrarDialogo(string mensaje)
    {
        if (textoSubtitulos != null)
        {
            textoSubtitulos.text = mensaje;
            CancelInvoke("LimpiarTexto");
            Invoke("LimpiarTexto", 4f);
        }
    }

    void LimpiarTexto() => textoSubtitulos.text = "";
}
using UnityEngine;

public class ControlLinterna : MonoBehaviour
{
    [Header("Componentes de Luz")]
    public GameObject luzLinterna;     // Tu Spot Light (Light) común
    public GameObject luzVolumetrica;  // Tu Spot Light (1) volumétrica

    [Header("Audio")]
    public AudioSource fuenteAudio;
    public AudioClip sonidoClick;

    private bool estaPrendida = false;
    private bool tieneLaLinterna = false;

    void Start()
    {
        // VITAL: Forzamos el apagado físico de ambas al arrancar la escena
        // Así podés dejarlas prendidas en el Inspector para que no se te rompa el mapeo
        if (luzLinterna != null) luzLinterna.SetActive(false);
        if (luzVolumetrica != null) luzVolumetrica.SetActive(false);
        
        estaPrendida = false;
        tieneLaLinterna = true;
    }

    public void HabilitarLinterna()
    {
        tieneLaLinterna = true;
        // Al levantar la botella arranca apagada hasta que Lucas pulse la F
        estaPrendida = false; 
        if (luzLinterna != null) luzLinterna.SetActive(false);
        if (luzVolumetrica != null) luzVolumetrica.SetActive(false);
        Debug.Log("SISTEMA: Linterna en mano y lista.");
    }

    void Update()
    {
        if (!tieneLaLinterna) return;

        if (Input.GetKeyDown(KeyCode.F))
        {
            AlternarLinterna();
        }
    }

    void AlternarLinterna()
    {
        estaPrendida = !estaPrendida;

        // Cambiamos el estado de las DOS luces al mismo tiempo
        if (luzLinterna != null) luzLinterna.SetActive(estaPrendida);
        if (luzVolumetrica != null) luzVolumetrica.SetActive(estaPrendida);

        // Sonido de feedback de la linterna vieja
        if (fuenteAudio != null && sonidoClick != null)
        {
            fuenteAudio.PlayOneShot(sonidoClick);
        }

        Debug.Log($"Linterna General: {(estaPrendida ? "ENCENDIDA" : "APAGADA")}");
    }
}
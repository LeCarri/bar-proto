using UnityEngine;

public class ControlLinterna : MonoBehaviour
{
    public Light luzLinterna; // Arrastrá el componente Light acá
    public GameObject luzVolumetrica; // Opcional (si tenés un haz de luz visual)
    private bool encendida = false;

    // Sonidos (opcional)
    public AudioSource fuenteAudio;
    public AudioClip sonidoClick;

    void Update()
    {
        // Detecta si Lucas presiona el botón (Click izquierdo o F)
        if (Input.GetKeyDown(KeyCode.F))
        {
            AlternarLinterna();
        }
    }

    void AlternarLinterna()
    {
        encendida = !encendida;
        
        // Prendemos/Apagamos la luz
        if (luzLinterna != null) luzLinterna.enabled = encendida;
        
        // Prendemos/Apagamos el haz visual si tenés uno
        if (luzVolumetrica != null) luzVolumetrica.SetActive(encendida);

        // Sonido de click
        if (fuenteAudio != null && sonidoClick != null)
        {
            fuenteAudio.PlayOneShot(sonidoClick);
        }
    }
}
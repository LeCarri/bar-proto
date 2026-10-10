using UnityEngine;

public class PuertaSotanoAct3 : MonoBehaviour, IInteractable
{
    [Header("Animación de apertura")]
    public float anguloApertura = 90f;
    public float velocidadApertura = 30f;

    [Header("Audio")]
    public AudioSource audioSourcePuerta;
    public AudioClip sonidoPuertaRechinado;

    [Header("Estado")]
    private bool abierta = false;
    private bool abriendo = false;

    private Quaternion rotacionObjetivo;

    public bool CanInteract()
    {
        return !abierta && !abriendo;
    }

    public void Interact()
    {
        if (abierta || abriendo)
            return;

        abierta = true;
        abriendo = true;

        rotacionObjetivo =
            transform.rotation *
            Quaternion.Euler(0f, anguloApertura, 0f);

        
        if (audioSourcePuerta != null &&
            sonidoPuertaRechinado != null)
        {
            audioSourcePuerta.PlayOneShot(sonidoPuertaRechinado);
        }

        Debug.Log("[PuertaSotanoAct3] Abriendo puerta.");
    }

    private void Update()
    {
        if (!abriendo)
            return;

        transform.rotation = Quaternion.RotateTowards(
            transform.rotation,
            rotacionObjetivo,
            velocidadApertura * Time.deltaTime
        );

        if (Quaternion.Angle(
            transform.rotation,
            rotacionObjetivo) < 0.5f)
        {
            transform.rotation = rotacionObjetivo;
            abriendo = false;
        }
    }

    public string GetDescription()
    {
        if (abierta)
            return "";

        return "Presiona [E] para abrir la puerta del sótano";
    }
}
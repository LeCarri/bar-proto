using System.Collections;
using UnityEngine;

public class DistorsionPasilloAct3 : MonoBehaviour
{
    [Header("Configuración")]
    public GameObject jugador;

    [Header("Audio")]
    public AudioSource audioDistorsion;

    [Header("Efecto visual")]
    public GameObject efectoGlitch;

    [Header("Duración")]
    public float duracionDistorsion = 3f;

    private bool activado = false;


    private void Start()
    {
        gameObject.SetActive(false);
    }


    public void ActivarTrigger()
    {
        gameObject.SetActive(true);

        Debug.Log("[Act3] Trigger de distorsión activado.");
    }


    private void OnTriggerEnter(Collider other)
    {
        if (activado)
            return;

        if (jugador != null && other.gameObject != jugador)
            return;

        if (Act3Manager.Instance == null)
            return;

        if (!Act3Manager.Instance.elementosGuardados)
            return;

        activado = true;

        Debug.Log("[Act3] Lucas activó la distorsión.");

        StartCoroutine(ReproducirDistorsion());
    }


    private IEnumerator ReproducirDistorsion()
    {
        if (audioDistorsion != null)
        {
            audioDistorsion.Play();
        }

        if (efectoGlitch != null)
        {
            efectoGlitch.SetActive(true);
        }

        yield return new WaitForSeconds(duracionDistorsion);

        if (audioDistorsion != null)
        {
            audioDistorsion.Stop();
        }

        if (efectoGlitch != null)
        {
            efectoGlitch.SetActive(false);
        }

        Debug.Log("[Act3] Distorsión finalizada.");
    }
}
using System.Collections;
using UnityEngine;
using UnityEngine.Rendering;

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
    private bool habilitado = false;

    private Volume volumenDistorsion;

    private void Awake()
    {
        habilitado = false;
        activado = false;

        
        if (audioDistorsion != null)
        {
            audioDistorsion.playOnAwake = false;
            audioDistorsion.Stop();
        }

        
        if (efectoGlitch != null)
        {
            volumenDistorsion = efectoGlitch.GetComponent<Volume>();

            if (volumenDistorsion != null)
            {
                volumenDistorsion.weight = 0f;
            }
        }
    }

    public void ActivarTrigger()
    {
        habilitado = true;

        Debug.Log("[Act3] Trigger de distorsión habilitado.");
    }

    private void OnTriggerEnter(Collider other)
    {
        if (activado)
            return;

        if (!habilitado)
            return;

        if (jugador != null && other.transform.root != jugador.transform.root)
            return;

        if (Act3Manager.Instance == null)
            return;

        if (!Act3Manager.Instance.elementosGuardados)
            return;

        activado = true;

        Debug.Log("[Act3] Lucas activó la distorsión del pasillo.");

        StartCoroutine(ReproducirDistorsion());
    }

    private IEnumerator ReproducirDistorsion()
    {
        
        if (audioDistorsion != null)
        {
            audioDistorsion.Play();
        }

        
        if (volumenDistorsion != null)
        {
            volumenDistorsion.weight = 1f;
        }

       
        yield return new WaitForSeconds(duracionDistorsion);

       
        if (audioDistorsion != null)
        {
            audioDistorsion.Stop();
        }

        
        if (volumenDistorsion != null)
        {
            volumenDistorsion.weight = 0f;
        }

        Debug.Log("[Act3] Distorsión finalizada.");
    }
}
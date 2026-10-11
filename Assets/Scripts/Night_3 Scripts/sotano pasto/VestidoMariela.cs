using System.Collections;
using UnityEngine;
using UnityEngine.Rendering;

public class ObjetoPerfumeSotano : MonoBehaviour, IInteractable
{
    [Header("Objeto")]
    [SerializeField] private GameObject objetoNormal;
    [SerializeField] private GameObject objetoConSangre;

    [Header("Efecto Glitch")]
    [SerializeField] private GameObject efectoGlitch;
    [SerializeField] private AudioSource audioDistorsion;
    [SerializeField] private float duracionGlitch = 0.5f;

    [Header("Diálogo")]
    [TextArea]
    [SerializeField]
    private string dialogoLucas =
        "Todavía tiene tu perfume...";

    private Volume volumenDistorsion;

    private bool interactuado;
    private bool secuenciaEnCurso;

    private void Start()
    {
        if (efectoGlitch != null)
        {
            volumenDistorsion = efectoGlitch.GetComponent<Volume>();

            if (volumenDistorsion != null)
                volumenDistorsion.weight = 0f;
        }

        if (audioDistorsion != null)
        {
            audioDistorsion.playOnAwake = false;
            audioDistorsion.Stop();
        }

        if (objetoConSangre != null)
            objetoConSangre.SetActive(false);
    }

    public void Interact()
    {
        if (!CanInteract())
            return;

        interactuado = true;
        secuenciaEnCurso = true;

        StartCoroutine(SecuenciaTerror());
    }

    private IEnumerator SecuenciaTerror()
    {
        Debug.Log("[ObjetoPerfumeSotano] Comenzando secuencia.");

        if (SotanoManager.Instance != null)
        {
            SotanoManager.Instance.MostrarDialogo(dialogoLucas);
        }
        else
        {
            Debug.LogError(
                "[ObjetoPerfumeSotano] SotanoManager.Instance es null."
            );
        }

        // Dar tiempo para leer el diálogo.
        yield return new WaitForSeconds(1f);

        // Activar el glitch y su sonido.
        if (audioDistorsion != null)
            audioDistorsion.Play();

        if (volumenDistorsion != null)
            volumenDistorsion.weight = 1f;

        yield return new WaitForSeconds(duracionGlitch);

        // Cambiar al vestido ensangrentado.
        if (objetoNormal != null)
            objetoNormal.SetActive(false);

        if (objetoConSangre != null)
            objetoConSangre.SetActive(true);

        // Desactivar el glitch y el sonido.
        if (audioDistorsion != null)
            audioDistorsion.Stop();

        if (volumenDistorsion != null)
            volumenDistorsion.weight = 0f;

        // La interacción queda completada.
        secuenciaEnCurso = false;

        Debug.Log("[ObjetoPerfumeSotano] Secuencia terminada.");
    }

    public string GetDescription()
    {
        return CanInteract() ? "Examinar" : "";
    }

    public bool CanInteract()
    {
        return !interactuado && !secuenciaEnCurso;
    }
}
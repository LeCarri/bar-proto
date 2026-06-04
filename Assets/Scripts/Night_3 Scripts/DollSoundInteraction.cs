using System.Collections;
using TMPro;
using UnityEngine;

public class DollSoundInteraction : MonoBehaviour, IInteractable
{
    [Header("Configuración")]
    [SerializeField] private string interactionDescription = "Presiona [E] para tocar la muñeca";

    [Header("Audio")]
    [SerializeField] private AudioSource dollAudioSource;
    [SerializeField] private AudioClip interactionClip;

    [Header("Diálogo")]
    [Tooltip("Panel de diálogo existente. Usá el mismo que funciona en el portarretrato, por ejemplo: FondoDialogo.")]
    [SerializeField] private GameObject panelDialogo;

    [Tooltip("Texto TMP del diálogo existente. Usá el mismo que funciona en el portarretrato, por ejemplo: Dialogue.")]
    [SerializeField] private TMP_Text textoDialogo;

    [TextArea(1, 3)]
    [SerializeField] private string fraseLucas = "La muñeca de mi hija...";

    [SerializeField] private float delayFrase = 0.25f;
    [SerializeField] private float tiempoFraseVisible = 3f;

    private Coroutine rutinaDialogo;

    private void Awake()
    {
        if (dollAudioSource == null)
            dollAudioSource = GetComponent<AudioSource>();

        if (dollAudioSource == null)
            dollAudioSource = GetComponentInChildren<AudioSource>();

        if (dollAudioSource != null)
        {
            dollAudioSource.playOnAwake = false;
            dollAudioSource.loop = false;
            dollAudioSource.Stop();
        }
    }

    public void Interact()
    {
        ReproducirRisa();
        MostrarFraseLucas();
    }

    public bool CanInteract()
    {
        return true;
    }

    public string GetDescription()
    {
        return interactionDescription;
    }

    private void ReproducirRisa()
    {
        if (dollAudioSource == null)
        {
            Debug.LogWarning("[DollSoundInteraction] Falta asignar Doll Audio Source.");
            return;
        }

        AudioClip clipToPlay = interactionClip;

        if (clipToPlay == null)
            clipToPlay = dollAudioSource.clip;

        if (clipToPlay == null)
        {
            Debug.LogWarning("[DollSoundInteraction] Falta asignar Interaction Clip o un clip en el AudioSource.");
            return;
        }

        dollAudioSource.Stop();
        dollAudioSource.PlayOneShot(clipToPlay);

        Debug.Log("[DollSoundInteraction] La muñeca se rió.");
    }

    private void MostrarFraseLucas()
    {
        if (rutinaDialogo != null)
            StopCoroutine(rutinaDialogo);

        rutinaDialogo = StartCoroutine(DialogoRoutine());
    }

    private IEnumerator DialogoRoutine()
    {
        yield return new WaitForSeconds(delayFrase);

        if (panelDialogo != null)
            panelDialogo.SetActive(true);

        if (textoDialogo != null)
        {
            textoDialogo.gameObject.SetActive(true);
            textoDialogo.text = fraseLucas;
            textoDialogo.enabled = true;
            textoDialogo.alpha = 1f;
        }

        Debug.Log("[DollSoundInteraction] Mostrando diálogo de muñeca: " + fraseLucas);

        yield return new WaitForSeconds(tiempoFraseVisible);

        if (textoDialogo != null)
            textoDialogo.text = "";

        if (panelDialogo != null)
            panelDialogo.SetActive(false);

        rutinaDialogo = null;
    }
}
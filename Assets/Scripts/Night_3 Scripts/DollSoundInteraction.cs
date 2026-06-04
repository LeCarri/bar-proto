using UnityEngine;

public class DollSoundInteraction : MonoBehaviour, IInteractable
{
    [Header("Configuración")]
    [SerializeField] private string playerTag = "Player";
    [SerializeField] private KeyCode interactKey = KeyCode.E;
    [SerializeField] private string interactionDescription = "Presiona [E] para escuchar";

    [Header("Audio")]
    [SerializeField] private AudioSource dollAudioSource;
    [SerializeField] private AudioClip interactionClip;
    [SerializeField] private AudioClip exitClip;

    [Header("Opciones")]
    [SerializeField] private bool canInteractOnlyOnce = true;
    [SerializeField] private bool playAgainWhenPlayerLeaves = true;
    [SerializeField] private float exitSoundDelay = 0.2f;

    private bool playerInside = false;
    private bool alreadyInteracted = false;
    private bool exitSoundPlayed = false;

    private void Awake()
    {
        if (dollAudioSource == null)
        {
            dollAudioSource = GetComponent<AudioSource>();
        }

        if (dollAudioSource == null)
        {
            dollAudioSource = GetComponentInChildren<AudioSource>();
        }

        if (dollAudioSource != null)
        {
            dollAudioSource.playOnAwake = false;
            dollAudioSource.loop = false;
            dollAudioSource.Stop();
        }
    }

    private void Update()
    {
        if (!playerInside)
            return;

        if (canInteractOnlyOnce && alreadyInteracted)
            return;

        if (Input.GetKeyDown(interactKey))
        {
            PlayInteractionSound();
        }
    }

    public void Interact()
    {
        if (!CanInteract())
            return;

        PlayInteractionSound();
    }

    public bool CanInteract()
    {
        return !canInteractOnlyOnce || !alreadyInteracted;
    }

    public string GetDescription()
    {
        return interactionDescription;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag(playerTag))
            return;

        playerInside = true;
    }

    private void OnTriggerExit(Collider other)
    {
        if (!other.CompareTag(playerTag))
            return;

        playerInside = false;

        if (!playAgainWhenPlayerLeaves)
            return;

        if (!alreadyInteracted)
            return;

        if (exitSoundPlayed)
            return;

        exitSoundPlayed = true;

        Invoke(nameof(PlayExitSound), exitSoundDelay);
    }

    private void PlayInteractionSound()
    {
        alreadyInteracted = true;

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

        Debug.Log("[DollSoundInteraction] Sonido de muñeca reproducido al interactuar.");
    }

    private void PlayExitSound()
    {
        if (dollAudioSource == null)
            return;

        AudioClip clipToPlay = exitClip;

        if (clipToPlay == null)
            clipToPlay = interactionClip;

        if (clipToPlay == null)
            clipToPlay = dollAudioSource.clip;

        if (clipToPlay == null)
            return;

        dollAudioSource.Stop();
        dollAudioSource.PlayOneShot(clipToPlay);

        Debug.Log("[DollSoundInteraction] Sonido de muñeca reproducido al alejarse.");
    }
}

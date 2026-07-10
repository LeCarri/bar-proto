using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class FinalEndingSequence : MonoBehaviour
{
    [Header("Activación")]
    [SerializeField] private string playerTag = "Player";

    [Header("Jugador")]
    [SerializeField] private GameObject player;
    [SerializeField] private MonoBehaviour playerMovementScript;
    [SerializeField] private MonoBehaviour playerLookScript;
    [SerializeField] private Camera playerCamera;

    [Header("HUDs del juego a desactivar")]
    [Tooltip("Arrastrá acá ParanoiaCanvas, ContenedorObjetivos y ParpadeoYDialogos.")]
    [SerializeField] private GameObject[] hudsToDisable;

    [Header("Opcional - apagar al final")]
    [SerializeField] private GameObject playerFlashlightObject;
    [SerializeField] private MonoBehaviour playerInteractionScript;
    [SerializeField] private MonoBehaviour sotanoManagerScript;

    [Header("Cámara final")]
    [SerializeField] private GameObject finalCameraObject;
    [SerializeField] private Camera finalCamera;
    [SerializeField] private Transform finalCameraTransform;
    [SerializeField] private Transform cameraStartPoint;
    [SerializeField] private Transform cameraEndPoint;
    [SerializeField] private float cameraMoveDuration = 7f;

    [Header("Pantalla negra")]
    [SerializeField] private CanvasGroup blackScreen;
    [SerializeField] private float fadeToFirstBlackDuration = 0.4f;
    [SerializeField] private float firstBlackDuration = 2.5f;
    [SerializeField] private float fadeFromBlackDuration = 1.5f;
    [SerializeField] private float finalFadeToBlackDuration = 2f;

    [Header("Diálogo final")]
    [SerializeField] private GameObject dialogPanel;
    [SerializeField] private TMP_Text dialogText;

    [TextArea(2, 5)]
    [SerializeField] private string finalDialog = "Perdón por la tardanza... el turno se hizo eterno. Ya podemos irnos.";

    [SerializeField] private float dialogDelayAfterCameraStarts = 1f;
    [SerializeField] private float dialogVisibleDuration = 5f;

    [Header("Luces de policía")]
    [SerializeField] private Light redPoliceLight;
    [SerializeField] private Light bluePoliceLight;
    [SerializeField] private float policeLightIntensity = 5f;
    [SerializeField] private float policeLightSpeed = 0.22f;

    [Header("Audios")]
    [SerializeField] private AudioSource sfxAudioSource;
    [SerializeField] private AudioSource sirenAudioSource;
    [SerializeField] private AudioSource finalMusicAudioSource;
    [SerializeField] private AudioClip doorKnocksClip;
    [SerializeField] private AudioClip gunshotClip;

    [Header("Panel final tipo intro")]
    [Tooltip("Arrastrá acá el IntroPanel que ya tiene el script IntroScreen.")]
    [SerializeField] private GameObject finalIntroPanel;

    [Tooltip("Solo se usa si el IntroScreen no cambia de escena solo.")]
    [SerializeField] private bool forceLoadHomeAfterDelay = false;

    [SerializeField] private float forceLoadHomeDelay = 8f;
    [SerializeField] private string homeSceneName = "Home";

    [Header("Tiempos finales")]
    [SerializeField] private float secondsAfterCameraMove = 1.5f;
    [SerializeField] private float secondsBeforeGunshot = 0.8f;
    [SerializeField] private float secondsAfterGunshot = 1.2f;

    private bool endingStarted = false;
    private Coroutine policeLightsCoroutine;
    private Coroutine dialogCoroutine;

    private void Awake()
    {
        if (finalCameraObject != null)
            finalCameraObject.SetActive(false);

        if (finalCamera != null)
            finalCamera.enabled = false;

        if (playerCamera != null)
            playerCamera.enabled = true;

        if (blackScreen != null)
        {
            blackScreen.gameObject.SetActive(true);
            blackScreen.alpha = 0f;
            blackScreen.interactable = false;
            blackScreen.blocksRaycasts = false;
        }

        HideDialog();

        if (finalIntroPanel != null)
            finalIntroPanel.SetActive(false);

        if (redPoliceLight != null)
            redPoliceLight.intensity = 0f;

        if (bluePoliceLight != null)
            bluePoliceLight.intensity = 0f;

        if (sfxAudioSource != null)
        {
            sfxAudioSource.playOnAwake = false;
            sfxAudioSource.Stop();
        }

        if (sirenAudioSource != null)
        {
            sirenAudioSource.playOnAwake = false;
            sirenAudioSource.loop = true;
            sirenAudioSource.Stop();
        }

        if (finalMusicAudioSource != null)
        {
            finalMusicAudioSource.playOnAwake = false;
            finalMusicAudioSource.loop = true;
            finalMusicAudioSource.Stop();
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (endingStarted)
            return;

        if (!other.CompareTag(playerTag))
            return;

        StartEnding();
    }

    public void StartEnding()
    {
        if (endingStarted)
            return;

        endingStarted = true;

        Collider triggerCollider = GetComponent<Collider>();

        if (triggerCollider != null)
            triggerCollider.enabled = false;

        Debug.Log("[FinalEndingSequence] Secuencia final iniciada.");

        StartCoroutine(EndingRoutine());
    }

    private IEnumerator EndingRoutine()
    {
        DisableGameplayHUDs();

        BlockPlayer();

        // 1. Pantalla negra inicial.
        yield return StartCoroutine(FadeCanvasGroup(blackScreen, 0f, 1f, fadeToFirstBlackDuration));

        // 2. Golpes en la puerta.
        PlayDoorKnocks();

        yield return new WaitForSeconds(firstBlackDuration);

        // 3. Cámara final.
        ActivateFinalCamera();

        // 4. Sirena y luces policiales.
        PlaySirens();
        policeLightsCoroutine = StartCoroutine(PoliceLightsRoutine());

        // 5. Sale la pantalla negra.
        yield return StartCoroutine(FadeCanvasGroup(blackScreen, 1f, 0f, fadeFromBlackDuration));

        // 6. Diálogo final mientras se aleja la cámara.
        dialogCoroutine = StartCoroutine(DialogRoutine());

        // 7. Cámara alejándose.
        yield return StartCoroutine(MoveFinalCameraRoutine());

        yield return new WaitForSeconds(secondsAfterCameraMove);

        HideDialog();

        // 8. Pantalla negra final.
        yield return StartCoroutine(FadeCanvasGroup(blackScreen, 0f, 1f, finalFadeToBlackDuration));

        StopPoliceLights();
        StopSirens();

        yield return new WaitForSeconds(secondsBeforeGunshot);

        // 9. Disparo.
        PlayGunshot();

        yield return new WaitForSeconds(secondsAfterGunshot);

        // 10. Se activa el panel final tipo intro.
        ShowFinalIntroPanel();

        Debug.Log("[FinalEndingSequence] Secuencia final terminada.");
    }

    private void DisableGameplayHUDs()
    {
        if (hudsToDisable == null)
            return;

        foreach (GameObject hud in hudsToDisable)
        {
            if (hud != null)
                hud.SetActive(false);
        }
    }

    private void BlockPlayer()
    {
        if (playerMovementScript != null)
            playerMovementScript.enabled = false;

        if (playerLookScript != null)
            playerLookScript.enabled = false;

        if (playerInteractionScript != null)
            playerInteractionScript.enabled = false;

        if (sotanoManagerScript != null)
            sotanoManagerScript.enabled = false;

        if (playerFlashlightObject != null)
            playerFlashlightObject.SetActive(false);

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    private void ActivateFinalCamera()
    {
        if (finalCameraTransform != null && cameraStartPoint != null)
        {
            finalCameraTransform.position = cameraStartPoint.position;
            finalCameraTransform.rotation = cameraStartPoint.rotation;
        }

        if (playerCamera != null)
            playerCamera.enabled = false;

        if (finalCameraObject != null)
            finalCameraObject.SetActive(true);

        if (finalCamera != null)
            finalCamera.enabled = true;

        Debug.Log("[FinalEndingSequence] Cámara final activada.");
    }

    private IEnumerator MoveFinalCameraRoutine()
    {
        if (finalCameraTransform == null || cameraStartPoint == null || cameraEndPoint == null)
        {
            Debug.LogWarning("[FinalEndingSequence] Falta asignar Final Camera Transform, Camera Start Point o Camera End Point.");
            yield break;
        }

        Debug.Log("[FinalEndingSequence] Movimiento de cámara iniciado.");

        float timer = 0f;

        Vector3 startPosition = cameraStartPoint.position;
        Quaternion startRotation = cameraStartPoint.rotation;

        Vector3 endPosition = cameraEndPoint.position;
        Quaternion endRotation = cameraEndPoint.rotation;

        while (timer < cameraMoveDuration)
        {
            timer += Time.deltaTime;

            float t = timer / cameraMoveDuration;
            t = Mathf.Clamp01(t);
            t = Mathf.SmoothStep(0f, 1f, t);

            finalCameraTransform.position = Vector3.Lerp(startPosition, endPosition, t);
            finalCameraTransform.rotation = Quaternion.Slerp(startRotation, endRotation, t);

            yield return null;
        }

        finalCameraTransform.position = endPosition;
        finalCameraTransform.rotation = endRotation;

        Debug.Log("[FinalEndingSequence] Movimiento de cámara terminado.");
    }

    private IEnumerator DialogRoutine()
    {
        yield return new WaitForSeconds(dialogDelayAfterCameraStarts);

        ShowDialog(finalDialog);

        yield return new WaitForSeconds(dialogVisibleDuration);

        HideDialog();
    }

    private void ShowDialog(string textToShow)
    {
        if (dialogPanel != null)
            dialogPanel.SetActive(true);

        if (dialogText != null)
            dialogText.text = textToShow;
    }

    private void HideDialog()
    {
        if (dialogCoroutine != null)
        {
            StopCoroutine(dialogCoroutine);
            dialogCoroutine = null;
        }

        if (dialogText != null)
            dialogText.text = "";

        if (dialogPanel != null)
            dialogPanel.SetActive(false);
    }

    private IEnumerator FadeCanvasGroup(CanvasGroup canvasGroup, float from, float to, float duration)
    {
        if (canvasGroup == null)
            yield break;

        canvasGroup.gameObject.SetActive(true);
        canvasGroup.alpha = from;

        if (to > 0f)
        {
            canvasGroup.blocksRaycasts = true;
            canvasGroup.interactable = true;
        }

        float timer = 0f;

        while (timer < duration)
        {
            timer += Time.deltaTime;

            float t = timer / duration;
            t = Mathf.Clamp01(t);

            canvasGroup.alpha = Mathf.Lerp(from, to, t);

            yield return null;
        }

        canvasGroup.alpha = to;

        if (to <= 0f)
        {
            canvasGroup.blocksRaycasts = false;
            canvasGroup.interactable = false;
        }
    }

private void ShowFinalIntroPanel()
{
    if (SaveManager.Instance != null)
    {
        SaveManager.Instance.WipeData();
    }

    if (finalMusicAudioSource != null)
        finalMusicAudioSource.Play();

    if (blackScreen != null)
    {
        blackScreen.alpha = 0f;
        blackScreen.interactable = false;
        blackScreen.blocksRaycasts = false;
        blackScreen.gameObject.SetActive(false);
    }

    if (finalIntroPanel == null)
    {
        Debug.LogError("[FinalEndingSequence] Falta asignar Final Intro Panel en el Inspector.");
        StartCoroutine(ForceLoadHomeRoutine());
        return;
    }

    MonoBehaviour[] behaviours = finalIntroPanel.GetComponentsInChildren<MonoBehaviour>(true);

    foreach (MonoBehaviour behaviour in behaviours)
    {
        if (behaviour != null && behaviour.GetType().Name == "IntroScreen")
            behaviour.enabled = false;
    }

    finalIntroPanel.SetActive(true);
    finalIntroPanel.transform.SetAsLastSibling();

    Canvas[] canvases = finalIntroPanel.GetComponentsInChildren<Canvas>(true);

    foreach (Canvas canvas in canvases)
    {
        canvas.enabled = true;
        canvas.overrideSorting = true;
        canvas.sortingOrder = 9999;
    }

    CanvasGroup[] groups = finalIntroPanel.GetComponentsInChildren<CanvasGroup>(true);

    foreach (CanvasGroup group in groups)
    {
        group.alpha = 1f;
        group.interactable = true;
        group.blocksRaycasts = true;
    }

    Cursor.lockState = CursorLockMode.None;
    Cursor.visible = true;

    Debug.Log("[FinalEndingSequence] Final Intro Panel activado.");

    StartCoroutine(ForceLoadHomeRoutine());
}
private IEnumerator ForceLoadHomeRoutine()
{
    yield return new WaitForSeconds(forceLoadHomeDelay);

    Time.timeScale = 1f;

    if (SaveManager.Instance != null)
    {
        SaveManager.Instance.WipeData();
    }

    SceneManager.LoadScene(homeSceneName);
}
    private void PlayDoorKnocks()
    {
        if (sfxAudioSource != null && doorKnocksClip != null)
            sfxAudioSource.PlayOneShot(doorKnocksClip);
        else
            Debug.LogWarning("[FinalEndingSequence] Falta SFXAudioSource o DoorKnocksClip.");
    }

    private void PlayGunshot()
    {
        if (sfxAudioSource != null && gunshotClip != null)
            sfxAudioSource.PlayOneShot(gunshotClip);
        else
            Debug.LogWarning("[FinalEndingSequence] Falta SFXAudioSource o GunshotClip.");
    }

    private void PlaySirens()
    {
        if (sirenAudioSource != null)
        {
            sirenAudioSource.loop = true;
            sirenAudioSource.Play();
        }
        else
        {
            Debug.LogWarning("[FinalEndingSequence] Falta SirenAudioSource.");
        }
    }

    private void StopSirens()
    {
        if (sirenAudioSource != null)
            sirenAudioSource.Stop();
    }

    private IEnumerator PoliceLightsRoutine()
    {
        while (true)
        {
            if (redPoliceLight != null)
                redPoliceLight.intensity = policeLightIntensity;

            if (bluePoliceLight != null)
                bluePoliceLight.intensity = 0f;

            yield return new WaitForSeconds(policeLightSpeed);

            if (redPoliceLight != null)
                redPoliceLight.intensity = 0f;

            if (bluePoliceLight != null)
                bluePoliceLight.intensity = policeLightIntensity;

            yield return new WaitForSeconds(policeLightSpeed);
        }
    }

    private void StopPoliceLights()
    {
        if (policeLightsCoroutine != null)
        {
            StopCoroutine(policeLightsCoroutine);
            policeLightsCoroutine = null;
        }

        if (redPoliceLight != null)
            redPoliceLight.intensity = 0f;

        if (bluePoliceLight != null)
            bluePoliceLight.intensity = 0f;
    }
    
    public void VolverAlMenuPrincipal()
    {
    Time.timeScale = 1f;

    if (SaveManager.Instance != null)
    {
        SaveManager.Instance.WipeData();
    }

    SceneManager.LoadScene("Home");
    }
}
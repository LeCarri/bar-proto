using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class FinalEndingSequence : MonoBehaviour
{
    [Header("Activación")]
    [SerializeField] private string playerTag = "Player";

    [Header("Jugador")]
    [SerializeField] private GameObject player;
    [SerializeField] private MonoBehaviour playerMovementScript;
    [SerializeField] private MonoBehaviour playerLookScript;
    [SerializeField] private Camera playerCamera;

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

    [Header("Pantalla de victoria diseñada")]
    [Tooltip("Arrastrá acá el prefab azul VictoryScreen desde Project, no desde la jerarquía.")]
    [SerializeField] private GameObject victoryScreenPrefab;

    [SerializeField] private float victoryFadeDuration = 2f;
    [SerializeField] private string mainMenuSceneName = "Home";

    [Header("Opcional - créditos")]
    [Tooltip("Si tu botón de créditos ya funciona con su propio script, podés dejar esto vacío.")]
    [SerializeField] private GameObject creditsPanelPrefab;

    [Header("Tiempos finales")]
    [SerializeField] private float secondsAfterCameraMove = 1.5f;
    [SerializeField] private float secondsBeforeGunshot = 0.8f;
    [SerializeField] private float secondsAfterGunshot = 1.2f;

    private bool endingStarted = false;
    private Coroutine policeLightsCoroutine;
    private Coroutine dialogCoroutine;

    private GameObject spawnedVictoryScreen;
    private CanvasGroup spawnedVictoryCanvasGroup;
    private GameObject spawnedCreditsPanel;

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
        BlockPlayer();

        yield return StartCoroutine(FadeCanvasGroup(blackScreen, 0f, 1f, fadeToFirstBlackDuration));

        PlayDoorKnocks();

        yield return new WaitForSeconds(firstBlackDuration);

        ActivateFinalCamera();

        PlaySirens();
        policeLightsCoroutine = StartCoroutine(PoliceLightsRoutine());

        yield return StartCoroutine(FadeCanvasGroup(blackScreen, 1f, 0f, fadeFromBlackDuration));

        dialogCoroutine = StartCoroutine(DialogRoutine());

        yield return StartCoroutine(MoveFinalCameraRoutine());

        yield return new WaitForSeconds(secondsAfterCameraMove);

        HideDialog();

        yield return StartCoroutine(FadeCanvasGroup(blackScreen, 0f, 1f, finalFadeToBlackDuration));

        StopPoliceLights();
        StopSirens();

        yield return new WaitForSeconds(secondsBeforeGunshot);

        PlayGunshot();

        yield return new WaitForSeconds(secondsAfterGunshot);

        PlayFinalMusic();

        yield return StartCoroutine(ShowDesignedVictoryScreen());

        Debug.Log("[FinalEndingSequence] Secuencia final terminada.");
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

    private IEnumerator ShowDesignedVictoryScreen()
    {
        if (victoryScreenPrefab == null)
        {
            Debug.LogError("[FinalEndingSequence] Falta asignar Victory Screen Prefab en el Inspector.");
            yield break;
        }

        Debug.Log("[FinalEndingSequence] Instanciando VictoryScreen diseñado.");

        spawnedVictoryScreen = Instantiate(victoryScreenPrefab);
        spawnedVictoryScreen.name = "VictoryScreen_Final";
        spawnedVictoryScreen.SetActive(true);

        ForceObjectAndChildrenActive(spawnedVictoryScreen);
        ForceCanvasVisible(spawnedVictoryScreen);
        ForceRectTransformSafe(spawnedVictoryScreen);
        ForceCanvasGroupsVisible(spawnedVictoryScreen);

        spawnedVictoryCanvasGroup = spawnedVictoryScreen.GetComponent<CanvasGroup>();

        if (spawnedVictoryCanvasGroup == null)
            spawnedVictoryCanvasGroup = spawnedVictoryScreen.AddComponent<CanvasGroup>();

        spawnedVictoryCanvasGroup.alpha = 0f;
        spawnedVictoryCanvasGroup.interactable = true;
        spawnedVictoryCanvasGroup.blocksRaycasts = true;

        FixVictoryButtons(spawnedVictoryScreen);

        if (blackScreen != null)
        {
            blackScreen.alpha = 1f;
            blackScreen.interactable = false;
            blackScreen.blocksRaycasts = false;
        }

        float timer = 0f;

        while (timer < victoryFadeDuration)
        {
            timer += Time.deltaTime;

            float t = timer / victoryFadeDuration;
            t = Mathf.Clamp01(t);

            spawnedVictoryCanvasGroup.alpha = Mathf.Lerp(0f, 1f, t);

            if (blackScreen != null)
                blackScreen.alpha = Mathf.Lerp(1f, 0f, t);

            yield return null;
        }

        spawnedVictoryCanvasGroup.alpha = 1f;
        spawnedVictoryCanvasGroup.interactable = true;
        spawnedVictoryCanvasGroup.blocksRaycasts = true;

        if (blackScreen != null)
        {
            blackScreen.alpha = 0f;
            blackScreen.interactable = false;
            blackScreen.blocksRaycasts = false;
            blackScreen.gameObject.SetActive(false);
        }

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        Debug.Log("[FinalEndingSequence] VictoryScreen diseñado mostrado correctamente.");
    }

    private void ForceObjectAndChildrenActive(GameObject root)
    {
        if (root == null)
            return;

        root.SetActive(true);

        Transform[] children = root.GetComponentsInChildren<Transform>(true);

        foreach (Transform child in children)
            child.gameObject.SetActive(true);
    }

    private void ForceCanvasVisible(GameObject root)
    {
        Canvas[] canvases = root.GetComponentsInChildren<Canvas>(true);

        if (canvases.Length == 0)
        {
            Canvas canvas = root.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.overrideSorting = true;
            canvas.sortingOrder = 9999;

            root.AddComponent<CanvasScaler>();
            root.AddComponent<GraphicRaycaster>();
            return;
        }

        foreach (Canvas canvas in canvases)
        {
            canvas.enabled = true;
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.overrideSorting = true;
            canvas.sortingOrder = 9999;
        }
    }

    private void ForceRectTransformSafe(GameObject root)
    {
        RectTransform rect = root.GetComponent<RectTransform>();

        if (rect == null)
            return;

        rect.SetParent(null, false);
        rect.localScale = Vector3.one;
        rect.localRotation = Quaternion.identity;

        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;
        rect.anchoredPosition = Vector2.zero;
    }

    private void ForceCanvasGroupsVisible(GameObject root)
    {
        CanvasGroup[] canvasGroups = root.GetComponentsInChildren<CanvasGroup>(true);

        foreach (CanvasGroup group in canvasGroups)
        {
            group.alpha = 1f;
            group.interactable = true;
            group.blocksRaycasts = true;
            group.ignoreParentGroups = false;
        }
    }

    private void FixVictoryButtons(GameObject root)
    {
        Button[] buttons = root.GetComponentsInChildren<Button>(true);

        foreach (Button button in buttons)
        {
            string buttonName = button.gameObject.name.ToLower();
            string buttonText = GetButtonText(button).ToLower();

            if (buttonName.Contains("menu") || buttonText.Contains("menú") || buttonText.Contains("menu"))
            {
                button.onClick.RemoveAllListeners();
                button.onClick.AddListener(ReturnToMainMenu);
                Debug.Log("[FinalEndingSequence] Botón de volver al menú conectado.");
            }

            if (buttonName.Contains("credit") || buttonText.Contains("crédit") || buttonText.Contains("credit"))
            {
                button.onClick.RemoveAllListeners();
                button.onClick.AddListener(ShowCredits);
                Debug.Log("[FinalEndingSequence] Botón de créditos conectado.");
            }
        }
    }

    private string GetButtonText(Button button)
    {
        TMP_Text tmp = button.GetComponentInChildren<TMP_Text>(true);

        if (tmp != null)
            return tmp.text;

        Text text = button.GetComponentInChildren<Text>(true);

        if (text != null)
            return text.text;

        return "";
    }

    private void ShowCredits()
    {
        Debug.Log("[FinalEndingSequence] Botón Créditos presionado.");

        if (creditsPanelPrefab == null)
        {
            Debug.LogWarning("[FinalEndingSequence] No hay Credits Panel Prefab asignado.");
            return;
        }

        if (spawnedCreditsPanel != null)
        {
            spawnedCreditsPanel.SetActive(true);
            return;
        }

        spawnedCreditsPanel = Instantiate(creditsPanelPrefab);
        spawnedCreditsPanel.name = "CreditsPanel_Final";

        ForceObjectAndChildrenActive(spawnedCreditsPanel);
        ForceCanvasVisible(spawnedCreditsPanel);
        ForceRectTransformSafe(spawnedCreditsPanel);
        ForceCanvasGroupsVisible(spawnedCreditsPanel);
    }

    private void ReturnToMainMenu()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(mainMenuSceneName);
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

    private void PlayFinalMusic()
    {
        if (finalMusicAudioSource != null)
        {
            finalMusicAudioSource.loop = true;
            finalMusicAudioSource.Play();
        }
        else
        {
            Debug.LogWarning("[FinalEndingSequence] Falta FinalMusicAudioSource.");
        }
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
}
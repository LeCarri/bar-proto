using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class PauseManager : MonoBehaviour
{
    [Header("Panels")]
    [SerializeField] private GameObject pausePanel;
    [SerializeField] private GameObject settingsPanel;

    [Header("Buttons")]
    [SerializeField] private Button resumeButton;
    [SerializeField] private Button settingsButton;
    [SerializeField] private Button mainMenuButton;
    [SerializeField] private Button quitButton;

    [Header("UI SFX Audio")]
    [SerializeField] private AudioSource uiAudioSource;
    [SerializeField] private AudioClip hoverClip;
    [SerializeField] private AudioClip clickClip;
    [SerializeField] [Range(0f, 1f)] private float hoverVolume = 0.6f;
    [SerializeField] [Range(0f, 1f)] private float clickVolume = 0.8f;

    [Header("Scene")]
    [SerializeField] private string mainMenuSceneName = "Home";

    private static PauseManager current;
    private bool isPaused;
    private bool cursorVisibleBeforePause;
    private CursorLockMode cursorLockModeBeforePause;

    private void Awake()
    {
        if (current != null && current != this)
        {
            Destroy(gameObject);
            return;
        }

        current = this;
        EnsureAudioSource();
        RegisterButtonCallbacks();
        RegisterButtonAudio();
        SetPausePanels(false);
        Time.timeScale = 1f;
    }

    private void OnDestroy()
    {
        if (current == this)
        {
            current = null;
        }
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            TogglePause();
        }
    }

    public void TogglePause()
    {
        if (isPaused)
        {
            ResumeGame();
            return;
        }

        PauseGame();
    }

    public void PauseGame()
    {
        if (isPaused)
        {
            return;
        }

        cursorVisibleBeforePause = Cursor.visible;
        cursorLockModeBeforePause = Cursor.lockState;

        isPaused = true;
        Time.timeScale = 0f;
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;

        if (pausePanel != null)
        {
            pausePanel.SetActive(true);
        }

        if (settingsPanel != null)
        {
            settingsPanel.SetActive(false);
        }
    }

    public void ResumeGame()
    {
        if (!isPaused)
        {
            SetPausePanels(false);
            return;
        }

        isPaused = false;
        Time.timeScale = 1f;
        SetPausePanels(false);
        Cursor.visible = cursorVisibleBeforePause;
        Cursor.lockState = cursorLockModeBeforePause;
    }

    public void OpenSettings()
    {
        if (pausePanel != null)
        {
            pausePanel.SetActive(false);
        }

        if (settingsPanel != null)
        {
            settingsPanel.SetActive(true);
        }
    }

    public void CloseSettings()
    {
        if (settingsPanel != null)
        {
            settingsPanel.SetActive(false);
        }

        if (pausePanel != null)
        {
            pausePanel.SetActive(true);
        }
    }

    public void ReturnToMainMenu()
    {
        Time.timeScale = 1f;
        isPaused = false;
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
        SceneManager.LoadScene(mainMenuSceneName);
    }

    public void QuitGame()
    {
        Time.timeScale = 1f;
        Application.Quit();

#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif
    }

    public void PlayHoverSound()
    {
        PlayClip(hoverClip, hoverVolume);
    }

    public void PlayClickSound()
    {
        PlayClip(clickClip, clickVolume);
    }

    private void RegisterButtonCallbacks()
    {
        AddClickListener(resumeButton, ResumeGame);
        AddClickListener(settingsButton, OpenSettings);
        AddClickListener(mainMenuButton, ReturnToMainMenu);
        AddClickListener(quitButton, QuitGame);
    }

    private void RegisterButtonAudio()
    {
        Button[] buttons = GetComponentsInChildren<Button>(true);
        foreach (Button button in buttons)
        {
            if (button == null)
            {
                continue;
            }

            button.onClick.AddListener(PlayClickSound);

            PauseButtonAudioRelay relay = button.GetComponent<PauseButtonAudioRelay>();
            if (relay == null)
            {
                relay = button.gameObject.AddComponent<PauseButtonAudioRelay>();
            }

            relay.Initialize(this);
        }
    }

    private void EnsureAudioSource()
    {
        if (uiAudioSource == null)
        {
            uiAudioSource = GetComponent<AudioSource>();
        }

        if (uiAudioSource == null)
        {
            uiAudioSource = gameObject.AddComponent<AudioSource>();
        }

        uiAudioSource.playOnAwake = false;
        uiAudioSource.loop = false;
    }

    private void SetPausePanels(bool active)
    {
        if (pausePanel != null)
        {
            pausePanel.SetActive(active);
        }

        if (settingsPanel != null)
        {
            settingsPanel.SetActive(false);
        }
    }

    public void TogglePanel(GameObject panelToShow)
    {
        if (panelToShow == null)
        {
            return;
        }

        bool isCurrentlyActive = panelToShow.activeSelf;
        SetPausePanels(false);
        panelToShow.SetActive(!isCurrentlyActive);
    }

    private void AddClickListener(Button button, UnityEngine.Events.UnityAction action)
    {
        if (button == null)
        {
            return;
        }

        button.onClick.AddListener(action);
    }

    private void PlayClip(AudioClip clip, float volume)
    {
        if (uiAudioSource == null || clip == null)
        {
            return;
        }

        uiAudioSource.PlayOneShot(clip, volume);
    }
}

public class PauseButtonAudioRelay : MonoBehaviour, IPointerEnterHandler
{
    private PauseManager pauseManager;

    public void Initialize(PauseManager manager)
    {
        pauseManager = manager;
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (!isActiveAndEnabled || pauseManager == null)
        {
            return;
        }

        pauseManager.PlayHoverSound();
    }
}

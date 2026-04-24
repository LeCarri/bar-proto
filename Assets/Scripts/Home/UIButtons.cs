using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class UIButtons : MonoBehaviour
{
    [Header("UI SFX Audio")]
    [SerializeField] private AudioSource uiSfxAudioSource;
    [SerializeField] private AudioClip hoverClip;
    [SerializeField] private AudioClip clickClip;
    [SerializeField] [Range(0f, 1f)] private float hoverVolume = 0.6f;
    [SerializeField] [Range(0f, 1f)] private float clickVolume = 0.8f;

    [Header("Intro Screen")]
    [SerializeField] private GameObject introPanel;

    private readonly Dictionary<GameObject, bool> menuVisibilityBeforeCredits = new Dictionary<GameObject, bool>();
    private bool creditsFocusApplied;

    private void Awake()
    {
        EnsureSfxAudioSource();
        RegisterButtonAudio();
    }

    public void LoadScene(string sceneName)
    {
        SceneManager.LoadScene(sceneName);
    }

    public void QuitGame()
    {
        Application.Quit();
        #if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
        #endif
    }

    public void TogglePanel(GameObject panel)
    {
        if (panel == null)
        {
            return;
        }

        bool shouldOpen = !panel.activeSelf;
        panel.SetActive(shouldOpen);

        if (panel.name == "PanelCreditos")
        {
            SetCreditsPanelFocus(panel, shouldOpen);
        }
    }

    public void ShowIntroScreen()
    {
        if (introPanel != null)
        {
            introPanel.SetActive(true);
        }
    }

    public void SetVolume(float volume)
    {
        AudioListener.volume = volume;
    }

    public void ToggleFullscreen(bool isFullscreen)
    {
        Screen.fullScreen = isFullscreen;
    }

    public void PlayHoverSound()
    {
        PlayClip(hoverClip, hoverVolume);
    }

    public void PlayClickSound()
    {
        PlayClip(clickClip, clickVolume);
    }

    private void EnsureSfxAudioSource()
    {
        if (uiSfxAudioSource != null)
        {
            return;
        }

        AudioSource[] audioSources = GetComponents<AudioSource>();
        foreach (AudioSource audioSource in audioSources)
        {
            if (audioSource != null && !audioSource.playOnAwake && !audioSource.loop)
            {
                uiSfxAudioSource = audioSource;
                break;
            }
        }

        if (uiSfxAudioSource == null)
        {
            uiSfxAudioSource = gameObject.AddComponent<AudioSource>();
        }

        uiSfxAudioSource.playOnAwake = false;
        uiSfxAudioSource.loop = false;
    }

    private void RegisterButtonAudio()
    {
        Button[] buttons = FindObjectsOfType<Button>(true);

        foreach (Button button in buttons)
        {
            if (button == null)
            {
                continue;
            }

            button.onClick.AddListener(PlayClickSound);

            HomeButtonAudioRelay relay = button.GetComponent<HomeButtonAudioRelay>();
            if (relay == null)
            {
                relay = button.gameObject.AddComponent<HomeButtonAudioRelay>();
            }

            relay.Initialize(this);
        }
    }

    private void PlayClip(AudioClip clip, float volume)
    {
        if (uiSfxAudioSource == null || clip == null)
        {
            return;
        }

        uiSfxAudioSource.PlayOneShot(clip, volume);
    }

    private void SetCreditsPanelFocus(GameObject creditsPanel, bool isOpen)
    {
        Transform canvasRoot = creditsPanel.transform.parent;
        if (canvasRoot == null)
        {
            return;
        }

        if (isOpen)
        {
            menuVisibilityBeforeCredits.Clear();

            foreach (Transform child in canvasRoot)
            {
                GameObject childObject = child.gameObject;

                if (childObject == creditsPanel)
                {
                    continue;
                }

                if (childObject.name == "Background")
                {
                    childObject.SetActive(true);
                    continue;
                }

                menuVisibilityBeforeCredits[childObject] = childObject.activeSelf;
                childObject.SetActive(false);
            }

            creditsFocusApplied = true;
            return;
        }

        if (!creditsFocusApplied)
        {
            return;
        }

        foreach (KeyValuePair<GameObject, bool> entry in menuVisibilityBeforeCredits)
        {
            if (entry.Key != null)
            {
                entry.Key.SetActive(entry.Value);
            }
        }

        menuVisibilityBeforeCredits.Clear();
        creditsFocusApplied = false;
    }
}

public class HomeButtonAudioRelay : MonoBehaviour, IPointerEnterHandler
{
    private UIButtons uiButtons;

    public void Initialize(UIButtons manager)
    {
        uiButtons = manager;
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (!isActiveAndEnabled || uiButtons == null)
        {
            return;
        }

        uiButtons.PlayHoverSound();
    }
}

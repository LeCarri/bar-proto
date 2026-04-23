using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class UIButtons : MonoBehaviour
{
    [Header("UI Audio")]
    [SerializeField] private AudioSource uiAudioSource;
    [SerializeField] private AudioClip hoverClip;
    [SerializeField] private AudioClip clickClip;
    [SerializeField] [Range(0f, 1f)] private float hoverVolume = 0.6f;
    [SerializeField] [Range(0f, 1f)] private float clickVolume = 0.8f;

    private void Awake()
    {
        EnsureAudioSource();
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
        panel.SetActive(!panel.activeSelf);
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

    private void EnsureAudioSource()
    {
        if (uiAudioSource != null)
        {
            return;
        }

        uiAudioSource = GetComponent<AudioSource>();

        if (uiAudioSource == null)
        {
            uiAudioSource = gameObject.AddComponent<AudioSource>();
        }

        uiAudioSource.playOnAwake = false;
        uiAudioSource.loop = false;
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
        if (uiAudioSource == null || clip == null)
        {
            return;
        }

        uiAudioSource.PlayOneShot(clip, volume);
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

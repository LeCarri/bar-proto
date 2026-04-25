using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;
using UnityEngine.UI;

public class IntroScreen : MonoBehaviour
{
    [Header("UI Elements")]
    [SerializeField] private Image blackPanel;
    [SerializeField] private TextMeshProUGUI introText;

    [Header("Animation Settings")]
    [SerializeField] private string fullText = "Texto de introducción aquí."; // El texto completo
    [SerializeField] private float typewriterSpeed = 0.05f; // Velocidad media

    [Header("Audio Settings")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioSource backgroundMusicSource;
    [SerializeField] private AudioClip typingSound; // Sonido durante la animación
    [SerializeField] [Range(0f, 1f)] private float typingVolume = 0.25f;

    [Header("Transition")]
    [SerializeField] private string nextSceneName = "Night_1 Scene";

    private void Start()
    {
        if (blackPanel != null)
        {
            blackPanel.color = Color.black;
        }

        if (audioSource == null)
        {
            audioSource = GetComponent<AudioSource>();
        }

        if (audioSource == null && typingSound != null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
            audioSource.playOnAwake = false;
            audioSource.loop = false;
        }

        if (backgroundMusicSource != null && backgroundMusicSource.isPlaying)
        {
            backgroundMusicSource.Pause();
        }

        if (introText != null)
        {
            introText.text = "";
            StartCoroutine(TypewriterEffect());
        }
    }

    private IEnumerator TypewriterEffect()
    {
        if (audioSource != null && typingSound != null)
        {
            audioSource.PlayOneShot(typingSound, typingVolume);
        }

        foreach (char letter in fullText.ToCharArray())
        {
            introText.text += letter;
            yield return new WaitForSeconds(typewriterSpeed);
        }

        // Esperar 2 segundos después de la animación
        yield return new WaitForSeconds(2f);

        // Cargar la siguiente escena
        SceneManager.LoadScene(nextSceneName);
    }
}

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

    [Header("Firma del autor")]
    [SerializeField] private TextMeshProUGUI signatureText;
    [SerializeField] private string firma = "— Sigmund Freud";

    [Header("Animation Settings")]
    [TextArea(3, 6)]
    [SerializeField] private string fullText =
        "Las emociones no expresadas nunca mueren.\n" +
        "Son enterradas vivas y regresan de formas perturbadoras.";

    [SerializeField] private float typewriterSpeed = 0.05f;
    [SerializeField] private float tiempoLecturaFinal = 2f;

    [Header("Audio Settings")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioSource backgroundMusicSource;
    [SerializeField] private AudioClip typingSound;

    [Range(0f, 1f)]
    [SerializeField] private float typingVolume = 0.25f;

    [Header("Transition")]
    [SerializeField] private string nextSceneName = "Exterior";

    private void Awake()
    {
        // Utilizar una fuente exclusiva para la escritura.
        if (audioSource == null ||
            audioSource == backgroundMusicSource)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }

        audioSource.playOnAwake = false;
        audioSource.loop = false;
        audioSource.spatialBlend = 0f;
    }

    private void Start()
    {
        // Mantener fondo negro.
        if (blackPanel != null)
            blackPanel.color = Color.black;

        // Evitar sonidos previos.
        if (audioSource != null)
            audioSource.Stop();

        // Pausar música de fondo.
        if (backgroundMusicSource != null &&
            backgroundMusicSource.isPlaying)
        {
            backgroundMusicSource.Pause();
        }

        // Preparar firma, invisible al principio.
        if (signatureText != null)
        {
            signatureText.text = firma;
            signatureText.alpha = 0f;
        }

        if (introText != null)
        {
            StartCoroutine(TypewriterEffect());
        }
        else
        {
            Debug.LogError(
                "[IntroScreen] Falta asignar Intro Text."
            );
        }
    }

    private IEnumerator TypewriterEffect()
    {
        // Cargar el texto completo desde el principio.
        // Así TextMeshPro calcula su distribución una sola vez.
        introText.text = fullText;

        // Ocultar todos los caracteres.
        introText.maxVisibleCharacters = 0;

        // Actualizar información de caracteres.
        introText.ForceMeshUpdate();

        int cantidadCaracteres =
            introText.textInfo.characterCount;

        // Mostrar uno por uno.
        for (int i = 0; i < cantidadCaracteres; i++)
        {
            char caracter =
                introText.textInfo.characterInfo[i].character;

            // Revelar el siguiente carácter.
            introText.maxVisibleCharacters = i + 1;

            // Los espacios y saltos de línea
            // se muestran inmediatamente.
            if (char.IsWhiteSpace(caracter))
                continue;

            // Sonido por cada carácter visible.
            if (audioSource != null && typingSound != null)
            {
                // Cortar el clic anterior para
                // evitar acumulaciones.
                audioSource.Stop();

                audioSource.PlayOneShot(
                    typingSound,
                    typingVolume
                );
            }

            // Esperar solamente por caracteres visibles.
            yield return new WaitForSecondsRealtime(
                Mathf.Max(0f, typewriterSpeed)
            );
        }

        // Asegurar que todo el texto esté visible.
        introText.maxVisibleCharacters = int.MaxValue;

        // Detener último clic si sigue sonando.
        if (audioSource != null)
            audioSource.Stop();

        // Mostrar firma instantáneamente,
        // sin mover ni reacomodar el texto principal.
        if (signatureText != null)
        {
            signatureText.alpha = 1f;
        }

        // Tiempo de lectura.
        yield return new WaitForSecondsRealtime(
            tiempoLecturaFinal
        );

        // Conservar el sistema original de guardado.
        if (SaveManager.Instance != null)
        {
            SaveManager.Instance.LoadSave();
        }
        else
        {
            SceneManager.LoadScene(nextSceneName);
        }
    }
}

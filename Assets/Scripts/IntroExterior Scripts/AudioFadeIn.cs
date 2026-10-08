using UnityEngine;
using System.Collections;

public class AudioFadeIn : MonoBehaviour
{
    public AudioSource audioSource;

    [Header("Fade In")]
    public float fadeDuration;
    public float delay;

    [Header("Fade Out")]
    public float fadeOutDuration;

    private float originalVolume;


    void Start()
    {
        originalVolume = audioSource.volume;

        audioSource.volume = 0f;
        audioSource.Play();

        StartCoroutine(FadeIn());
    }

    IEnumerator FadeIn()
    {
        yield return new WaitForSeconds(delay);

        float timer = 0f;

        while (timer < fadeDuration)
        {
            timer += Time.deltaTime;
            audioSource.volume = Mathf.Lerp(0f, originalVolume, timer / fadeDuration);
            yield return null;
        }

        audioSource.volume = originalVolume;
    }

    public void FadeOut()
    {
        StartCoroutine(FadeOutCoroutine());
    }

    IEnumerator FadeOutCoroutine()
    {
        float startVolume = audioSource.volume;
        float timer = 0f;

        while (timer < fadeOutDuration)
        {
            timer += Time.deltaTime;
            audioSource.volume = Mathf.Lerp(startVolume, 0f, timer / fadeOutDuration);
            yield return null;
        }

        audioSource.volume = 0f;
    }
}
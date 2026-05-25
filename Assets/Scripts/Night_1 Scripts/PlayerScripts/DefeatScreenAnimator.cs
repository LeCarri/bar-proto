using System.Collections;
using UnityEngine;

public class DefeatScreenAnimator : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private GameObject defeatCanvas;
    [SerializeField] private CanvasGroup backgroundGroup;
    [SerializeField] private CanvasGroup defeatTitleGroup;
    [SerializeField] private RectTransform defeatTitleTransform;
    [SerializeField] private CanvasGroup mainMenuButtonGroup;
    [SerializeField] private RectTransform mainMenuButtonTransform;
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip defeatImpactSound;

    [Header("Timing")]
    [SerializeField] private float backgroundFadeDuration = 0.35f;
    [SerializeField] private float titleDelay = 0.18f;
    [SerializeField] private float titleImpactDuration = 0.28f;
    [SerializeField] private float buttonDelay = 0.25f;
    [SerializeField] private float buttonFadeDuration = 0.35f;
    [SerializeField] private float buttonMoveDistance = 24f;

    [Header("Impact")]
    [SerializeField] private float titleStartScale = 1.45f;
    [SerializeField] private float titleOvershootScale = 0.92f;

    private Coroutine animationRoutine;
    private Vector3 titleOriginalScale;
    private Vector2 buttonOriginalPosition;

    private void Awake()
    {
        if (defeatTitleTransform != null)
        {
            titleOriginalScale = defeatTitleTransform.localScale;
        }

        if (mainMenuButtonTransform != null)
        {
            buttonOriginalPosition = mainMenuButtonTransform.anchoredPosition;
        }

        SetInitialState();
    }

    private void OnEnable()
    {
        StartAnimation();
    }

    private void OnDisable()
    {
        if (animationRoutine != null)
        {
            StopCoroutine(animationRoutine);
            animationRoutine = null;
        }
    }

    private void StartAnimation()
    {
        if (defeatCanvas != null && !defeatCanvas.activeSelf)
        {
            defeatCanvas.SetActive(true);
        }

        Time.timeScale = 0f;
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;

        if (animationRoutine != null)
        {
            StopCoroutine(animationRoutine);
        }

        animationRoutine = StartCoroutine(AnimateDefeatScreen());
    }

    private IEnumerator AnimateDefeatScreen()
    {
        SetInitialState();

        yield return FadeCanvasGroup(backgroundGroup, 0f, 1f, backgroundFadeDuration);
        yield return new WaitForSecondsRealtime(titleDelay);

        PlayImpactSound();
        yield return AnimateTitleImpact();

        yield return new WaitForSecondsRealtime(buttonDelay);
        yield return AnimateButton();

        animationRoutine = null;
    }

    private void SetInitialState()
    {
        SetGroup(backgroundGroup, 0f, true);
        SetGroup(defeatTitleGroup, 0f, false);
        SetGroup(mainMenuButtonGroup, 0f, false);

        if (defeatTitleTransform != null)
        {
            Vector3 baseScale = titleOriginalScale == Vector3.zero ? Vector3.one : titleOriginalScale;
            defeatTitleTransform.localScale = baseScale * titleStartScale;
        }

        if (mainMenuButtonTransform != null)
        {
            Vector2 basePosition = buttonOriginalPosition;
            mainMenuButtonTransform.anchoredPosition = basePosition - new Vector2(0f, buttonMoveDistance);
        }
    }

    private IEnumerator AnimateTitleImpact()
    {
        if (defeatTitleGroup != null)
        {
            defeatTitleGroup.alpha = 1f;
            defeatTitleGroup.blocksRaycasts = false;
            defeatTitleGroup.interactable = false;
        }

        if (defeatTitleTransform == null)
        {
            yield break;
        }

        Vector3 baseScale = titleOriginalScale == Vector3.zero ? Vector3.one : titleOriginalScale;
        float halfDuration = titleImpactDuration * 0.55f;

        yield return ScaleTransform(defeatTitleTransform, baseScale * titleStartScale, baseScale * titleOvershootScale, halfDuration);
        yield return ScaleTransform(defeatTitleTransform, baseScale * titleOvershootScale, baseScale, titleImpactDuration - halfDuration);
    }

    private IEnumerator AnimateButton()
    {
        if (mainMenuButtonGroup != null)
        {
            mainMenuButtonGroup.blocksRaycasts = false;
            mainMenuButtonGroup.interactable = false;
        }

        Vector2 startPosition = buttonOriginalPosition - new Vector2(0f, buttonMoveDistance);
        float elapsed = 0f;

        while (elapsed < buttonFadeDuration)
        {
            elapsed += Time.unscaledDeltaTime;
            float t = Mathf.Clamp01(elapsed / buttonFadeDuration);
            float easedT = Smooth(t);

            if (mainMenuButtonGroup != null)
            {
                mainMenuButtonGroup.alpha = easedT;
            }

            if (mainMenuButtonTransform != null)
            {
                mainMenuButtonTransform.anchoredPosition = Vector2.LerpUnclamped(startPosition, buttonOriginalPosition, easedT);
            }

            yield return null;
        }

        if (mainMenuButtonGroup != null)
        {
            mainMenuButtonGroup.alpha = 1f;
            mainMenuButtonGroup.blocksRaycasts = true;
            mainMenuButtonGroup.interactable = true;
        }
    }

    private IEnumerator FadeCanvasGroup(CanvasGroup group, float from, float to, float duration)
    {
        if (group == null)
        {
            yield break;
        }

        float elapsed = 0f;
        group.alpha = from;

        while (elapsed < duration)
        {
            elapsed += Time.unscaledDeltaTime;
            float t = Mathf.Clamp01(elapsed / duration);
            group.alpha = Mathf.Lerp(from, to, Smooth(t));
            yield return null;
        }

        group.alpha = to;
    }

    private IEnumerator ScaleTransform(RectTransform target, Vector3 from, Vector3 to, float duration)
    {
        if (target == null)
        {
            yield break;
        }

        float elapsed = 0f;
        target.localScale = from;

        while (elapsed < duration)
        {
            elapsed += Time.unscaledDeltaTime;
            float t = Mathf.Clamp01(elapsed / duration);
            target.localScale = Vector3.LerpUnclamped(from, to, Smooth(t));
            yield return null;
        }

        target.localScale = to;
    }

    private void PlayImpactSound()
    {
        if (audioSource != null && defeatImpactSound != null)
        {
            audioSource.PlayOneShot(defeatImpactSound);
        }
    }

    private void SetGroup(CanvasGroup group, float alpha, bool blocksRaycasts)
    {
        if (group == null)
        {
            return;
        }

        group.alpha = alpha;
        group.blocksRaycasts = blocksRaycasts;
        group.interactable = blocksRaycasts;
    }

    private float Smooth(float t)
    {
        return t * t * (3f - 2f * t);
    }
}

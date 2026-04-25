using System.Collections;
using TMPro;
using UnityEngine;

[RequireComponent(typeof(TMP_Text))]
public class CreditsSweepAnimation : MonoBehaviour
{
    [SerializeField] private float entryDelay = 0.15f;
    [SerializeField] private float duration = 0.85f;
    [SerializeField] private float sweepWidth = 120f;
    [SerializeField] private Vector2 startOffset = new Vector2(-90f, 0f);

    private TMP_Text creditsText;
    private RectTransform rectTransform;
    private Coroutine animationRoutine;
    private Vector2 originalAnchoredPosition;
    private bool hasOriginalPosition;

    private void Awake()
    {
        CacheComponents();
    }

    private void OnEnable()
    {
        CacheComponents();
        StoreOriginalPosition();
        Play();
    }

    private void OnDisable()
    {
        if (animationRoutine != null)
        {
            StopCoroutine(animationRoutine);
            animationRoutine = null;
        }

        if (hasOriginalPosition && rectTransform != null)
        {
            rectTransform.anchoredPosition = originalAnchoredPosition;
        }

        SetSweepProgress(1f);
    }

    public void Play()
    {
        if (!isActiveAndEnabled || creditsText == null)
        {
            return;
        }

        if (animationRoutine != null)
        {
            StopCoroutine(animationRoutine);
        }

        animationRoutine = StartCoroutine(AnimateSweep());
    }

    private void CacheComponents()
    {
        if (creditsText == null)
        {
            creditsText = GetComponent<TMP_Text>();
        }

        if (rectTransform == null)
        {
            rectTransform = GetComponent<RectTransform>();
        }
    }

    private void StoreOriginalPosition()
    {
        if (hasOriginalPosition || rectTransform == null)
        {
            return;
        }

        originalAnchoredPosition = rectTransform.anchoredPosition;
        hasOriginalPosition = true;
    }

    private IEnumerator AnimateSweep()
    {
        SetSweepProgress(0f);

        if (entryDelay > 0f)
        {
            yield return new WaitForSeconds(entryDelay);
        }

        float elapsed = 0f;
        float animationDuration = Mathf.Max(0.01f, duration);

        while (elapsed < animationDuration)
        {
            elapsed += Time.deltaTime;
            SetSweepProgress(Mathf.Clamp01(elapsed / animationDuration));
            yield return null;
        }

        SetSweepProgress(1f);
        animationRoutine = null;
    }

    private void SetSweepProgress(float progress)
    {
        if (creditsText == null)
        {
            return;
        }

        float easedProgress = SmoothStep(progress);

        if (hasOriginalPosition && rectTransform != null)
        {
            rectTransform.anchoredPosition = Vector2.Lerp(
                originalAnchoredPosition + startOffset,
                originalAnchoredPosition,
                easedProgress);
        }

        creditsText.ForceMeshUpdate();
        TMP_TextInfo textInfo = creditsText.textInfo;

        if (textInfo == null || textInfo.characterCount == 0)
        {
            return;
        }

        GetVisibleTextBounds(textInfo, out float minX, out float maxX);

        if (float.IsInfinity(minX) || float.IsInfinity(maxX))
        {
            return;
        }

        float revealWidth = Mathf.Max(1f, sweepWidth);
        float sweepPosition = Mathf.Lerp(minX - revealWidth, maxX + revealWidth, easedProgress);

        for (int i = 0; i < textInfo.characterCount; i++)
        {
            TMP_CharacterInfo characterInfo = textInfo.characterInfo[i];

            if (!characterInfo.isVisible)
            {
                continue;
            }

            float characterProgress = Mathf.Clamp01((sweepPosition - characterInfo.bottomLeft.x) / revealWidth);
            byte alpha = (byte)Mathf.RoundToInt(characterProgress * 255f);
            SetCharacterAlpha(textInfo, characterInfo, alpha);
        }

        creditsText.UpdateVertexData(TMP_VertexDataUpdateFlags.Colors32);
    }

    private void GetVisibleTextBounds(TMP_TextInfo textInfo, out float minX, out float maxX)
    {
        minX = float.PositiveInfinity;
        maxX = float.NegativeInfinity;

        for (int i = 0; i < textInfo.characterCount; i++)
        {
            TMP_CharacterInfo characterInfo = textInfo.characterInfo[i];

            if (!characterInfo.isVisible)
            {
                continue;
            }

            minX = Mathf.Min(minX, characterInfo.bottomLeft.x);
            maxX = Mathf.Max(maxX, characterInfo.topRight.x);
        }
    }

    private void SetCharacterAlpha(TMP_TextInfo textInfo, TMP_CharacterInfo characterInfo, byte alpha)
    {
        Color32[] vertexColors = textInfo.meshInfo[characterInfo.materialReferenceIndex].colors32;
        int vertexIndex = characterInfo.vertexIndex;

        vertexColors[vertexIndex].a = alpha;
        vertexColors[vertexIndex + 1].a = alpha;
        vertexColors[vertexIndex + 2].a = alpha;
        vertexColors[vertexIndex + 3].a = alpha;
    }

    private float SmoothStep(float progress)
    {
        progress = Mathf.Clamp01(progress);
        return progress * progress * (3f - 2f * progress);
    }
}

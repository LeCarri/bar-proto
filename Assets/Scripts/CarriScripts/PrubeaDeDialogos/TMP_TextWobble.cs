using UnityEngine;
using TMPro;

[RequireComponent(typeof(TMP_Text))]
public class TMP_TextWobble : MonoBehaviour
{
    private TMP_Text textComponent;

    [Header("Ajustes de Temblor / Enojo")]
    public float shakeSpeed = 25f;       // Frecuencia del temblor
    public float shakeAmount = 3.0f;     // Intensidad de la vibración
    public bool applyToAllText = false;  // Si es true, hace temblar todo el texto sin depender de tags

    [Header("Tag Personalizado")]
    public string shakeTag = "shake";    // Tag para activar el efecto: <shake>texto</shake>

    void Awake()
    {
        textComponent = GetComponent<TMP_Text>();
    }

    void Update()
    {
        textComponent.ForceMeshUpdate();
        TMP_TextInfo textInfo = textComponent.textInfo;

        // Si no hay texto o no hay caracteres, cortamos la ejecución
        if (textInfo == null || textInfo.characterCount == 0) return;

        bool hasShakeTag = textComponent.text.Contains($"<{shakeTag}>");

        for (int i = 0; i < textInfo.characterCount; i++)
        {
            TMP_CharacterInfo charInfo = textInfo.characterInfo[i];

            if (!charInfo.isVisible) continue;

            // Determinar si el carácter actual debe temblar
            bool shouldShake = applyToAllText || (hasShakeTag && IsCharacterInsideTag(i));

            if (shouldShake)
            {
                int materialIndex = charInfo.materialReferenceIndex;
                int vertexIndex = charInfo.vertexIndex;

                Vector3[] vertices = textInfo.meshInfo[materialIndex].vertices;

                // Variación PerlinNoise basada en tiempo e índice del carácter
                float xOffset = (Mathf.PerlinNoise(Time.time * shakeSpeed, i * 1.5f) - 0.5f) * shakeAmount;
                float yOffset = (Mathf.PerlinNoise(i * 1.5f, Time.time * shakeSpeed) - 0.5f) * shakeAmount;
                Vector3 offset = new Vector3(xOffset, yOffset, 0f);

                // Aplicar offset a los 4 vértices del carácter
                textInfo.meshInfo[materialIndex].vertices[vertexIndex + 0] += offset;
                textInfo.meshInfo[materialIndex].vertices[vertexIndex + 1] += offset;
                textInfo.meshInfo[materialIndex].vertices[vertexIndex + 2] += offset;
                textInfo.meshInfo[materialIndex].vertices[vertexIndex + 3] += offset;
            }
        }

        // Actualizar mallas
        int totalMaterialsUsed = textInfo.materialCount;
        for (int i = 0; i < totalMaterialsUsed; i++)
        {
            if (textInfo.meshInfo[i].mesh != null)
            {
                textInfo.meshInfo[i].mesh.vertices = textInfo.meshInfo[i].vertices;
                textComponent.UpdateGeometry(textInfo.meshInfo[i].mesh, i);
            }
        }
    }

    private bool IsCharacterInsideTag(int charIndex)
    {
        // Obtener el texto plano procesado por TMP y comparar índices si hay tags
        string parsedText = textComponent.GetParsedText();
        string rawText = textComponent.text;

        int openTagIndex = rawText.IndexOf($"<{shakeTag}>");
        int closeTagIndex = rawText.IndexOf($"</{shakeTag}>");

        if (openTagIndex == -1 || closeTagIndex == -1) return false;

        // Ajustar índices descontando los caracteres del tag de apertura
        int cleanStartIndex = openTagIndex;
        int cleanEndIndex = closeTagIndex - ($"<{shakeTag}>").Length;

        return charIndex >= cleanStartIndex && charIndex < cleanEndIndex;
    }
}
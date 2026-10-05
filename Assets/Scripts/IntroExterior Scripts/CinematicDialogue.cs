using System.Collections;
using TMPro;
using UnityEngine;

public class CinematicDialogue : MonoBehaviour
{
    public TextMeshProUGUI dialogueText;
    public AudioSource typingAudio;

    [Header("Tipeo de la Máquina")]
    public float typingSpeed = 0.05f;

    [Header("Diálogo")]
    public float displayTime = 2f;
    public float fadeOutTime = 0.5f;

    int soundCounter = 0;

    void Start()
    {
        dialogueText.text = "";
        SetAlpha(0f);
    }


    public void ShowDialogue(string text)
    {
        StopAllCoroutines();
        StartCoroutine(TypeText(text));
    }

    public void Dialogue01()
    {
        ShowDialogue("Llevo tres semanas durmiendo dos horas por día...");
    }

    public void Dialogue02()
    {
        ShowDialogue("Las facturas no dejan de llegar...... y el banco no espera.");
    }

    public void Dialogue03()
    {
        ShowDialogue("Odio este trabajo.");
    }

    public void Dialogue04()
    {
        ShowDialogue("Odio este lugar.");
    }

    public void Dialogue05()
    {
        ShowDialogue("Perderlo significaría quedarme en la calle... Pero...");
    }

    public void Dialogue06()
    {
        ShowDialogue("Solo tengo que aguantar...");
    }

    public void Dialogue07()
    {
        ShowDialogue("...UN ÚLTIMO TURNO.");
    }


    IEnumerator TypeText(string text)
    {
        dialogueText.text = "";
        SetAlpha(1f);

        typingAudio.Play();

        foreach (char letter in text)
        {
            dialogueText.text += letter;
            yield return new WaitForSeconds(typingSpeed);
        }

        typingAudio.Stop();

        //LA FRASE SE MUESTRA DURANTE x SEGUNDOS
        yield return new WaitForSeconds(displayTime);

        //LUEGO DESAPARECE
        float timer = 0f;

        while (timer < fadeOutTime)
        {
            timer += Time.deltaTime;

            float alpha = Mathf.Lerp(1f, 0f, timer / fadeOutTime);
            SetAlpha(alpha);
        
            yield return null;
        }

        SetAlpha(0f);
        dialogueText.text = "";
    }

    void SetAlpha(float alpha)
    {
        Color color = dialogueText.color;
        color.a = alpha;
        dialogueText.color = color;
    }


    










}

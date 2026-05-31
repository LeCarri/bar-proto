using System.Collections;
using TMPro;
using UnityEngine;

public class ClienteDialogueSystem : MonoBehaviour
{
    public static ClienteDialogueSystem Instance;

    public GameObject dialoguePanel;

    public TextMeshProUGUI nombreText;
    public TextMeshProUGUI dialogoText;

    Coroutine currentDialogue;

    void Awake()
    {
        Instance = this;
    }

    public void MostrarDialogo(string nombre, string mensaje)
    {
        if (currentDialogue != null)
            StopCoroutine(currentDialogue);

        dialoguePanel.SetActive(true);

        nombreText.text = nombre;
        dialogoText.text = mensaje;

        currentDialogue = StartCoroutine(CerrarDialogo());
    }

    IEnumerator CerrarDialogo()
    {
        yield return new WaitForSeconds(4f);

        dialoguePanel.SetActive(false);
    }
}

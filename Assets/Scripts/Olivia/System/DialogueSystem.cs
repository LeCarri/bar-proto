using System.Collections.Generic;
using System.Collections;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UIElements;
using Unity.Cinemachine;
using Unity.VisualScripting;

public class DialogueSystem : MonoBehaviour
{
    public static DialogueSystem Instance { get; private set; }

    [SerializeField]
    private GameObject dialoguePanel;

    [SerializeField]
    private TextMeshProUGUI nameTextfield;

    [SerializeField]
    private TextMeshProUGUI dialogueTextfield;

    [SerializeField]
    private List<string> dialogueIds = new List<string>();

    [SerializeField]
    [TextArea]
    private List<string> dialogueTexts = new List<string>();

    private Dictionary<string, string> dialogueDictionary = new Dictionary<string, string>();

    [SerializeField]
    private List<string> names = new List<string>();

    private InputAction interactDialogue;
    public bool inDialogue = false;

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(Instance);
    }

    void Start()
    {
        interactDialogue = InputSystem.actions.FindAction("PickupRHand");

        fillDictionary();
    }

    void Update()
    {
        
    }

    public void AddDialogue(string newId, string newText) 
    {
        dialogueIds.Add(newId);
        dialogueTexts.Add(newText);

        dialogueDictionary.Add(newId, newText);
    }

    void fillDictionary() 
    {
        dialogueDictionary.Clear();
        for (int i = 0; i < dialogueIds.Count; i++) 
        {
            dialogueDictionary.Add(dialogueIds[i], dialogueTexts[i]);
            //Debug.Log("Added text: " + dialogueDictionary[dialogueIds[i]]);
        }

        //foreach (KeyValuePair<string, string> entry in dialogueDictionary) 
        //{
        //    //Debug.Log("Key: " + entry.Key + "\n" + "Value: " + entry.Value);
        //}
        //Displaytext(dialogueIds[Random.Range(0, dialogueIds.Count)], "Test");
    }

    public void Displaytext(string id, int displayName)
    {
        if (dialogueIds.Contains(id))
        {
            inDialogue = true;
            dialoguePanel.SetActive(true); //Mostrar el panel.

            nameTextfield.text = names[displayName]; //Cambiar el nombre.
            StartCoroutine(WriteText(dialogueDictionary[id], 0.05f, 0.02f)); //Escribir diálogo.
        }
    }

    public void Displaytext(List<string> id_chain, List<int> namesOrder)
    {
        bool idsAreValid = true;

        foreach (string id in id_chain) 
        {
            if (dialogueIds.Contains(id) == false)
                idsAreValid = false;
        }

        if (idsAreValid == false) //Si alguna de las IDs no existe, cancela el display.
            return;
        else
        {
            inDialogue = true;
            dialoguePanel.SetActive(true); //Mostrar el panel.

            StartCoroutine(WriteChainText(id_chain, namesOrder, 0.05f, 0.02f)); //Escribir diálogos.
        }
    }

    private IEnumerator WriteText(string text, float minDelay, float maxDelay) 
    {
        string displayText = "";
        char[] textCharArray = text.ToCharArray();

        for (int i = 0; i < textCharArray.Length; i++) 
        {
            displayText += textCharArray[i];
            dialogueTextfield.text = displayText;
            //Debug.Log(displayText);
            yield return new WaitForSeconds(Random.Range(minDelay, maxDelay));
        }

        while (inDialogue) 
        {
            if (interactDialogue.WasCompletedThisFrame()) 
            {
                dialoguePanel.SetActive (false);
                inDialogue = false;
            }

            yield return new WaitForSeconds(Time.deltaTime);
        }
        
        yield break;
    }

    private IEnumerator WriteChainText(List<string> id_chain, List<int> namesIndexes,
                                                            float minDelay, float maxDelay)
    {
        foreach (string id in id_chain) 
        {
            //Debug.Log(id_chain.IndexOf(id));

            int IndexOfId = id_chain.IndexOf(id); //Buscar el index del texto actual en la lista de IDs pasada.

            string displayText = "";

            if (names[namesIndexes[IndexOfId]] != null ||
                names.Contains(names[namesIndexes[IndexOfId]]))
                nameTextfield.text = names[namesIndexes[IndexOfId]];
            else
                nameTextfield.text = "null"; //Si el nombre que trata de mostrar no es válido, muestra null.

            string text = dialogueDictionary[id_chain[IndexOfId]]; //Setear el texto de la ID actual a la variable.

            char[] textCharArray = text.ToCharArray();
            
            for (int i = 0; i < textCharArray.Length; i++) //Escribir el texto.
            { 
                displayText += textCharArray[i];
                dialogueTextfield.text = displayText;
                //Debug.Log(displayText);
                yield return new WaitForSeconds(Random.Range(minDelay, maxDelay));
            }

            //Esperar el input del jugador.
            while (inDialogue)
            {
                if (interactDialogue.WasCompletedThisFrame())
                {
                    break;
                }
            
                yield return new WaitForSeconds(Time.deltaTime);
            }

            //Repetir
        }

        inDialogue = false;
        dialoguePanel.SetActive(false);

        yield break;
    }
}



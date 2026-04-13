using System.Collections.Generic;
using System.Collections;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UIElements;
using Unity.Cinemachine;

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
        foreach (KeyValuePair<string, string> entry in dialogueDictionary) 
        {
            //Debug.Log("Key: " + entry.Key + "\n" + "Value: " + entry.Value);
        }
        //Displaytext(dialogueIds[Random.Range(0, dialogueIds.Count)], "Test");
    }

    public void Displaytext(string id, string displayName)
    {
        if (dialogueIds.Contains(id))
        {
            inDialogue = true;
            dialoguePanel.SetActive(true);

            nameTextfield.text = displayName;
            StartCoroutine(WriteText(dialogueDictionary[id], 0.05f, 0.02f));
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
}

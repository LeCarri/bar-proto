using System.Collections.Generic;
using UnityEngine;

public class DialogueFlag : FlagListener
{
    [SerializeField]
    private List<int> dialogueDisplayNamesIndexes = new List<int>();

    [SerializeField]
    private List<string> dialogueIds = new List<string>();

    [SerializeField]
    bool isDialogueChain = false;

    public void DisplayText(string flag, bool flagsValue)
    {
        if (flag == flagName && flagsValue == changeFlagTo)
        {
            if (!isDialogueChain)
                DialogueSystem.Instance.Displaytext(dialogueIds[0], dialogueDisplayNamesIndexes[0]); //Escribe la primer entrada
            else
                DialogueSystem.Instance.Displaytext(dialogueIds, dialogueDisplayNamesIndexes); //Escribe todas las entradas
        }
    }

}

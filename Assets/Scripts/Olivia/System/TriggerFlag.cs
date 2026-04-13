using System;
using System.Collections.Generic;
using UnityEngine;

public class TriggerFlag : FlagListener
{

    [SerializeField]
    private GameObject exampleObject; //Objeto de ejemplo

    [SerializeField]
    private string dialogueDisplayName;

    [SerializeField]
    private string dialogueId;

    [SerializeField]
    bool deactivateOnTouch = true;

    override protected void OnStart() 
    {
        if (exampleObject.activeSelf == true) //Si el objeto está activo al iniciar la escena, se desactiva.
            exampleObject.SetActive(false);
        
        if (exampleObject != null) //Si tiene un objeto asignado en la variable, se suscribe al evento de onFlagsChange.
            flagsSystem.onFlagsChange += EnableObject;

        if (dialogueId != null || dialogueId != "")
            flagsSystem.onFlagsChange += PlayText;
    }

    private void EnableObject(string flag, bool flagsValue) 
    {
        if (flag == flagName && flagsValue == changeFlagTo) 
        {
            exampleObject.SetActive(true); //Activa el objeto si sus flags y valores coinciden.
        }
    }

    private void PlayText(string flag, bool flagsValue) 
    {
        if (flag == flagName && flagsValue == changeFlagTo)
        {
            DialogueSystem.Instance.Displaytext(dialogueId, dialogueDisplayName);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        flagsSystem.ChangeFlag(flagName, changeFlagTo); //Cambia la flag al hacer contacto con el trigger.

        if (deactivateOnTouch)
            gameObject.SetActive(false); //Desactiva el trigger al activarlo si se desea.
    }



}

using System;
using System.Collections.Generic;
using UnityEngine;

public class TriggerFlag : FlagListener
{

    [SerializeField]
    private GameObject exampleObject; //Objeto de ejemplo

    [SerializeField]
    private DialogueFlag dialogueFlagScript;

    [SerializeField]
    bool deactivateOnTouch = true;

    override protected void OnStart() 
    {
        if (exampleObject.activeSelf == true) //Si el objeto está activo al iniciar la escena, se desactiva.
            exampleObject.SetActive(false);
        
        if (exampleObject != null) //Si tiene un objeto asignado en la variable, se suscribe al evento de onFlagsChange.
            flagsSystem.onFlagsChange += EnableObject;

        if (dialogueFlagScript != null)
            flagsSystem.onFlagsChange += StartDialogue;
    }

    private void EnableObject(string flag, bool flagsValue) 
    {
        if (flag == flagName && flagsValue == changeFlagTo) 
        {
            exampleObject.SetActive(true); //Activa el objeto si sus flags y valores coinciden.
        }
    }

    private void StartDialogue(string flag, bool flagsValue) 
    {
        dialogueFlagScript.DisplayText(flag, flagsValue);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("Player")) 
        {
            flagsSystem.ChangeFlag(flagName, changeFlagTo); //Cambia la flag al hacer contacto con el trigger.

            if (deactivateOnTouch)
                gameObject.SetActive(false); //Desactiva el trigger al activarlo si se desea.
        }
    }



}

using System;
using UnityEngine;

public class TriggerFlag : FlagListener
{

    [SerializeField]
    private GameObject exampleObject; //Objeto de ejemplo

    [SerializeField]
    bool deactivateOnTouch = true;

    override protected void OnStart() 
    {
        if (exampleObject.activeSelf == true) //Si el objeto está activo al iniciar la escena, se desactiva.
            exampleObject.SetActive(false);
        
        if (exampleObject != null) //Si tiene un objeto asignado en la variable, se suscribe al evento de onFlagsChange.
            flagsSystem.onFlagsChange += EnableObject;
    }

    private void EnableObject(string flag, bool flagsValue) 
    {
        if (flag == flagName && flagsValue == changeFlagTo) 
        {
            exampleObject.SetActive(true); //Activa el objeto si sus flags y valores coinciden.
        }
    }

  private void OnTriggerEnter(Collider other)
    {
        flagsSystem.ChangeFlag(flagName, changeFlagTo); //Cambia la flag al hacer contacto con el trigger.

        if (deactivateOnTouch)
            gameObject.SetActive(false); //Desactiva el trigger al activarlo si se desea.
    }

}

using System;
using System.Collections.Generic;
using UnityEngine;

public class FlagsSystem : MonoBehaviour
{
    public static FlagsSystem Instance { get; private set; }

    public Dictionary<string,bool> flags = new Dictionary<string, bool>();

    public bool readToConsole = false;

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(this);

        AddFlag("TestFlag", false);
    }

    private void Update()
    {
        if (readToConsole)
        {
            ReadFlagsList();
            readToConsole = false;
        }
    }

    public void AddFlag(string newFlag, bool startingValue = false) 
    {
        //añadir flag a la lista
        flags.Add(newFlag, startingValue);
        //Debug.Log(newFlag + " " + startingValue);
    }

    public event Action<string, bool> onFlagsChange;
    public void ChangeFlag(string flagName, bool value) 
    {
        //cambiar el valor de x flag
        flags[flagName] = value;

        if (onFlagsChange != null)
            onFlagsChange(flagName, value); //Activa el evento de onFlagsChange.
    }
    
    //Función para leer los valores del diccionario de flags a la consola.
    //                                (Los diccionarios son no serializables, por eso no se pueden ver en el inspector.)
    public void ReadFlagsList() 
    {
        foreach (KeyValuePair<string, bool> entry in flags)
        {
            Debug.Log("Key: " + entry.Key + ", Value: " + entry.Value);
        }

    }



}

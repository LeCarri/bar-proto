using UnityEngine;

public class FlagListener : MonoBehaviour
{
    protected FlagsSystem flagsSystem; //Instancia del FlagsSystem

    [SerializeField]
    protected string flagName; //Nombre de la flag que va a modificar/crear

    [SerializeField]
    protected bool changeFlagTo = true; //Valor al que va a setear a la flag correspondiente

    void Start()
    {
        flagsSystem = FlagsSystem.Instance;

        if (!flagsSystem.flags.ContainsKey(flagName)) //Si la flag que quiere cambiar no existe aún, se crea.
        {
            flagsSystem.AddFlag(flagName);
        }

        OnStart();
    }

    //Método llamado antes de finalizar el Start().
    //
    //Práctico en caso de necesitar añadir funciones al Start() en scripts heredados.
    virtual protected void OnStart() { }

}

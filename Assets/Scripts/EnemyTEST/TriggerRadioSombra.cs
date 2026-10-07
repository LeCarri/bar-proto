using UnityEngine;

public class TriggerRadioSombra : MonoBehaviour
{
    public enum TipoZona
    {
        Huida,
        Encuentro
    }


    [Header("Configuración")]
    public TipoZona tipoZona;

    public AparicionSombraCocina controlador;


    private void OnTriggerStay(
        Collider other)
    {
        if (!other.CompareTag("Player"))
            return;

        if (controlador == null)
            return;


        switch (tipoZona)
        {
            case TipoZona.Huida:

                controlador
                    .EntrarZonaHuida();

                break;


            case TipoZona.Encuentro:

                controlador
                    .EntrarZonaEncuentro();

                break;
        }
    }
}
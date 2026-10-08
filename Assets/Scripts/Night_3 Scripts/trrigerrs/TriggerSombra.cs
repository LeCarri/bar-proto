using UnityEngine;

public class TriggerAparicionSombraAct3 : MonoBehaviour
{
    [Header("Vigilante")]
    public GameObject Sombra;

    private bool activado = false;

    public GameObject siguienteTrigger;

    private void OnTriggerEnter(Collider other)
    {
        if (activado)
            return;

        if (!other.CompareTag("Player"))
            return;

        activado = true;

        Debug.Log(
            "[VIGILANTE ACT3] Lucas llegó al punto de aparición."
        );

        if (Sombra!= null)
        {
            Sombra.SetActive(true);

            Debug.Log(
                "[VIGILANTE ACT3] ¡Vigilante apareció!"
            );
        }
        else
        {
            Debug.LogWarning(
                "[VIGILANTE ACT3] No hay Vigilante asignado."
            );
        }

        siguienteTrigger.SetActive(true);

    }
}

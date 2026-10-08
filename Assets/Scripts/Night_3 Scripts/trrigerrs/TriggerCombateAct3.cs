using UnityEngine;

public class TriggerInicioCombateAct3 : MonoBehaviour
{
    [Header("Configuración")]
    public bool activarUnaSolaVez = true;

    private bool activado = false;

    public GameObject siguienteTrigger;

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player"))
            return;

        if (activarUnaSolaVez && activado)
            return;

        activado = true;

        Debug.Log("[COMBATE ACT3] ¡Lucas entró al trigger!");

        if (Act3Manager.Instance != null)
        {
            Act3Manager.Instance.IniciarCombateFinal();
        }
        else
        {
            Debug.LogError(
                "[COMBATE ACT3] No se encontró Act3Manager."
            );
        }

        siguienteTrigger.SetActive(true);
    }
}

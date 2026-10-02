
using UnityEngine;

public class TriggerSalidaBarraTEST : MonoBehaviour
{
    private bool yaSeDisparo = false;

    private void OnTriggerEnter(Collider other)
    {
        if (yaSeDisparo) return;

        if (other.CompareTag("Player"))
        {
            yaSeDisparo = true;

            if (EnemyTEST.Instance != null)
            {
                EnemyTEST.Instance.DispararSecuenciaNaniela();

                Debug.Log("[TEST] Aparición de Ñañiela activada.");
            }
            else
            {
                Debug.LogWarning("[TEST] No se encontró EnemyTEST.Instance.");
            }

            // Desactivamos el trigger para evitar que se repita.
            gameObject.SetActive(false);
        }
    }
}

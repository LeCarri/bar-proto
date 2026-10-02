using UnityEngine;

public class TriggerSalidaBarra : MonoBehaviour
{
    private bool yaSeDisparo = false;

    private void OnTriggerEnter(Collider other)
    {
        if (yaSeDisparo) return;

        if (other.CompareTag("Player"))
        {
            yaSeDisparo = true;

            if (Act1Manager.Instance != null)
            {
                Act1Manager.Instance.DispararSecuenciaNaniela();
            }

            // Desactivamos el trigger para que no vuelva a ejecutarse
            gameObject.SetActive(false);
        }
    }
}
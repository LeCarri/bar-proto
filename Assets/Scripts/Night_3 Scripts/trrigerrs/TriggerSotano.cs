using UnityEngine;
using UnityEngine.SceneManagement;

public class TriggerSotanoAct3 : MonoBehaviour
{
    private bool activado = false;

    private void OnTriggerEnter(Collider other)
    {
        if (activado)
            return;

        if (!other.CompareTag("Player"))
            return;

        activado = true;

        Debug.Log("[SOTANO ACT3] Lucas entró al sótano.");

        SceneManager.LoadScene("Basement (pasto)");
    }
}

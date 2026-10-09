using UnityEngine;
using UnityEngine.SceneManagement;

public class TriggerSotano : MonoBehaviour
{
    private bool activado = false;

    private void OnTriggerEnter(Collider other)
    {
        if (activado)
            return;

        if (!other.CompareTag("Player"))
            return;

        activado = true;

        Debug.Log("[SOTANO] Lucas entró al trigger.");

        SceneManager.LoadScene("Basement (pasto)");
    }
}


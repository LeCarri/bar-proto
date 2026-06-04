using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;
public class TriggerSotano : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            SceneManager.LoadScene("Basement (pasto)");

            Debug.Log("El jugador entró al trigger");
        }
    }
}

using UnityEngine;

public class TriggerMelodiaSotano : MonoBehaviour
{
    [SerializeField] private CajaMusicalSotano cajaMusical;

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player"))
            return;

        if (cajaMusical != null)
            cajaMusical.IniciarMelodia();
    }
}

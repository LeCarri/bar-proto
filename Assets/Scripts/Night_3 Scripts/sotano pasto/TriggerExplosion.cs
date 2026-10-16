using UnityEngine;

public class TriggerExplosionSotano : MonoBehaviour
{
    [SerializeField] private AudioSource sonidoExplosion;

    private bool activado = false;

    private void OnTriggerEnter(Collider other)
    {
        if (activado)
            return;

        if (!other.CompareTag("Player"))
            return;

        activado = true;

        if (sonidoExplosion != null)
            sonidoExplosion.Play();
    }
}

using UnityEngine;

public class TriggerAudioSotano : MonoBehaviour
{
    [SerializeField] private AudioSource audioSotano;
    private bool activado = false;

    private void OnTriggerEnter(Collider other)
    {
        if (activado)
            return;

        if (!other.CompareTag("Player"))
            return;

        activado = true;

        if (audioSotano != null)
            audioSotano.Play();
    }
}
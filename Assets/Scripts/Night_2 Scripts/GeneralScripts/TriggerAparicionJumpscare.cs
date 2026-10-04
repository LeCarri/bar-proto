using System.Collections;
using UnityEngine;

public class TriggerAparicionJumpscare : MonoBehaviour
{
    private bool disparado3 = false;

    [SerializeField]
    private VigenteMirror referenciaVigenteMirror; // Asignar en Inspector

    void Awake()
    {
        if (referenciaVigenteMirror == null)
        {
            referenciaVigenteMirror = Object.FindAnyObjectByType<VigenteMirror>();
            if (referenciaVigenteMirror == null)
            {
                Debug.Log("[TriggerAparicionJumpscare] referenciaVigenteMirror no asignada en inspector.");
            }
        }
    }

    void OnTriggerEnter(Collider other)
    {
        if (disparado3) return;
        if (!other.CompareTag("Player")) return;

        Act2ManagerDemo manager = Act2ManagerDemo.Instance;
        if (manager == null)
        {
            Debug.LogError("[TriggerAparicionJumpscare] Act2ManagerDemo no encontrado en la escena.");
            return;
        }

        if (!manager.TieneLlave())
        {
            Debug.Log("[TriggerAparicionJumpscare] El jugador pasó por el trigger pero no tiene la llave todavía.");
            return;
        }

        disparado3 = true;

        if (referenciaVigenteMirror == null)
        {
            referenciaVigenteMirror = Object.FindAnyObjectByType<VigenteMirror>();
            if (referenciaVigenteMirror == null)
            {
                Debug.LogWarning("[TriggerAparicionJumpscare] No se encontró VigenteMirror en la escena.");
                return;
            }
        }

        // Activamos el GameObject y el componente por si están deshabilitados
        if (!referenciaVigenteMirror.gameObject.activeInHierarchy)
        {
            referenciaVigenteMirror.gameObject.SetActive(true);
        }
        
        if (!referenciaVigenteMirror.enabled)
        {
            referenciaVigenteMirror.enabled = true;
        }

        // 1. Ejecutamos la aparición del espejo con la sacudida leve
        referenciaVigenteMirror.Aparecer();

        // 2. Notificamos al manager que se recogió la llave para iniciar la psicosis
        manager.LlaveRecogida();
    }
}
using System.Collections;
using UnityEngine;

public class TriggerAparicionJumpscare : MonoBehaviour
{
    private bool disparado3 = false;

    [SerializeField]
    private VigenteMirror referenciaVigenteMirror; // Asignar en inspector preferiblemente

    void Awake()
    {
        // Intento de fallback automático (solo si no se asignó en inspector)
        if (referenciaVigenteMirror == null)
        {
            referenciaVigenteMirror = Object.FindAnyObjectByType<VigenteMirror>();
            if (referenciaVigenteMirror == null)
            {
                // No se hace más aquí porque Resources puede devolver prefabs/asset, no instancia en escena.
                Debug.Log("[TriggerAparicionJumpscare] referenciaVigenteMirror no asignada en inspector y no encontrado con FindFirstObjectByType.");
            }
        }
    }

    void OnTriggerEnter(Collider other)
    {
        if (disparado3) return;
        if (!other.CompareTag("Player")) return;

        Act2Manager manager = Act2Manager.Instance;
        if (manager == null)
        {
            Debug.LogError("[TriggerAparicionJumpscare] Act2Manager no encontrado en la escena.");
            return;
        }

        if (!manager.TieneLlave())
        {
            Debug.Log("[TriggerAparicionJumpscare] El jugador pasó por el trigger pero no tiene la llave todavía.");
            return;
        }

        disparado3 = true;

        // Si no hay referencia válida, intentamos un último fallback (pero puede devolver prefabs).
        if (referenciaVigenteMirror == null)
        {
            referenciaVigenteMirror = Object.FindAnyObjectByType<VigenteMirror>();
            if (referenciaVigenteMirror == null)
            {
                Debug.LogWarning("[TriggerAparicionJumpscare] No se encontró VigenteMirror en escena. Asigna la instancia en el inspector.");
                return;
            }
        }

        // Si la instancia está en escena pero su GameObject o componente está desactivado, activarlo:
        if (!referenciaVigenteMirror.gameObject.activeInHierarchy)
        {
            Debug.Log("[TriggerAparicionJumpscare] Activando GameObject de VigenteMirror antes de iniciar la coroutine.");
            referenciaVigenteMirror.gameObject.SetActive(true);
        }

        if (!referenciaVigenteMirror.enabled)
        {
            referenciaVigenteMirror.enabled = true;
        }

        Debug.Log("[TriggerAparicionJumpscare] Iniciando AparicionEnEspejo.");
        referenciaVigenteMirror.StartCoroutine(referenciaVigenteMirror.AparicionEnEspejo());

        Act2Manager.Instance?.LlaveRecogida();
    }
}
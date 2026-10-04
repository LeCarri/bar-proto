using System.Collections;
using UnityEngine;

public class TriggerAparicionJumpscare : MonoBehaviour
{
    private bool disparado3 = false;

    [SerializeField]
    private VigenteMirror referenciaVigenteMirror; // Asignar en inspector preferiblemente

    void Awake()
    {
        // Intento de fallback autom�tico (solo si no se asign� en inspector)
        if (referenciaVigenteMirror == null)
        {
            referenciaVigenteMirror = Object.FindAnyObjectByType<VigenteMirror>();
            if (referenciaVigenteMirror == null)
            {
                // No se hace m�s aqu� porque Resources puede devolver prefabs/asset, no instancia en escena.
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
            Debug.Log("[TriggerAparicionJumpscare] El jugador pas� por el trigger pero no tiene la llave todav�a.");
            return;
        }

        disparado3 = true;

        // Si no hay referencia v�lida, intentamos un �ltimo fallback (pero puede devolver prefabs).
        if (referenciaVigenteMirror == null)
        {
            referenciaVigenteMirror = Object.FindAnyObjectByType<VigenteMirror>();
            if (referenciaVigenteMirror == null)
            {
                Debug.LogWarning("[TriggerAparicionJumpscare] No se encontr� VigenteMirror en escena. Asigna la instancia en el inspector.");
                return;
            }
        }

        // ---- VERSIÓN ANTERIOR (comentada en la reestructura) ----
        // if (!referenciaVigenteMirror.gameObject.activeInHierarchy)
        // {
        //     Debug.Log("[TriggerAparicionJumpscare] Activando GameObject de VigenteMirror antes de iniciar la coroutine.");
        //     referenciaVigenteMirror.gameObject.SetActive(true);
        // }
        //
        // if (!referenciaVigenteMirror.enabled)
        // {
        //     referenciaVigenteMirror.enabled = true;
        // }

        // ---- VERSIÓN ANTERIOR (comentada en la reestructura) ----
        // En la nueva estructura el Vigilante aparece en la puerta del baño de hombres y en el de mujeres
        // (AparicionVigilanteAct2) y el combate lo arranca el Act2Manager. Este trigger ya no hace nada.
        // Debug.Log("[TriggerAparicionJumpscare] Iniciando AparicionEnEspejo.");
        // referenciaVigenteMirror.StartCoroutine(referenciaVigenteMirror.AparicionEnEspejo());
        // Act2Manager.Instance?.LlaveRecogida();

        Debug.Log("[TriggerAparicionJumpscare] OBSOLETO en la reestructura de la Noche 2 — se puede desactivar este objeto.");
    }
}
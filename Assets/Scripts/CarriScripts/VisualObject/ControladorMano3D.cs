using System.Collections.Generic;
using UnityEngine;

public class ControladorMano3D : MonoBehaviour
{
    public static ControladorMano3D Instance;

    [Header("Punto de Agarre en la Cámara/Jugador")]
    [SerializeField] private Transform puntoMano;

    [Header("Ítem Activo Actual")]
    private ItemSO itemEquipadoActual;
    private GameObject objetoInstanciadoEnMano;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void EquiparItem(ItemSO nuevoItem)
    {
        // Limpiamos lo que haya en mano antes de equipar
        VaciarMano();

        itemEquipadoActual = nuevoItem;

        if (nuevoItem != null && nuevoItem.prefab3D != null && puntoMano != null)
        {
            objetoInstanciadoEnMano = Instantiate(nuevoItem.prefab3D, puntoMano);
            objetoInstanciadoEnMano.transform.localPosition = Vector3.zero;
            objetoInstanciadoEnMano.transform.localRotation = Quaternion.identity;
            Debug.Log($"[ControladorMano3D] Equipado con éxito: {nuevoItem.nombreItem}");
        }
        else
        {
            Debug.LogWarning("[ControladorMano3D] No se pudo equipar el ítem: Faltan referencias en ItemSO o puntoMano.");
        }
    }

    public void VaciarMano()
    {
        if (objetoInstanciadoEnMano != null)
        {
            Destroy(objetoInstanciadoEnMano);
            objetoInstanciadoEnMano = null; // <-- Limpiamos la referencia explícitamente
            Debug.Log("[ControladorMano3D] Objeto destruido de la mano.");
        }
        else
        {
            Debug.Log("[ControladorMano3D] VaciarMano llamado, pero no había ningún objeto instanciado.");
        }

        itemEquipadoActual = null;
    }

    public ItemSO ObtenerItemActual() => itemEquipadoActual;
    public bool TieneManoOcupada() => itemEquipadoActual != null;
}
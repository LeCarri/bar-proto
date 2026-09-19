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

    private void Start()
{
    // Oculta cualquier visual de Preview al iniciar el juego para que no moleste en pantalla
    Transform preview = puntoMano != null ? puntoMano.Find("PREVIEW_MANO_NO_BORRAR") : null;
    if (preview != null)
    {
        preview.gameObject.SetActive(false);
    }
}

public void EquiparItem(ItemSO nuevoItem)
{
    VaciarMano();

    itemEquipadoActual = nuevoItem;

    if (nuevoItem != null && nuevoItem.prefab3D != null && puntoMano != null)
    {
        // Instanciamos el objeto
        objetoInstanciadoEnMano = Instantiate(nuevoItem.prefab3D, puntoMano);

        // Forzamos que se active en jerarquía por si el prefab estaba desactivado
        objetoInstanciadoEnMano.SetActive(true);

        // Aseguramos el escalado y reseteamos offsets antes de aplicar las coordenadas del SO
        objetoInstanciadoEnMano.transform.localScale = 
            nuevoItem.escalaEnMano != Vector3.zero ? nuevoItem.escalaEnMano : Vector3.one;

        objetoInstanciadoEnMano.transform.localPosition = nuevoItem.posicionEnMano;
        objetoInstanciadoEnMano.transform.localRotation = Quaternion.Euler(nuevoItem.rotacionEnMano);

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
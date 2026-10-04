using UnityEngine;

/// <summary>
/// NOCHE 2 — Herramienta de limpieza que se recoge con [E] (escoba, trapo o cepillo).
///
/// Guion: "el jugador debe buscar la escoba en el depósito (...) al ir a buscar la escoba,
/// cae una hoja desde el armario". Para eso está el campo "objetoQueCae": arrastrá ahí la hoja
/// (dibujo de Pilar con la familia tachada). Empieza DESACTIVADA y al tomar la escoba aparece
/// y cae con física.
///
/// SETUP:
///  - Poner este script en el modelo de la escoba / trapo / cepillo (con Collider, capa "Interactable").
///  - Elegir el tipo de herramienta.
///  - (Opcional) itemEnMano: un ItemSO para mostrar la herramienta en la mano con ControladorMano3D.
/// </summary>
public class HerramientaLimpiezaAct2 : MonoBehaviour, IInteractable
{
    [Header("Herramienta")]
    public HerramientaAct2 tipo = HerramientaAct2.Escoba;
    [SerializeField] private string textoAccion = "Tomar escoba";

    [Tooltip("Opcional: ItemSO para ver la herramienta en la mano (ControladorMano3D).")]
    public ItemSO itemEnMano;

    [Header("Objeto que cae al tomarla (hoja del armario)")]
    [Tooltip("La hoja con el dibujo de Pilar. Debe empezar DESACTIVADA. Al tomar la herramienta aparece y cae.")]
    public GameObject objetoQueCae;
    [Tooltip("Empuje que recibe la hoja al caer (en coordenadas del mundo).")]
    public Vector3 empujeCaida = new Vector3(0f, 0f, 0.6f);
    public AudioSource sonidoHojaCayendo;

    [Header("Audio")]
    public AudioSource sonidoRecoger;

    private bool recogida = false;

    public bool CanInteract() => !recogida;
    public string GetDescription() => textoAccion;

    public void Interact()
    {
        if (recogida) return;
        recogida = true;

        Act2Manager manager = Act2Manager.Instance;
        if (manager != null) manager.DarHerramienta(tipo);
        else Debug.LogError("[HerramientaLimpiezaAct2] No hay Act2Manager en la escena.");

        if (sonidoRecoger != null) sonidoRecoger.Play();

        if (itemEnMano != null && ControladorMano3D.Instance != null)
            ControladorMano3D.Instance.EquiparItem(itemEnMano);

        if (objetoQueCae != null)
        {
            objetoQueCae.SetActive(true);

            Rigidbody rb = objetoQueCae.GetComponent<Rigidbody>();
            if (rb == null) rb = objetoQueCae.AddComponent<Rigidbody>();
            rb.isKinematic = false;
            rb.useGravity = true;
            rb.mass = 0.05f;
            rb.linearDamping = 3f;
            rb.angularDamping = 2f;
            rb.AddForce(empujeCaida, ForceMode.Impulse);

            if (sonidoHojaCayendo != null) sonidoHojaCayendo.Play();
        }

        // El sonido puede seguir sonando aunque se oculte el modelo
        foreach (Renderer r in GetComponentsInChildren<Renderer>())
            if (!EsParteDeLaHoja(r.transform)) r.enabled = false;
        foreach (Collider c in GetComponentsInChildren<Collider>())
            if (!EsParteDeLaHoja(c.transform)) c.enabled = false;
    }

    bool EsParteDeLaHoja(Transform t)
    {
        return objetoQueCae != null && t.IsChildOf(objetoQueCae.transform);
    }
}

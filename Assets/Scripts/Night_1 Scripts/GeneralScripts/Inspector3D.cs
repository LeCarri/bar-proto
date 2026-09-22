using UnityEngine;

public class Inspector3D : MonoBehaviour
{
    public static Inspector3D Instance { get; private set; }

    [Header("Punto donde se montan los objetos a inspeccionar")]
    [SerializeField] private Transform puntoInspeccion;

    [Header("Configuración de Rotación")]
    [SerializeField] private float velocidadRotacion = 5f;

    [Header("Referencias del Jugador")]
    [SerializeField] private PlayerController playerController; 

    private GameObject objetoActual;
    private bool estaInspeccionando = false;
    private bool listoParaCerrar = false;

    // Propiedad para que los objetos narrativos encuentren el Transform fácil
    public Transform PuntoInspeccion => puntoInspeccion;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    private void Update()
    {
        if (!estaInspeccionando || objetoActual == null) return;

        // Evita que la misma tecla [E] usada para interactuar cierre la inspección en el mismo frame
        if (!listoParaCerrar)
        {
            if (Input.GetKeyUp(KeyCode.E))
            {
                listoParaCerrar = true;
            }
            return;
        }

        // Rotación con Clic Izquierdo (Mouse Drag)
        if (Input.GetMouseButton(0))
        {
            float rotX = Input.GetAxis("Mouse X") * velocidadRotacion;
            float rotY = Input.GetAxis("Mouse Y") * velocidadRotacion;

            objetoActual.transform.Rotate(Vector3.up, -rotX, Space.World);
            objetoActual.transform.Rotate(Vector3.right, rotY, Space.World);
        }

        // Salir / Guardar objeto
        if (Input.GetKeyDown(KeyCode.E) || Input.GetKeyDown(KeyCode.Escape) || Input.GetMouseButtonDown(1))
        {
            CerrarInspeccion();
        }
    }

    public void IniciarInspeccion(GameObject objeto, string dialogo)
    {
        objetoActual = objeto;
        estaInspeccionando = true;
        listoParaCerrar = false; // Bloquea la salida inmediata durante el primer frame

        // 1. Bloquear controles del Player
        if (playerController != null)
        {
            playerController.controlesBloqueados = true;
        }

        // 2. Liberar el cursor
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        // 3. Mostrar subtítulo
        if (Act1Manager.Instance != null)
        {
            Act1Manager.Instance.MostrarDialogo(dialogo);
        }
    }

    public void CerrarInspeccion()
    {
        if (!estaInspeccionando) return;

        estaInspeccionando = false;
        listoParaCerrar = false;

        // 1. Ocultar y volver a bloquear cursor en el centro
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        // 2. Desbloquear controles del Player
        if (playerController != null)
        {
            playerController.controlesBloqueados = false;
        }

        // 3. Guardar / Desactivar objeto inspeccionado
        if (objetoActual != null)
        {
            objetoActual.SetActive(false);
            objetoActual = null;
        }
    }
}
using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class PlayerController : MonoBehaviour
{
    [Header("Movimiento")]
    public float walkSpeed = 3.0f;
    public float gravity = -20f; // Un poco más fuerte para que no flote
    
    [Header("Cámara (Cinemachine)")]
    [Tooltip("Arrastrá acá el objeto vacío 'PivoteMirada' que creaste dentro de Lucas.")]
    public Transform cameraPivot; 
    [HideInInspector] public Camera playerCamera; // Para que SettingsManager no rompa
    public float lookSpeed = 2.0f;
    public float lookXLimit = 80.0f;

    private CharacterController characterController;
    private Vector3 moveDirection = Vector3.zero;
    private float rotationX = 0;

    void Awake() // Usamos Awake para asegurarnos de captar el componente antes que nada
    {
        characterController = GetComponent<CharacterController>();
    }

    void Start()
    {
        // Si no asignaste el pivote en el inspector, busca un objeto hijo con cámara o el primero que encuentre
        if (cameraPivot == null) 
        {
            Camera cam = GetComponentInChildren<Camera>();
            if (cam != null) cameraPivot = cam.transform;
        }
        
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        // Busca la Main Camera y se la da al script de settings para que no tire error
        playerCamera = GetComponentInChildren<Camera>();
    }

    void Update()
    {
        // Si por alguna razón el componente desaparece, no ejecutamos nada más
        if (characterController == null) return;

        // 1. ROTACIÓN (Modificada para afectar al Pivote en lugar de a la cámara directa)
        rotationX += -Input.GetAxis("Mouse Y") * lookSpeed * Time.timeScale;
        rotationX = Mathf.Clamp(rotationX, -lookXLimit, lookXLimit);
        
        if (cameraPivot != null)
        {
            // Rotación vertical va al pivote vacío
            cameraPivot.localRotation = Quaternion.Euler(rotationX, 0, 0);
        }
        
        // Rotación horizontal (Gira todo el cuerpo de Lucas de izquierda a derecha)
        transform.rotation *= Quaternion.Euler(0, Input.GetAxis("Mouse X") * lookSpeed * Time.timeScale, 0);

        // 2. MOVIMIENTO
        float inputX = Input.GetAxis("Vertical");
        float inputY = Input.GetAxis("Horizontal");

        Vector3 forward = transform.forward;
        Vector3 right = transform.right;

        // Limpiamos el movimiento en Y para que no camine hacia el cielo/suelo
        forward.y = 0;
        right.y = 0;
        forward.Normalize();
        right.Normalize();

        float verticalSpeed = moveDirection.y;
        moveDirection = (forward * inputX + right * inputY) * walkSpeed;

        // 3. GRAVEDAD
        if (characterController.isGrounded)
        {
            moveDirection.y = -0.5f; // Mantener pegado al suelo
        }
        else
        {
            moveDirection.y = verticalSpeed + (gravity * Time.deltaTime);
        }

        characterController.Move(moveDirection * Time.deltaTime);
    }
}
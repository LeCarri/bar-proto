using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class PlayerController : MonoBehaviour
{
    [Header("Movimiento")]
    public float walkSpeed = 3.0f;
    public float runSpeed = 6.0f;
    public float gravity = -20f;
    [Header("Sensación de correr")]
    public float normalFOV = 60f;
    public float runFOV = 68f;
    public float fovTransitionSpeed = 6f;

    [Header("Audio")]
    public AudioSource audioPasos;

    [Header("Cámara (Cinemachine)")]
    [Tooltip("Arrastrá acá el objeto vacío 'PivoteMirada' que creaste dentro de Lucas.")]
    public Transform cameraPivot; 
    [HideInInspector] public Camera playerCamera;
    public float lookSpeed = 2.0f;
    public float lookXLimit = 80.0f;

    private CharacterController characterController;
    private Vector3 moveDirection = Vector3.zero;
    private float rotationX = 0;

    void Awake()
    {
        characterController = GetComponent<CharacterController>();
    }

    void Start()
    {
        if (cameraPivot == null) 
        {
            Camera cam = GetComponentInChildren<Camera>();
            if (cam != null) cameraPivot = cam.transform;
        }
        
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        playerCamera = GetComponentInChildren<Camera>();
    }

    void Update()
    {
        if (characterController == null) return;

        rotationX += -Input.GetAxis("Mouse Y") * lookSpeed * Time.timeScale;
        rotationX = Mathf.Clamp(rotationX, -lookXLimit, lookXLimit);
        
        if (cameraPivot != null)
        {
            cameraPivot.localRotation = Quaternion.Euler(rotationX, 0, 0);
        }
        
        transform.rotation *= Quaternion.Euler(0, Input.GetAxis("Mouse X") * lookSpeed * Time.timeScale, 0);

        float inputX = Input.GetAxis("Vertical");
        float inputY = Input.GetAxis("Horizontal");

        Vector3 forward = transform.forward;
        Vector3 right = transform.right;

        forward.y = 0;
        right.y = 0;
        forward.Normalize();
        right.Normalize();

        float verticalSpeed = moveDirection.y;

        float currentSpeed = GetCurrentSpeed();

        moveDirection = (forward * inputX + right * inputY) * currentSpeed;

        if (characterController.isGrounded)
        {
            moveDirection.y = -0.5f;
        }
        else
        {
            moveDirection.y = verticalSpeed + (gravity * Time.deltaTime);
        }
    if (playerCamera != null)
{
    bool isRunning = Input.GetKey(KeyCode.LeftShift) && (inputX != 0 || inputY != 0);
    float targetFOV = isRunning ? runFOV : normalFOV;
    playerCamera.fieldOfView = Mathf.Lerp(playerCamera.fieldOfView, targetFOV, Time.deltaTime * fovTransitionSpeed);
}
        bool caminando =
        (inputX != 0 || inputY != 0);

        if (audioPasos != null)
        {
            if (caminando)
            {
                if (!audioPasos.isPlaying)
                    audioPasos.Play();
            }
            else
            {
                audioPasos.Stop();
            }
        }
        characterController.Move(moveDirection * Time.deltaTime);
    }

    float GetCurrentSpeed()
    {
        if (Input.GetKey(KeyCode.LeftShift))
        {
            return runSpeed;
        }

        return walkSpeed;
    }
}
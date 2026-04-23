using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMovement : MonoBehaviour
{
    DialogueSystem dialogueSys;

    [SerializeField]
    private float speed = 1f;

    [SerializeField]
    private float maxSpeed = 5f;

    //[SerializeField]
    private Transform cameraTransform;

    private Rigidbody rb;
    private Vector2 moveInput;

    private void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        cameraTransform = GameObject.FindWithTag("MainCamera").transform;
        rb = GetComponent<Rigidbody>();
        Debug.Log(rb);
        dialogueSys = DialogueSystem.Instance;
    }

    private void FixedUpdate()
    {
        if ((moveInput.y != 0 || moveInput.x != 0) && !InDialogue())
            Move();
    }

    private void Move() 
    {
        CapXSpeed();
        CapZSpeed();

        Vector3 move =
            cameraTransform.forward * moveInput.y + cameraTransform.right * moveInput.x;
        move.y = 0;
        rb.AddForce(move.normalized * speed * Time.deltaTime, ForceMode.VelocityChange);
    }

    private void CapXSpeed() 
    {
        rb.linearVelocity = new Vector3(
        Mathf.Clamp(rb.linearVelocity.x, -maxSpeed, maxSpeed),
        rb.linearVelocity.y,
        rb.linearVelocity.z);
        //Debug.Log(rb.linearVelocity);
    }
    private void CapZSpeed() 
    {
        rb.linearVelocity = new Vector3(
                    rb.linearVelocity.x,
                    rb.linearVelocity.y,
                    Mathf.Clamp(rb.linearVelocity.z, -maxSpeed, maxSpeed));
    }

    public void OnMove(InputValue value) 
    {
        moveInput = value.Get<Vector2>(); // x = A/D && y = W/D
        
    }

    private bool InDialogue() 
    {
        if (dialogueSys != null) 
        {
            if (dialogueSys.inDialogue == false)
                return false;
            else
                return true;
        }
        else return false;
    }

}

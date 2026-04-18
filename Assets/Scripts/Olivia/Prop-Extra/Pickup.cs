using Unity.Cinemachine;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;

public class Pickup : MonoBehaviour
{
    PlayerInteract playerInteract;

    Rigidbody playerRB;
    
    InputAction inputReleaseRPickup;
    InputAction inputAltMode;
    InputAction inputToggleRotation;
    InputAction inputToggleMovement;


    public bool isHolding = false;
    
    private PickupMode pickMode = PickupMode.Holding;

    float throwForce = 8f;
    float rotationMultiplier = 15f;

    [SerializeField]
    float slowModeMultiplier = 0.5f;
   
    float maxDistancce;
    float distance;

    LiquidContainer lc;
    TempParent tempParent;
    Rigidbody rb;

    Vector3 objPos;

    private void Start()
    {
        rb = GetComponent<Rigidbody>();
        lc = GetComponent<LiquidContainer>();
        //tempParent = TempParent.Instance;
        GetDependencies();

        //inputInteraction = InputSystem.actions.FindAction("Interact");
        inputReleaseRPickup = InputSystem.actions.FindAction("ReleaseRPickup");
        inputAltMode = InputSystem.actions.FindAction("ThrowMode");
        inputToggleRotation = InputSystem.actions.FindAction("RotationMode");
        inputToggleMovement = InputSystem.actions.FindAction("MovingMode");
    }

    private void Update()
    {
        if (isHolding == true) 
        { Hold(); }
    }

    private void GetDependencies() 
    {
        while (tempParent == null || playerRB == null || playerInteract == null) 
        {
            playerInteract = GameObject.FindWithTag("Player").GetComponent<PlayerInteract>();
            tempParent = GameObject.FindWithTag("TempParent").GetComponent<TempParent>();
            playerRB = GameObject.FindWithTag("Player").GetComponent<Rigidbody>();

            maxDistancce = playerInteract.playerReach;
        }
    }

    public void OnPickup() 
    {
        if (tempParent != null)
        {
            distance = Vector3.Distance(this.transform.position, tempParent.transform.position);
            if (distance < maxDistancce) {
                isHolding = true;
                rb.useGravity = false;
                rb.detectCollisions = true;
                
                this.transform.SetParent(tempParent.transform);
                
            }

            if (lc != null) 
            {
                UIManager.Instance.HideShowPickupInfo(true);
                UIManager.Instance.HideShowPickupControls(true);
                Debug.Log("Can contain liquids");
            }
            //Debug.Log("Temp parent detected");
        }
        else { Debug.Log("No temp parent detected"); }
    }
    private void Hold() 
    {
        distance = Vector3.Distance(this.transform.position, tempParent.transform.position);
        if (distance >= maxDistancce)
            Drop();

        rb.linearVelocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;

        if (inputToggleRotation.WasCompletedThisFrame()) //Cambiar modos.
            TransitionToMode(PickupMode.Rotating);

        if (inputToggleMovement.WasCompletedThisFrame())
            TransitionToMode(PickupMode.Moving);

                //Hacer las correspondientes funciones para cada modo.
        if (pickMode == PickupMode.Holding)
        {
            if (inputAltMode.IsPressed() && inputReleaseRPickup.WasPerformedThisFrame()) // Shift + RMB
            {
                Throw(); //throw
            }
            else if (inputReleaseRPickup.WasPerformedThisFrame()) // RMB
            {
                Drop(); //drop
            }
        }
        else
        {
            switch (pickMode)
            {
                case PickupMode.Rotating: OnRotationMode(); break;
                case PickupMode.Moving: OnMoveMode(); break;
            }
        }

        if (lc != null)
        { playerInteract.HandleLiquidContainerPickup(lc); }

        //Debug.Log(distance + "/" + maxDistancce);
    }

    private void Drop(bool applyPlayerForce = false) 
    {
        if (isHolding)
        {
            if (lc != null)
                UIManager.Instance.HideShowPickupInfo(false);

            UIManager.Instance.HideShowPickupControls(false);
            TransitionToMode(PickupMode.Holding);
            
            isHolding = false;
            playerInteract.ResetCurrentPickup();

            objPos = this.transform.position;
            this.transform.position = objPos;
            
            this.transform.SetParent(null);
            rb.useGravity = true;

            if (applyPlayerForce)
                ApplyForceToDroppedPickup(throwForce);
            else
                ApplyForceToDroppedPickup();

            

            //Debug.Log("Dropped the pickup");
        }
    }

    private void ApplyForceToDroppedPickup() 
    {
        Vector3 playerVelocity = playerRB.linearVelocity;

        Rigidbody rb = GetComponent<Rigidbody>();

        rb.linearVelocity = playerVelocity * 1.1f;
    }

    private void ApplyForceToDroppedPickup(float Force)
    {
        Vector3 playerVelocity = playerRB.linearVelocity;

        Rigidbody rb = GetComponent<Rigidbody>();

        rb.linearVelocity = playerVelocity + tempParent.transform.forward * Force;
    }

    private void Throw() 
    {
        Drop(true);
    }

    private void TransitionToMode(PickupMode newMode) 
    {
        CinemachineInputAxisController cameraController =
            GameObject.FindWithTag("CinemachineCamera").GetComponent<CinemachineInputAxisController>();

        if (newMode == PickupMode.Holding)
        {
            cameraController.enabled = true;
        }
        else if (newMode == pickMode) 
        {
            pickMode = PickupMode.Holding;
            cameraController.enabled = true;
            return;
        }
        else
        {
            cameraController.enabled = false;
        }

            pickMode = newMode;
    }

    private void OnRotationMode() 
    {
        Vector2 lookValues = InputSystem.actions.FindAction("Look").ReadValue<Vector2>();

        if ( lookValues.y != 0)
        {
            if (inputAltMode.IsPressed())
                transform.Rotate(new Vector3(-lookValues.y * slowModeMultiplier * Time.deltaTime * rotationMultiplier,
                                                -lookValues.x * Time.deltaTime * rotationMultiplier, 0));
            else
                transform.Rotate(new Vector3(-lookValues.y * Time.deltaTime * rotationMultiplier,
                    -lookValues.x * Time.deltaTime * rotationMultiplier, 0));
            //transform.Rotate(new Vector3(lookValues.y, 0, 0));
        }

        Debug.Log(InputSystem.actions.FindAction("Look").ReadValue<Vector2>());
        //Debug.Log(inputRotationValues.ReadValue<float>());
    }

    private void OnMoveMode() 
    {
        Vector2 lookValues = InputSystem.actions.FindAction("Look").ReadValue<Vector2>();

        Transform cameraTransform = GameObject.FindWithTag("MainCamera").transform;

        if (lookValues.y != 0 || lookValues.x != 0)
        {
            if (inputAltMode.IsPressed())
            {
                //transform.Translate(new Vector3(0, 0, lookValues.y * Time.deltaTime * 0.6f * slowModeMultiplier));
                //transform.localPosition += new Vector3
                //    (cameraTransform.right.x * -lookValues.x * Time.deltaTime * 0.6f * slowModeMultiplier, 0, 0);
                transform.Translate(new Vector3(0, 0, lookValues.y * Time.deltaTime * 0.6f));
                transform.localPosition += new Vector3(0, 0, cameraTransform.right.x * -lookValues.x * Time.deltaTime * 0.6f);
            }
            else
            {
                transform.Translate(new Vector3(0, 0, lookValues.y * Time.deltaTime * 0.6f));
                transform.localPosition += new Vector3(cameraTransform.right.x * -lookValues.x * Time.deltaTime * 0.6f, 0, 0);
            }
        }

        //Arreglar el movimiento en z
        Debug.Log(InputSystem.actions.FindAction("Look").ReadValue<Vector2>());
    }
}

public enum PickupMode 
    { 
        Holding, // Mientras se mueve el jugador
        Rotating, // Mientras se rota el objeto
        Moving // Mientras se mueve el objeto
    }

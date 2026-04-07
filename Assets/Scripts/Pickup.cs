using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;

public class Pickup : MonoBehaviour
{
    PlayerInteract playerInteract;

    Rigidbody playerRB;
    
    InputAction inputReleaseRPickup;
    InputAction inputThrowMode;


    public bool isHolding = false;

    float throwForce = 8f;
   
    float maxDistancce;
    float distance;

    TempParent tempParent;
    Rigidbody rb;

    Vector3 objPos;

    private void Start()
    {
        rb = GetComponent<Rigidbody>();
        //tempParent = TempParent.Instance;
        GetDependencies();

        //inputInteraction = InputSystem.actions.FindAction("Interact");
        inputReleaseRPickup = InputSystem.actions.FindAction("ReleaseRPickup");
        inputThrowMode = InputSystem.actions.FindAction("ThrowMode");
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
            //Debug.Log("Temp parent detected");
        }
        else { Debug.Log("No temp parent detected"); }
    }

    private void OnRelease() { }

    private void OnSlip() { }

    private void Hold() 
    {
        distance = Vector3.Distance(this.transform.position, tempParent.transform.position);
        if (distance >= maxDistancce)
            Drop();

        rb.linearVelocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;

        if (inputThrowMode.IsPressed() && inputReleaseRPickup.WasPerformedThisFrame()) // Shift + RMB
        {
            Throw(); //throw
        }
        else if (inputReleaseRPickup.WasPerformedThisFrame()) // RMB
        {
            Drop(); //drop
        }

        //Debug.Log(distance + "/" + maxDistancce);
    }

    private void Drop(bool applyPlayerForce = false) 
    {
        if (isHolding)
        {
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

            

            Debug.Log("Dropped the pickup");
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
}

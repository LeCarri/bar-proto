using System.Collections;
using Unity.IO.LowLevel.Unsafe;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerInteract : MonoBehaviour
{
    public static PlayerInteract Instance { get; private set; }

    public float playerReach = 3f;

    [SerializeField]
    Pickup currentPickup = null;

    InputAction inputPickupWithRightHand;

    Transform HandsCenterParent;

    [SerializeField]
    GameObject R_HandObject;
    Vector3 RH_initialPos; 
    

    private void Start()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(this);

        inputPickupWithRightHand = InputSystem.actions.FindAction("PickupRHand");

        HandsCenterParent = R_HandObject.transform.parent;
        //Debug.Log(R_HandObject.transform.localPosition);
        //RH_initialPos = R_HandObject.transform.position; //hand spawn pos
    }

    private void Update()
    {

        if (inputPickupWithRightHand.WasReleasedThisFrame() && currentPickup == null) 
        {
            //Debug.Log("Checking for pickups");
            CheckPickup();
        }
    }

    void CheckPickup() 
    {

        RaycastHit hit;
        Ray ray = new Ray(Camera.main.transform.position, Camera.main.transform.forward);

        if (Physics.Raycast(ray, out hit, playerReach))
        {
            if (hit.collider.tag == "Pickup")
            {
                Debug.Log("Recognized tag"); //SetNewPickup

                Pickup newPickup = hit.collider.GetComponent<Pickup>();

                if ( newPickup.isHolding != true)
                {
                    //set new pickup
                    currentPickup = newPickup;
                    //move hand to pickup
                    HandleItem(R_HandObject);
                    //tell pickup is being held
                    //newPickup.OnPickup();

                    Debug.Log(currentPickup);
                }

            }
        }
    }

    private void HandleItem(GameObject hand) 
    {
        //set hand parent to correct handle
        hand.transform.SetParent(null);

        //move hand to handle
        StartCoroutine(MoveHandToPickup(hand, currentPickup.gameObject));
    }

    private IEnumerator MoveHandToPickup(GameObject hand, GameObject targetObj) 
    {
        //move hand to item
        while (true) 
        {   
            if (Vector3.Distance(hand.transform.position, targetObj.transform.position) > 0.2f)
                hand.transform.position = Vector3.Slerp(hand.transform.position, targetObj.transform.position, 1f);
            else
            {
                hand.transform.position = targetObj.transform.position;

                hand.transform.SetParent(targetObj.transform);
                break;
            }

            //Debug.Log(Vector3.Distance(hand.transform.position, targetObj.transform.position));

            yield return new WaitForSeconds(0.5f);
        }

        //tell pickup is being held
        currentPickup.OnPickup();
        Debug.Log("End of couroutine 1");
        yield break;

    }

    private IEnumerator MoveHandToPlayer(GameObject hand, Vector3 initialPos) 
    {
        //reset parent

        hand.transform.SetParent(HandsCenterParent);

        //reset hand pos
        while (true)
        {
            if (Vector3.Distance(hand.transform.localPosition, initialPos) > 0.2f)
                hand.transform.localPosition = Vector3.Slerp(hand.transform.position, initialPos, 1f);
            else
            {
                hand.transform.localPosition = initialPos;
                hand.transform.localEulerAngles = new Vector3(-90, 180, 0);
                break;
            }

            //Debug.Log(Vector3.Distance(hand.transform.localPosition, initialPos));
    
            yield return new WaitForSeconds(0.2f);
        }

        //Debug.Log("End of couroutine 2");
        yield break;
    }

    public void ResetCurrentPickup() 
    {
        currentPickup = null;

        //move hand back to player
        StartCoroutine(MoveHandToPlayer(R_HandObject, RH_initialPos));

        Debug.Log(currentPickup);
    }
}

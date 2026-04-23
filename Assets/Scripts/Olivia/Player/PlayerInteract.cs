using System.Collections;
using Unity.Cinemachine;
using Unity.IO.LowLevel.Unsafe;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerInteract : MonoBehaviour
{
    UIManager ui;

    DialogueSystem dialogueSys;
    CinemachineInputAxisController cameraController;
    
    public static PlayerInteract Instance { get; private set; }

    public float playerReach = 3f;

    [SerializeField]
    public Pickup currentPickup = null;

    InputAction inputPickupWithRightHand;

    Transform HandsCenterParent;

    [SerializeField]
    GameObject R_HandObject;
    Vector3 RH_initialPos;

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(this);
    }

    private void Start()
    {
        dialogueSys = DialogueSystem.Instance;
        inputPickupWithRightHand = InputSystem.actions.FindAction("PickupRHand");
        ui = UIManager.Instance;
        cameraController = GameObject.FindWithTag("CinemachineCamera").GetComponent<CinemachineInputAxisController>();

        HandsCenterParent = R_HandObject.transform.parent;
        //Debug.Log(R_HandObject.transform.localPosition);
        //RH_initialPos = R_HandObject.transform.position; //hand spawn pos
    }

    private void Update()
    {

        if (inputPickupWithRightHand.WasReleasedThisFrame() && currentPickup == null && dialogueSys.inDialogue == false) 
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
                //Debug.Log("Recognized tag"); //SetNewPickup

                Pickup newPickup = hit.collider.GetComponent<Pickup>();

                if ( newPickup.isHolding != true)
                {
                    //set new pickup
                    currentPickup = newPickup;
                    
                    HandleItem(R_HandObject);
                }

            }
        }
    }

    public void HandleLiquidContainerPickup(LiquidContainer providedContainer) 
    {
        ui.PickupInfo.text =
            "- " + providedContainer.containerName + "\n" +
            "- Contains: " + providedContainer.GetLiquidType() + "\n" +
            "- " + providedContainer.GetStoragedLiquid() + "/" + providedContainer.GetMaxStoragedLiquid() + "ml";
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
        //lock camera
        cameraController.enabled = false;

        //move hand to item
        hand.transform.LeanMove(targetObj.transform.position, 0.4f)
            .setEaseOutSine()
            .setOnComplete(() => hand.transform.SetParent(targetObj.transform));

        //tell pickup is being held
        yield return new WaitForSeconds(0.4f);

        currentPickup.OnPickup(); //pickup obj
        cameraController.enabled = true; //unlock camera
        //Debug.Log("End of couroutine 1");
        yield break;

    }

    private IEnumerator MoveHandToPlayer(GameObject hand, Vector3 initialPos) 
    {
        //reset parent

        hand.transform.SetParent(HandsCenterParent);

        //reset hand pos
        hand.transform.LeanMoveLocal(initialPos, 0.3f)
            .setEaseOutSine();
        LeanTween.rotateLocal(hand, new Vector3(-90, 180, 0), 0.2f)
            .setEaseOutSine();

        //while (true)
        //{
        //    if (Vector3.Distance(hand.transform.localPosition, initialPos) > 0.2f)
        //        hand.transform.localPosition = Vector3.Slerp(hand.transform.position, initialPos, 1f);
        //    else
        //    {
        //        hand.transform.localPosition = initialPos;
        //        hand.transform.localEulerAngles = new Vector3(-90, 180, 0);
        //        break;
        //    }
        //
        //    //Debug.Log(Vector3.Distance(hand.transform.localPosition, initialPos));
        //
        //    yield return new WaitForSeconds(0.1f);
        //}

        //Debug.Log("End of couroutine 2");
        yield break;
    }

    public void ResetCurrentPickup() 
    {
        currentPickup = null;

        //move hand back to player
        StartCoroutine(MoveHandToPlayer(R_HandObject, new Vector3(2,0,0)));

        //Debug.Log(currentPickup);
    }
}

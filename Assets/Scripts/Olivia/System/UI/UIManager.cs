using TMPro;
using UnityEngine;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance { get; private set; }

    [SerializeField]
    private GameObject pickupControls;

    [SerializeField]
    public TextMeshProUGUI PickupInfo;

    [SerializeField]
    public TextMeshProUGUI LookingAtInfo;

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(this);
    }

    private void Start()
    {
        HideShowPickupInfo(false);
    }

    private void Update()
    {
        CheckForContainers();
    }

    #region Hide or show funcs
    public void HideShowPickupInfo(bool value) 
    {
        PickupInfo.transform.parent.gameObject.SetActive(value);
    }

    public void HideShowLookingInfo(bool value)
    {
        LookingAtInfo.transform.parent.gameObject.SetActive(value);
    }

    public void HideShowPickupControls(bool value) 
    {
        pickupControls.SetActive(value);
    }

    #endregion

    private void CheckForContainers() 
    {

        RaycastHit hit;
        Ray ray = new Ray(Camera.main.transform.position, Camera.main.transform.forward);

        if (Physics.Raycast(ray, out hit, PlayerInteract.Instance.playerReach))
        {
            if (hit.collider.GetComponent<LiquidContainer>() != null && PlayerInteract.Instance.currentPickup != hit.collider.GetComponent<Pickup>())
            {
                HideShowLookingInfo(true);
                HandleLiquidContainerInFront(hit.collider.GetComponent<LiquidContainer>());
            }
            else
                HideShowLookingInfo(false);
        }
        else
            HideShowLookingInfo(false);
    }

    public void HandleLiquidContainerInFront(LiquidContainer providedContainer)
    {
        LookingAtInfo.text =
            providedContainer.containerName + " -" + "\n" +
            "Contains: " + providedContainer.GetLiquidType() + " -" + "\n" +
            providedContainer.GetStoragedLiquid() + "/" + providedContainer.GetMaxStoragedLiquid() + "ml" + " -";
    }

}

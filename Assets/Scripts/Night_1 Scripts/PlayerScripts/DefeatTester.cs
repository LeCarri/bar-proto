using UnityEngine;

public class DefeatTester : MonoBehaviour
{
    [SerializeField] private GameObject defeatCanvas;

    private void Awake()
    {
        if (defeatCanvas != null)
        {
            defeatCanvas.SetActive(false);
        }
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.K))
        {
            if (defeatCanvas != null)
            {
                defeatCanvas.SetActive(true);
            }
        }
    }
}

using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class UIConitnueHandler : MonoBehaviour
{
    public static UIConitnueHandler Instance;

    [SerializeField]
    [InspectorName("Texto de inicio.")]
    private TextMeshProUGUI textObj;

    [SerializeField]
    [InspectorName("Texto alternativo")]
    [Tooltip("El texto que se verá en el botón si ya hay una partida progresada.")]
    private string altText;

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
    }

    void Start()
    {
        if (textObj != null && SaveManager.Instance != null && SaveManager.Instance.GetSavedAct() > 1)
            textObj.text = altText;
    }

    public void UpdateText() 
    {
        if (textObj != null && SaveManager.Instance != null && SaveManager.Instance.GetSavedAct() > 1)
            textObj.text = altText;
        else
            textObj.text = "Comenzar";
    }

    public void WipeData() 
    {
        if (SaveManager.Instance != null)
            SaveManager.Instance.WipeData();
    }

}

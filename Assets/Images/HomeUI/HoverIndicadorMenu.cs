
using UnityEngine;
using UnityEngine.EventSystems;

public class HoverIndicadorMenu : MonoBehaviour,
    IPointerEnterHandler,
    IPointerExitHandler,
    ISelectHandler
{
    [SerializeField] private IndicadorMenu indicadorMenu;

    private RectTransform rectTransform;

    private void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (indicadorMenu != null)
            indicadorMenu.MoverA(rectTransform);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (indicadorMenu != null)
            indicadorMenu.SalirDe(rectTransform);
    }

    public void OnSelect(BaseEventData eventData)
    {
        if (indicadorMenu != null)
            indicadorMenu.MoverA(rectTransform);
    }
}

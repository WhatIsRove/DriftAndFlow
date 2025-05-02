using UnityEngine;
using UnityEngine.EventSystems;

public class Tooltip : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [TextArea]
    public string description;

    public void OnPointerEnter(PointerEventData eventData)
    {
        FindObjectOfType<InventoryManager>().ShowToolTip(description);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        FindObjectOfType<InventoryManager>().HideToolTip();
    }
}
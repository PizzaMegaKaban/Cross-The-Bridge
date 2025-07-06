using UnityEngine;
using UnityEngine.EventSystems;

public class UiGmCanvas : MonoBehaviour, IPointerDownHandler
{
    public void OnPointerDown(PointerEventData eventData)
    {
        Debug.Log("Before canvas was clicked!");
        EventManager.BlockStopClick.Invoke();
        Debug.Log("Canvas was clicked!");
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class BlackPanelScript : MonoBehaviour, IPointerClickHandler
{
    public void OnPointerClick(PointerEventData eventData)
    {
        // Debug.Log("Before canvas was clicked!");
        EventManager.OnBlackPanelMissClick.Invoke();
        // Debug.Log("Canvas was clicked!");
    }
}

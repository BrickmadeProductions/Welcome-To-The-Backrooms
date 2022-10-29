using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class InventoryItemDropArea : MonoBehaviour, IDropHandler
{
    public void OnDrop(PointerEventData eventData)
    {
        
        GameSettings.Instance.Player.GetComponent<InteractionSystem>().SetDrop(GameSettings.Instance.Player.GetComponent<InventorySystem>().currentItemSlected.slotIn);
        GameSettings.Instance.Player.GetComponent<InventorySystem>().canOpen = true;
        GameSettings.Instance.Player.GetComponent<InventorySystem>().currentItemSlected = null;
    }
}

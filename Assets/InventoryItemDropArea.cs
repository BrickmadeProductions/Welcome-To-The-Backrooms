using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class InventoryItemDropArea : MonoBehaviour, IDropHandler
{
    public void OnDrop(PointerEventData eventData)
    {
        
        GameSettings.GetLocalPlayer().GetComponent<InteractionSystem>().SetDrop(GameSettings.GetLocalPlayer().GetComponent<InventorySystem>().currentItemSlected.slotIn);
        GameSettings.GetLocalPlayer().GetComponent<InventorySystem>().canOpen = true;
        GameSettings.GetLocalPlayer().GetComponent<InventorySystem>().currentItemSlected = null;
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class InventorySlot : MonoBehaviour, IDropHandler
{
    public List<InventoryItem> itemsInSlot;
    
    public int weightAllowed;

    public List<OBJECT_TYPE> whiteList;

    public Transform physicalLocation;


    public int GetCurrentSlotWeight()
    {
        int currentSlotWeight = 0;

        foreach (InventoryItem item in itemsInSlot)
        {
            currentSlotWeight += item.connectedObject.inventoryObjectData.inventoryWeight;
        }

        return currentSlotWeight;
    }
    public void RemoveItemFromSlot(InventoryItem invItem, bool destroyItem)
    {
        itemsInSlot.Remove(invItem);

        invItem.connectedObject.RemoveMetaData("INV_SLOT");

        if (destroyItem)
            Destroy(invItem.gameObject);
    }
    public void RemoveItemFromSlot(HoldableObject objectRemove, bool destroyItem)
    {
        foreach (InventoryItem invItem in itemsInSlot)
        {
            if (invItem.connectedObject == objectRemove)
            {
                itemsInSlot.Remove(invItem);
                objectRemove.RemoveMetaData("INV_SLOT");

                if (destroyItem)

                    Destroy(invItem.gameObject);
                break;
            }
        }

    }

    public void AddItemToSlot(InventoryItem invItem)
    {

        //exit the method and dont do anything if the slot can't hold anymore items
        /*if (GetCurrentSlotWeight() + invItem.connectedObject.inventoryObjectData.inventoryWeight > weightAllowed)
            return;*/

        if (itemsInSlot.Count > 0)
            return;

        if (whiteList.Count > 0)
            if (!whiteList.Contains(invItem.connectedObject.type))
                return;

        if (GameSettings.Instance.Player.GetComponent<InventoryMenuSystem>().rHand == this && itemsInSlot.Count > 0)
            return;

       
        //remove current item from that slot
        GameSettings.Instance.Player.GetComponent<InventoryMenuSystem>().currentItemSlected.slotIn.itemsInSlot.Remove(
            GameSettings.Instance.Player.GetComponent<InventoryMenuSystem>().currentItemSlected
            );

        //set itemSlotInfo
        invItem.slotIn = this;
        itemsInSlot.Add(invItem);

        invItem.connectedObject.transform.parent = physicalLocation;
        invItem.connectedObject.transform.position = physicalLocation.position;
        invItem.connectedObject.transform.localRotation = physicalLocation.localRotation;

        invItem.connectedObject.rb.isKinematic = true;
        BPUtil.SetAllColliders(invItem.connectedObject.transform, false);

        invItem.connectedObject.SetMetaData("INV_SLOT", name);

        /*if (itemsInSlot.Count == 0)
        {
            
            
        }
        else //swap
        {
            //old slot
            InventorySlot oldSlot = invItem.slotIn;

            oldSlot.RemoveItemFromSlot(invItem, false);

            oldSlot.AddItemToSlot(itemsInSlot[0]);

            invItem.slotIn = this;
            itemsInSlot.Add(invItem);

            invItem.connectedObject.transform.parent = physicalLocation;
            invItem.connectedObject.transform.position = physicalLocation.position;
            invItem.connectedObject.transform.localRotation = physicalLocation.localRotation;

            invItem.connectedObject.rb.isKinematic = true;
            BPUtil.SetAllColliders(invItem.connectedObject.transform, false);
        }
*/
        //call the callback in the interactionsystem to update the physicals
        GameSettings.Instance.Player.GetComponent<InteractionSystem>().OnInventoryItemAddedToSlot_CallBack(this, invItem.connectedObject);

    }
    public void AddItemToSlot(HoldableObject holdableObject)
    {
        //exit the method and dont do anything if the slot can't hold anymore items
        /*if (GetCurrentSlotWeight() + holdableObject.inventoryObjectData.inventoryWeight > weightAllowed)
            return;*/
        if (itemsInSlot.Count > 0)
            return;

        if (GameSettings.Instance.Player.GetComponent<InventoryMenuSystem>().rHand == this && itemsInSlot.Count > 0)
            return;

        GameObject newInventoryItem = Instantiate(GameSettings.Instance.inventoryItemPrefab.gameObject, transform);

        
        //set itemSlotInfo
        newInventoryItem.GetComponent<InventoryItem>().SetDetails(holdableObject.inventoryObjectData, holdableObject, this);
        itemsInSlot.Add(newInventoryItem.GetComponent<InventoryItem>());

        holdableObject.rb.isKinematic = true;
        BPUtil.SetAllColliders(holdableObject.transform, false);

        holdableObject.transform.parent = physicalLocation;
        holdableObject.transform.position = physicalLocation.position;
        holdableObject.transform.localRotation = physicalLocation.localRotation;

        holdableObject.SetMetaData("INV_SLOT", name);
        //call the callback in the interactionsystem to update the physicals
        GameSettings.Instance.Player.GetComponent<InteractionSystem>().OnInventoryItemAddedToSlot_CallBack(this, holdableObject);
    }

    public void OnDrop(PointerEventData data)
    {
        if (GameSettings.Instance.Player.GetComponent<InventoryMenuSystem>().currentItemSlected != null)
        {
            AddItemToSlot(GameSettings.Instance.Player.GetComponent<InventoryMenuSystem>().currentItemSlected);

            GameSettings.Instance.Player.GetComponent<InventoryMenuSystem>().currentItemSlected = null;

            Debug.Log("Dropping To " + name);
        }
        
    }
}


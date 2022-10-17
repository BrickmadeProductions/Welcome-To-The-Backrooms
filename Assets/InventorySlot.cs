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
        InventorySlot slotFrom = invItem.slotIn;
        //exit the method and dont do anything if the slot can't hold anymore items
        /*if (GetCurrentSlotWeight() + invItem.connectedObject.inventoryObjectData.inventoryWeight > weightAllowed)
            return;*/

        if (whiteList.Count > 0)
            if (!whiteList.Contains(invItem.connectedObject.type))
                return;


        //remove current item we are putting into here from its original slot (do this before potential swap)
        invItem.slotIn.itemsInSlot.Remove(invItem);

        //swap senerio
        if (itemsInSlot.Count > 0)
        {

            //put the item from here into the other slot
            InventoryItem itemFromThisSlot = itemsInSlot[0];

            itemsInSlot.Remove(itemFromThisSlot);

            itemFromThisSlot.slotIn = invItem.slotIn;
            invItem.slotIn.itemsInSlot.Add(itemFromThisSlot);

            itemFromThisSlot.connectedObject.transform.parent = invItem.slotIn.physicalLocation;
            itemFromThisSlot.connectedObject.transform.position = invItem.slotIn.physicalLocation.position;
            itemFromThisSlot.connectedObject.transform.localRotation = invItem.slotIn.physicalLocation.localRotation;
            itemFromThisSlot.connectedObject.transform.localPosition = Vector3.zero;

            itemFromThisSlot.connectedObject.rb.isKinematic = true;
            BPUtil.SetAllColliders(itemFromThisSlot.connectedObject.transform, false);

            //itemFromThisSlot.connectedObject.SetMetaData("INV_SLOT", itemFromThisSlot.slotIn.name);
            //GameSettings.Instance.Player.GetComponent<InventoryMenuSystem>().SetSaveData(itemFromThisSlot.slotIn.name, itemFromThisSlot.connectedObject.GetWorldID());

            itemFromThisSlot.transform.parent = itemFromThisSlot.slotIn.transform;
            itemFromThisSlot.transform.position = itemFromThisSlot.slotIn.transform.position;

            GameSettings.Instance.Player.GetComponent<InventorySystem>().currentItemSlected = null;
            itemFromThisSlot.canvasGroup.alpha = 1f;
            itemFromThisSlot.canvasGroup.blocksRaycasts = true;

        }


        //set itemSlotInfo
        invItem.slotIn = this;
        itemsInSlot.Add(invItem);

        invItem.connectedObject.transform.parent = physicalLocation;
        invItem.connectedObject.transform.position = physicalLocation.position;
        invItem.connectedObject.transform.localPosition = Vector3.zero;
        invItem.connectedObject.transform.localRotation = physicalLocation.localRotation;

        invItem.connectedObject.rb.isKinematic = true;
        BPUtil.SetAllColliders(invItem.connectedObject.transform, false);

        //itemFromThisSlot.connectedObject.SetMetaData("INV_SLOT", invItem.slotIn.name);
        //GameSettings.Instance.Player.GetComponent<InventoryMenuSystem>().SetSaveData(name, invItem.connectedObject.GetWorldID());
        //call the callback in the interactionsystem to update the physicals
        GameSettings.Instance.Player.GetComponent<InteractionSystem>().OnInventoryItemAddedToSlot_CallBack(slotFrom, this);

    }
    public void AddItemToSlot(HoldableObject holdableObject)
    {
        
        //exit the method and dont do anything if the slot can't hold anymore items
        /*if (GetCurrentSlotWeight() + holdableObject.inventoryObjectData.inventoryWeight > weightAllowed)
            return;*/
        if (itemsInSlot.Count > 0)
            return;

        GameObject newInventoryItem = Instantiate(GameSettings.Instance.inventoryItemPrefab.gameObject, transform);

        
        //set itemSlotInfo
        newInventoryItem.GetComponent<InventoryItem>().SetDetails(holdableObject.inventoryObjectData, holdableObject, this);
        itemsInSlot.Add(newInventoryItem.GetComponent<InventoryItem>());

        holdableObject.rb.isKinematic = true;
        BPUtil.SetAllColliders(holdableObject.transform, false);

        holdableObject.transform.parent = physicalLocation;
        holdableObject.transform.position = physicalLocation.position;
        holdableObject.transform.localPosition = Vector3.zero;
        holdableObject.transform.localRotation = physicalLocation.localRotation;

        //holdableObject.SetMetaData("INV_SLOT", name);

        //GameSettings.Instance.Player.GetComponent<InventoryMenuSystem>().SetSaveData(name, holdableObject.GetWorldID());

        //call the callback in the interactionsystem to update the physicals
        GameSettings.Instance.Player.GetComponent<InteractionSystem>().OnInventoryItemAddedToSlot_CallBack(null, this);
    }

    public void OnDrop(PointerEventData data)
    {
        if (GameSettings.Instance.Player.GetComponent<InventorySystem>().currentItemSlected != null)
        {
            AddItemToSlot(GameSettings.Instance.Player.GetComponent<InventorySystem>().currentItemSlected);

            GameSettings.Instance.Player.GetComponent<InventorySystem>().currentItemSlected = null;

            //Debug.Log("Dropping To " + name);
        }
        
    }
}


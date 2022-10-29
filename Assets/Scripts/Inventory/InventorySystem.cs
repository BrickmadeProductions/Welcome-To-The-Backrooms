using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public struct SavedPlayerInventory
{
    ///inventory slot name, saveableItem world id
    public Dictionary<string, string> propsInInventory;
}

public class InventorySystem : GenericMenu
{
    public SavedPlayerInventory currentPlayerInventorySave;
    //inventorySystem

    //2 pockets to start with

    //you can find backpacks in relevant locations (10 extra slots)

    //each item has a weight that prevents you from placing a lot of large items in the backpack, certain items like chairs have to be broken down to fit in the backpack

    //find backpacks in crates in level 1

    //crafting system

    //each item has a specific type of way it is crafted, such as pouring bottles into eachother

    //rhand, lhand, head, chest, etc. Things not contained in a container object, stored on the inventory item
    public InventorySlot rHand;
    public InventorySlot lHand;
    public InventorySlot rPocket;
    public InventorySlot lPocket;
    public InventorySlot head;
    public InventorySlot chest;
    //location to put the backpack
    public InventorySlot backPack;


    public InventoryItem currentItemSlected;

    IEnumerator UpdateAllItemsStats()
    {

        while (true)
        {
            foreach (InventorySlot slot in GetAllInvSlots())
            {
                if (slot.itemsInSlot.Count > 0)
                    foreach (InventoryItem item in slot.itemsInSlot)
                    {
                        string[] statDataArray = new string[item.connectedObject.stats.Count];

                        int count = 0;
                        foreach (KeyValuePair<string, string> statData in item.connectedObject.stats)
                        {
                           
                            statDataArray[count] = statData.Key + ": " + statData.Value + "\n";

                            
                            count++;

                        }

                        item.statText.text = string.Concat(statDataArray).ToUpper();
                    }
                
            }
            
            yield return new WaitForSecondsRealtime(0.5f);
        }

    }
    public void LoadInData(PlayerSaveData data)
    {
        currentPlayerInventorySave = data.savedPlayerInventory;
       
    }
    public void PutSavedItemsInInventory()
    {
        //players inventory data has been loaded this runs after spawn chunks have generated, just add items to it
        foreach (InventorySlot slot in GetAllInvSlots())
        {
            if (currentPlayerInventorySave.propsInInventory.ContainsKey(slot.name))
            {
                //Debug.Log(currentPlayerInventorySave.propsInInventory[slot.name]);
                if ((HoldableObject)GameSettings.Instance.worldInstance.FindPropInWorldByKey(currentPlayerInventorySave.propsInInventory[slot.name]) != null)
                    slot.AddItemToSlot((HoldableObject)GameSettings.Instance.worldInstance.FindPropInWorldByKey(currentPlayerInventorySave.propsInInventory[slot.name]));
                else
                {
                    continue;
                }

                if (slot == rHand)
                {
                    GetComponent<PlayerController>().builder.layers[1].active = true;
                    if (GetComponent<PlayerController>().offHandIK.data.target != null)
                        GetComponent<PlayerController>().offHandIK.data.target = slot.itemsInSlot[0].connectedObject.offHandIKPoint;
                    GetComponent<PlayerController>().builder.Build();
                }
            }
        }
    }
    public override void Awake_Init()
    {
        currentPlayerInventorySave = new SavedPlayerInventory
        {
            propsInInventory = new Dictionary<string, string>()
        };

        StartCoroutine(UpdateAllItemsStats());
    } 

    bool CanItemGoInSlot(InventorySlot slot, HoldableObject objectToAdd)
    {
        if (slot.blackList.Count > 0)
            if (slot.blackList.Contains(objectToAdd.type))
            {
                return false;
            }
                

        if (slot.itemsInSlot.Count == 0 && slot.GetCurrentSlotWeight() + objectToAdd.inventoryObjectData.inventoryWeight <= slot.weightAllowed)
        {
            if (slot.whiteList.Count > 0)
            {
                if (slot.whiteList.Contains(objectToAdd.type))
                    return true;
            }
            else
            {
                return true;
            }
        }

        return false;
    }

    public InventorySlot GetNextAvailableInventorySlot(HoldableObject objectToAdd)
    {
        //rHand can only hold 1 item
        if (CanItemGoInSlot(rHand, objectToAdd))

            return rHand;

        //rHand can only hold 1 item
        if (CanItemGoInSlot(rPocket, objectToAdd))

            return rPocket;

        if (CanItemGoInSlot(lPocket, objectToAdd))

            return lPocket;

        if (CanItemGoInSlot(backPack, objectToAdd))

            return backPack;

        if (CanItemGoInSlot(head, objectToAdd))

            return head;

        if (GetBackPack() != null)

            foreach (InventorySlot slot in GetBackPack().storageSlots)
            {
                if(CanItemGoInSlot(slot, objectToAdd))
                {
                    return slot;
                }
                
            }
        
        GameSettings.Instance.GetComponent<NotificationSystem>().QueueNotification("THERE ARE NO SLOTS AVAILABLE FOR THIS ITEM");
        
        return null;
    }

    public List<InventorySlot> GetAllInvSlots()
    {
        List<InventorySlot> slots = new List<InventorySlot>() { rHand, lHand, rPocket, lPocket, head, chest, backPack };

        if (backPack.itemsInSlot.Count > 0)
        {
            if (backPack.itemsInSlot[0].GetType() == typeof(ContainerObject))
            slots.AddRange((ContainerObject)backPack.itemsInSlot[0].connectedObject);
        }

        return slots;

    }

    public ContainerObject GetBackPack()
    {
        if (backPack.itemsInSlot.Count > 0)
            if (backPack.itemsInSlot[0].GetType() == typeof(ContainerObject))
                return (ContainerObject)backPack.itemsInSlot[0].connectedObject;
            else return null;
        else return null;
    }

    public override void Update_ExtraInputs()
    {
        if (Input.GetButtonDown("SwapRPocketToHand"))
        {
            rHand.AddItemToSlot(rPocket.itemsInSlot[0]);
            rHand.itemsInSlot[0].transform.parent = rHand.itemsInSlot[0].slotIn.transform;
            rHand.itemsInSlot[0].transform.position = rHand.itemsInSlot[0].slotIn.transform.position;
        }
        if (Input.GetButtonDown("SwapLPocketToHand"))
        {
            rHand.AddItemToSlot(lPocket.itemsInSlot[0]);
            rHand.itemsInSlot[0].transform.parent = rHand.itemsInSlot[0].slotIn.transform;
            rHand.itemsInSlot[0].transform.position = rHand.itemsInSlot[0].slotIn.transform.position;
        }
    }

    public void SetSlotSaveData(string field, string value)
    {

        if (currentPlayerInventorySave.propsInInventory.ContainsKey(field))
        {

            currentPlayerInventorySave.propsInInventory[field] = value;
        }
        else
        {
            currentPlayerInventorySave.propsInInventory.Add(field, value);
        }


    }

    public void RemoveSlotSaveData(string field)
    {

        if (currentPlayerInventorySave.propsInInventory.ContainsKey(field))
        {

            currentPlayerInventorySave.propsInInventory.Remove(field);
        }


    }




    

}




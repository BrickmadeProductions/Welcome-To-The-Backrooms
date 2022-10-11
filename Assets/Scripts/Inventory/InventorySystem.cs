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

    public List<ContainerObject> containerObjectsHeld;

    //rhand, lhand, head, chest, etc. Things not contained in a container object, stored on the inventory item
    public InventorySlot rHand;
    /*public InventorySlot lHand;*/
    public InventorySlot rPocket;
    public InventorySlot lPocket;
    public InventorySlot head;
    public InventorySlot chest;

    public InventoryItem currentItemSlected;

    IEnumerator UpdateAllItemsMetaData()
    {
        while (true)
        {
            foreach (InventorySlot slot in GetAllInvSlots())
            {
                if (slot.itemsInSlot.Count > 0)
                    foreach (InventoryItem item in slot.itemsInSlot)
                    {
                        string[] metaDataTextArray = new string[item.connectedObject.saveableData.metaData.Count];

                        int count = 0;
                        foreach (KeyValuePair<string, string> metaData in item.connectedObject.saveableData.metaData)
                        {
                           
                            if (metaData.Key != "INV_SLOT")
                                metaDataTextArray[count] = metaData.Key + ": " + metaData.Value + "\n";

                            
                            count++;

                        }

                        item.metaDataText.text = string.Concat(metaDataTextArray).ToUpper();
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
                slot.AddItemToSlot((HoldableObject)GameSettings.Instance.worldInstance.FindPropInWorldByKey(GetComponent<InventorySystem>().currentPlayerInventorySave.propsInInventory[slot.name]));
            }
        }
    }
    public override void Awake_Init()
    {
        currentPlayerInventorySave = new SavedPlayerInventory
        {
            propsInInventory = new Dictionary<string, string>()
        };

        StartCoroutine(UpdateAllItemsMetaData());
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
    public InventorySlot GetNextAvailableInventorySlot(HoldableObject objectToAdd)
    {
        //rHand can only hold 1 item
        if (rHand.itemsInSlot.Count == 0)
            return rHand;

        if (rPocket.itemsInSlot.Count == 0)
            return rPocket;

        if (lPocket.itemsInSlot.Count == 0)
            return lPocket;

        return null;
    }

    public InventorySlot[] GetAllInvSlots()
    {
        return new InventorySlot[] { rHand, rPocket, lPocket, head, chest, };

    }


   



}




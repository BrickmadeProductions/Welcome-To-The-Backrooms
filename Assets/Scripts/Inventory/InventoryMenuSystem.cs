using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InventoryMenuSystem : GenericMenu
{
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

/*
    public List<InventorySlot> pocketHandLocations;

    public LayerMask InventorySlotLayerMask;

    public InventorySlot currentlySelectedSlot;*/

    public InventorySlot GetNextAvailableInventorySlot(HoldableObject objectToAdd)
    {
        //rHand can only hold 1 item
        if (rHand.itemsInSlot.Count == 0)
            return rHand;
        else if (rPocket.GetCurrentSlotWeight() + objectToAdd.inventoryObjectData.inventoryWeight < rPocket.weightAllowed)
            return rPocket;
        else if (lPocket.GetCurrentSlotWeight() + objectToAdd.inventoryObjectData.inventoryWeight < lPocket.weightAllowed)
            return rHand;

        return null;
    }

   



}




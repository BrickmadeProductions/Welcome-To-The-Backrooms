using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class ContainerObject : MonoBehaviour
{
    //dynamic ui inventory slot data
    public List<InventorySlot> storageSlots;

    //locations for holdable objects to be stored when inside of inventory
    public List<Transform> availablePhysicalStorageSlots;

    //weighting of objects inside
    public int maxWeight;

    void Awake()
    {
        storageSlots = new List<InventorySlot>();
    }

    public bool isAtMaxWeight()
    {
        int currentWeight = 0;
        foreach (InventorySlot slot in storageSlots)
        {
            currentWeight += slot.objectInSlot.inventoryWeight;
        }

        return currentWeight >= maxWeight;
    }

    public void AddItem(HoldableObject objectToAdd)
    {
       if (!isAtMaxWeight())
       {
            foreach (InventorySlot slot in storageSlots)
            {
                if (slot.objectInSlot == null)
                {
                    slot.SwitchObjectToThisSlot(objectToAdd);
                }
            }
        }
    }

}

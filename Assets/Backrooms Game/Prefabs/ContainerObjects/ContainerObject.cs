using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class ContainerObject : MonoBehaviour
{
    //dynamic ui inventory slot data
    public List<InventoryItem> storageSlots;

    //locations for holdable objects to be stored when inside of inventory
    public List<Transform> availablePhysicalStorageSlots;



}

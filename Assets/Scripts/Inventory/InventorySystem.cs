using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InventorySystem : MonoBehaviour
{
    //inventorySystem

    //2 pockets to start with

    //you can find backpacks in relevant locations (10 extra slots)

    //no concrete UI

    //each item has a weight that prevents you from placing a lot of large items in the backpack, certain items like chairs have to be broken down to fit in the backpack

    //find backpacks in crates in level 1

    //crafting system

    //each item has a specific type of way it is crafted, such as pouring bottles into eachother


    int inventoryLimit = 5;
    int currentSelectedInventorySlot = 0;
    List<HoldableObject> inventorySlots;
    HoldableObject currentlyLookingAt;
    public GameObject pickup;

    public Transform dropLocation;
    // Update is called once per frame

    private void Awake()
    {
        inventorySlots = new List<HoldableObject>();
    }
    void Update()
    {
       /* if (currentlyLookingAt != null)
        Debug.Log(currentlyLookingAt.name);
        Debug.DrawRay(transform.parent.GetChild(0).transform.position, transform.parent.GetChild(0).transform.forward * 3, new Color(14,23,132));*/

        RaycastHit[] hits = Physics.RaycastAll(new Ray(transform.parent.GetChild(0).transform.position, transform.parent.GetChild(0).transform.forward), 3f);

        currentlyLookingAt = null;

        foreach (RaycastHit hit in hits)
        {

            if (hit.collider.GetComponent<HoldableObject>() != null)
            {
                currentlyLookingAt = hit.collider.GetComponent<HoldableObject>();
                
                break;
               
            }

        }

        if (currentlyLookingAt == null)
        {
            pickup.gameObject.SetActive(false);
        }
       
        else
        {
            pickup.gameObject.SetActive(true);

            if (Input.GetButtonDown("Pickup") && inventorySlots.Count < inventoryLimit)
            {
                
                    pickupObject();
            }
        }

        //if (Input.GetButtonDown("Drop") && inventorySlots.Count > 0)
        if (Input.GetButtonDown("Drop") && inventorySlots.Count > 0)
        {

            dropObject();
        }
    }

    private void pickupObject()
    {

        currentlyLookingAt.gameObject.SetActive(false);
        
        inventorySlots.Add(currentlyLookingAt);

        currentlyLookingAt.transform.SetParent(gameObject.transform);

        Debug.Log("Added Object " + currentlyLookingAt.name);
        currentlyLookingAt = null;
        

    }

    private void dropObject()
    {
        inventorySlots[currentSelectedInventorySlot].gameObject.SetActive(true);
        inventorySlots[currentSelectedInventorySlot].gameObject.transform.parent = null;
        inventorySlots[currentSelectedInventorySlot].gameObject.transform.position = dropLocation.transform.position;
        inventorySlots.RemoveAt(currentSelectedInventorySlot);


    }
}

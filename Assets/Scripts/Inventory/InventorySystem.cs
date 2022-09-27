using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InventorySystem : MonoBehaviour
{
    //inventorySystem

    //2 pockets to start with

    //you can find backpacks in relevant locations (10 extra slots)

    //each item has a weight that prevents you from placing a lot of large items in the backpack, certain items like chairs have to be broken down to fit in the backpack

    //find backpacks in crates in level 1

    //crafting system

    //each item has a specific type of way it is crafted, such as pouring bottles into eachother

    public bool inventoryOpened = false;

    public List<ContainerObject> containerObjectsHeld;

    public GameObject inventory;

/*
    public List<InventorySlot> pocketHandLocations;

    public LayerMask InventorySlotLayerMask;

    public InventorySlot currentlySelectedSlot;*/

    public ContainerObject GetNextContainerObjectNotFull()
    {
        foreach (ContainerObject container in containerObjectsHeld)
        {
            if (!container.isAtMaxWeight())
            {
                return container;
            }

        }

        return null;
    }

    public void HandleSelecting()
    {
        if (inventoryOpened)
        {
           /* InventorySlot lastSelectedSlot = currentlySelectedSlot;

            RaycastHit hit;

            Ray ray = GameSettings.Instance.Player.GetComponent<PlayerController>().playerCamera.ScreenPointToRay(Input.mousePosition);

            if (Physics.Raycast(ray, out hit, 1f, InventorySlotLayerMask))
            {
                currentlySelectedSlot = hit.transform.gameObject.GetComponent<InventorySlot>();

                //slot contains items
                if (currentlySelectedSlot.objectInSlot != null)
                {
                    if (!currentlySelectedSlot.selected)
                    {
                        currentlySelectedSlot.objectInSlot.transform.localPosition = new Vector3(0.05f, 0, 0);
                        currentlySelectedSlot.selected = true;
                    }

                    if (Input.GetMouseButtonDown(0))
                    {
                        currentlySelectedSlot.SwitchHandAndSlot();
                        ToggleContainer("OpenPocket");

                    }
                }
                
                    

            }
            else if (currentlySelectedSlot != null)
            {
                if (currentlySelectedSlot.objectInSlot != null)
                {
                    currentlySelectedSlot.objectInSlot.transform.localPosition = Vector3.zero;
                    currentlySelectedSlot.selected = false;
                    currentlySelectedSlot = null;
                }
                
                
                
            }
            if (lastSelectedSlot != null)
            {
                if (lastSelectedSlot != currentlySelectedSlot)
                {
                    if (lastSelectedSlot.objectInSlot != null)
                    {
                        lastSelectedSlot.objectInSlot.transform.localPosition = Vector3.zero;
                        lastSelectedSlot.selected = false;
                    }
                }
            }*/
           


        }
       
        
    }

    //when you pick up an object, open inventory selection UI
    public void OpenInventory()
    {
        inventoryOpened = !inventoryOpened;

        //GetComponent<PlayerController>().bodyAnim.SetBool(type, inventoryOpened);

        if (!inventoryOpened)
        {
            GetComponent<PlayerController>().playerHealth.canMoveHead = true;
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
            inventory.SetActive(false);

        }

        else
        {
            GetComponent<PlayerController>().playerHealth.canMoveHead = false;
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
            inventory.SetActive(true);

        }

        /*switch (type)
        {
            case "OpenPocket":

                if (inventoryOpened)
                {
                    // int index = 0;


                    inventory.SetActive(true);
                    //move hand slot first

                    *//*//pockets
                    foreach (InventorySlot slot in containerObjectsHeld[0].storageSlots)
                    {
                        pocketHandLocations[index].SwitchOtherSlotToThisSlot(slot);
                        index++;
                    }

                    foreach (InventorySlot slot in containerObjectsHeld[1].storageSlots)
                    {
                        pocketHandLocations[index].SwitchOtherSlotToThisSlot(slot);
                        index++;
                    }*//*
                }
                else
                {
                    inventory.SetActive(false);
                    *//*int index = 0;
                    //pockets
                    foreach (InventorySlot slot in containerObjectsHeld[0].storageSlots)
                    {
                        slot.SwitchOtherSlotToThisSlot(pocketHandLocations[index]);
                        index++;
                    }

                    foreach (InventorySlot slot in containerObjectsHeld[1].storageSlots)
                    {
                        slot.SwitchOtherSlotToThisSlot(pocketHandLocations[index]);
                        index++;
                    }*//*
                }
               

                break;
        }*/
    }

    void Update()
    {
        ManageInventoryInput();
        HandleSelecting();
    }

    public void ManageInventoryInput()
    {
        if (!GameSettings.Instance.PauseMenuOpen)
        {

            if (Input.GetButtonDown("OpenInventory"))
            {
                OpenInventory();
            }
        }

            
    }

}




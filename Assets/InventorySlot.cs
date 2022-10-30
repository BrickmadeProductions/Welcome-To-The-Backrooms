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
    public List<OBJECT_TYPE> blackList;

    public Transform physicalLocation;

    public bool shouldDisableSlotMeshRenderers;
    public int GetCurrentSlotWeight()
    {
        int currentSlotWeight = 0;

        foreach (InventoryItem item in itemsInSlot)
        {
            currentSlotWeight += item.connectedObject.inventoryObjectData.inventoryWeight;
        }

        return currentSlotWeight;
    }
    public void RemoveItemFromSlot(InventoryItem invItem, bool destroyInvItemInInv)
    {
        if (shouldDisableSlotMeshRenderers)
        {

            BPUtil.SetMeshRenderers(itemsInSlot[0].connectedObject.transform, true);

        }

        itemsInSlot.Remove(invItem);

        if (destroyInvItemInInv)
            Destroy(invItem.gameObject);
    }
    public void RemoveItemFromSlot(HoldableObject objectRemove, bool destroyInvItemInInv)
    {
        if (shouldDisableSlotMeshRenderers)
        {

            BPUtil.SetMeshRenderers(itemsInSlot[0].connectedObject.transform, true);

        }

        foreach (InventoryItem invItem in itemsInSlot)
        {
            if (invItem.connectedObject == objectRemove)
            {
                itemsInSlot.Remove(invItem);

                

                if (destroyInvItemInInv)
                {
                    Destroy(invItem.gameObject);
                   
                }

                    
                break;
            }
        }

    }

    public void AddItemToSlot(InventoryItem invItem)
    {

        if (!GameSettings.Instance.Player.GetComponent<PlayerController>().hasGivenCraftingNotification)
        {
            GameSettings.Instance.GetComponent<NotificationSystem>().QueueNotification("YOU CAN CRAFT ITEMS TOGETHER BY DRAGGING THEM OVER EACHOTHER IN YOUR INVENTORY, ITEMS HAVE TOOLTIPS EXPLAINING WHAT THEY CAN MAKE!");
            GameSettings.Instance.Player.GetComponent<PlayerController>().hasGivenCraftingNotification = true;
        }

        //check if this item can fit
        if (GetCurrentSlotWeight() + invItem.connectedObject.inventoryObjectData.inventoryWeight > weightAllowed && itemsInSlot.Count == 0)
        {
            return;
        }
        //check swap senerio
        else if (itemsInSlot.Count > 0)
        {
            if (itemsInSlot[0].connectedObject.inventoryObjectData.inventoryWeight > invItem.slotIn.weightAllowed
                    || invItem.connectedObject.inventoryObjectData.inventoryWeight > weightAllowed)
            {
                return;
            }

            if (invItem.slotIn.whiteList.Count > 0)
                if (!invItem.slotIn.whiteList.Contains(itemsInSlot[0].connectedObject.type))
                    return;

            if (invItem.slotIn.blackList.Count > 0)
                if (invItem.slotIn.blackList.Contains(itemsInSlot[0].connectedObject.type))
                    return;
        }

            

        if (whiteList.Count > 0)
            if (!whiteList.Contains(invItem.connectedObject.type))
                return;

        if (blackList.Count > 0)
            if (blackList.Contains(invItem.connectedObject.type))
                return;

        InventorySlot slotFrom = invItem.slotIn;
        //exit the method and dont do anything if the slot can't hold anymore items


        switch (invItem.connectedObject.objectCategory)
        {
            case OBJECT_CATEGORY.CONTAINER:

                ((ContainerObject)invItem.connectedObject).ConnectUIToPlayer();
                ((ContainerObject)invItem.connectedObject).UIObject.SetActive(true);

                break;

            case OBJECT_CATEGORY.ARMOR:

                break;
        }

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
            itemFromThisSlot.connectedObject.transform.localRotation = Quaternion.identity;
            itemFromThisSlot.connectedObject.transform.localPosition = Vector3.zero;

            //itemFromThisSlot.connectedObject.rb.isKinematic = true;
            BPUtil.SetAllColliders(itemFromThisSlot.connectedObject.transform, false);

            //itemFromThisSlot.connectedObject.SetMetaData("INV_SLOT", itemFromThisSlot.slotIn.name);
            //GameSettings.Instance.Player.GetComponent<InventoryMenuSystem>().SetSaveData(itemFromThisSlot.slotIn.name, itemFromThisSlot.connectedObject.GetWorldID());

            itemFromThisSlot.transform.parent = itemFromThisSlot.slotIn.transform;
            itemFromThisSlot.transform.position = itemFromThisSlot.slotIn.transform.position;

            GameSettings.Instance.Player.GetComponent<InventorySystem>().currentItemSlected = null;
            itemFromThisSlot.canvasGroup.alpha = 1f;
            itemFromThisSlot.canvasGroup.blocksRaycasts = true;

            if (itemFromThisSlot.slotIn.shouldDisableSlotMeshRenderers)
            {

                BPUtil.SetMeshRenderers(itemFromThisSlot.slotIn.itemsInSlot[0].connectedObject.transform, false);

            }
            else
            {
                BPUtil.SetMeshRenderers(itemFromThisSlot.slotIn.itemsInSlot[0].connectedObject.transform, true);
            }

        }


        //set itemSlotInfo
        invItem.slotIn = this;
        itemsInSlot.Add(invItem);

        invItem.connectedObject.transform.parent = physicalLocation;
        invItem.connectedObject.transform.position = physicalLocation.position;
        invItem.connectedObject.transform.localPosition = Vector3.zero;
        invItem.connectedObject.transform.localRotation = Quaternion.identity;

        //invItem.connectedObject.rb.isKinematic = true;
        BPUtil.SetAllColliders(invItem.connectedObject.transform, false);

        //itemFromThisSlot.connectedObject.SetMetaData("INV_SLOT", invItem.slotIn.name);
        //GameSettings.Instance.Player.GetComponent<InventoryMenuSystem>().SetSaveData(name, invItem.connectedObject.GetWorldID());
        //call the callback in the interactionsystem to update the physicals
        GameSettings.Instance.Player.GetComponent<InteractionSystem>().OnInventoryItemAddedToSlot_CallBack(slotFrom, this);

        if (shouldDisableSlotMeshRenderers)
        {

            BPUtil.SetMeshRenderers(itemsInSlot[0].connectedObject.transform, false);

        }
        else
        {
            BPUtil.SetMeshRenderers(itemsInSlot[0].connectedObject.transform, true);
        }

    }
    public void AddItemToSlot(HoldableObject holdableObject)
    {
        if (holdableObject.GetComponent<Rigidbody>() != null)
        {
            Destroy(holdableObject.GetComponent<Rigidbody>());
            holdableObject.rb = null;
        }

        if (!GameSettings.Instance.Player.GetComponent<PlayerController>().hasGivenCraftingNotification)
        {
            GameSettings.Instance.GetComponent<NotificationSystem>().QueueNotification("YOU CAN CRAFT ITEMS TOGETHER BY DRAGGING THEM OVER EACHOTHER IN YOUR INVENTORY, CHECK ITEM TOOLTIPS!");
            GameSettings.Instance.Player.GetComponent<PlayerController>().hasGivenCraftingNotification = true;
        }

        if (GetCurrentSlotWeight() + holdableObject.inventoryObjectData.inventoryWeight > weightAllowed)
        {
            return;
        }

        if (blackList.Count > 0)
            if (blackList.Contains(holdableObject.type))
                return;

        if (itemsInSlot.Count > 0)
            return;

        switch (holdableObject.objectCategory)
        {
            case OBJECT_CATEGORY.CONTAINER:

                ((ContainerObject)holdableObject).ConnectUIToPlayer();
                ((ContainerObject)holdableObject).UIObject.SetActive(true);

                break;

            case OBJECT_CATEGORY.ARMOR:

                break;
        }


        GameObject newInventoryItem = Instantiate(GameSettings.Instance.inventoryItemPrefab.gameObject, transform);

        //set itemSlotInfo
        newInventoryItem.GetComponent<InventoryItem>().SetDetails(holdableObject.inventoryObjectData, holdableObject, this);
        itemsInSlot.Add(newInventoryItem.GetComponent<InventoryItem>());

        //holdableObject.rb.isKinematic = true;
        BPUtil.SetAllColliders(holdableObject.transform, false);

        holdableObject.transform.parent = physicalLocation;
        holdableObject.transform.position = physicalLocation.position;
        holdableObject.transform.localPosition = Vector3.zero;
        holdableObject.transform.localRotation = Quaternion.identity;

        //holdableObject.SetMetaData("INV_SLOT", name);

        //GameSettings.Instance.Player.GetComponent<InventoryMenuSystem>().SetSaveData(name, holdableObject.GetWorldID());

        //call the callback in the interactionsystem to update the physicals
        GameSettings.Instance.Player.GetComponent<InteractionSystem>().OnInventoryItemAddedToSlot_CallBack(null, this);
        
        if (shouldDisableSlotMeshRenderers)
        {

            BPUtil.SetMeshRenderers(itemsInSlot[0].connectedObject.transform, false);

        }
        else
        {
            BPUtil.SetMeshRenderers(itemsInSlot[0].connectedObject.transform, true);
        }

    }

    public void OnDrop(PointerEventData data)
    {

        StartCoroutine(DropAction());
    }

    IEnumerator DropAction()
    {
        if (GameSettings.Instance.Player.GetComponent<InventorySystem>().currentItemSlected != null && GameSettings.Instance.Player.GetComponent<InventorySystem>().currentItemSlected.slotIn != this)
        {
            switch (GameSettings.Instance.Player.GetComponent<InventorySystem>().currentItemSlected.connectedObject.objectCategory)
            {
                case OBJECT_CATEGORY.AMMO:

                    if (itemsInSlot.Count > 0)
                    {
                        if (itemsInSlot[0].connectedObject.objectCategory == OBJECT_CATEGORY.LOADABLE)
                        {
                            AmmoObject ammo = ((AmmoObject)GameSettings.Instance.Player.GetComponent<InventorySystem>().currentItemSlected.connectedObject);

                            int amountToLoad;

                            if (ammo.amountLeft + itemsInSlot[0].connectedObject.GetComponent<Loadable>().amountLoaded > itemsInSlot[0].connectedObject.GetComponent<Loadable>().maxLoad)
                            {
                                amountToLoad = itemsInSlot[0].connectedObject.GetComponent<Loadable>().maxLoad - itemsInSlot[0].connectedObject.GetComponent<Loadable>().amountLoaded;
                            }
                            else
                            {
                                amountToLoad = ammo.maxAmount;
                            }

                            itemsInSlot[0].connectedObject.GetComponent<Loadable>().LoadObject(amountToLoad, true);

                            if (ammo.shouldDestroyWhenLoaded)
                            {
                                string ammoToDestroy = GameSettings.Instance.Player.GetComponent<InventorySystem>().currentItemSlected.connectedObject.GetWorldID();
                                GameSettings.Instance.Player.GetComponent<InteractionSystem>().SetDrop(GameSettings.Instance.Player.GetComponent<InventorySystem>().currentItemSlected.slotIn);
                                GameSettings.Instance.worldInstance.RemoveProp(ammoToDestroy, true);
                                GameSettings.Instance.Player.GetComponent<InventorySystem>().canOpen = true;
                            }
                            else
                            {
                                ((AmmoObject)GameSettings.Instance.Player.GetComponent<InventorySystem>().currentItemSlected.connectedObject).RemoveAmount(amountToLoad);
                            }

                        }
                    }
                    else
                    {
                        AddItemToSlot(GameSettings.Instance.Player.GetComponent<InventorySystem>().currentItemSlected);

                    }


                    break;

                case OBJECT_CATEGORY.CRAFTING_MATERIAL:

                    if (itemsInSlot.Count > 0)
                    {
                        if (itemsInSlot[0].connectedObject.objectCategory == OBJECT_CATEGORY.WEAPON_BASE || itemsInSlot[0].connectedObject.objectCategory == OBJECT_CATEGORY.CRAFTING_MATERIAL)
                        {
                            bool hasCraftingPair = false;
                            List<CraftingPair> possiblePairs = new List<CraftingPair>();

                            foreach (CraftingPair pair in GameSettings.Instance.possibleCraftingPairs)
                            {
                                if (pair.objectsRequired[0] == GameSettings.Instance.Player.GetComponent<InventorySystem>().currentItemSlected.connectedObject.type &&
                                    pair.objectsRequired[1] == itemsInSlot[0].connectedObject.type)
                                {
                                    possiblePairs.Add(pair);

                                }
                            }

                            if (possiblePairs.Count > 0)
                            {
                                //just craft
                                if (possiblePairs.Count == 1)
                                {
                                    CraftItem(possiblePairs[0], GameSettings.Instance.Player.GetComponent<InventorySystem>().currentItemSlected, itemsInSlot[0]);
                                    
                                }
                                //give selections
                                else
                                {
                                    CraftingPrompt craftingPrompt = Instantiate(GameSettings.Instance.Player.GetComponent<InventorySystem>().promptPrefab.gameObject, GameSettings.Instance.Player.GetComponent<InventorySystem>().menuObject.transform).GetComponent<CraftingPrompt>();
                                    craftingPrompt.SetDetails(possiblePairs, this);

                                    yield return new WaitUntil(() => !GameSettings.Instance.Player.GetComponent<InventorySystem>().isCrafting);

                                    Destroy(craftingPrompt.gameObject);

                                    
                                }
                                hasCraftingPair = true;
                                GameSettings.Instance.Player.GetComponent<InventorySystem>().canOpen = true;

                            }
                           

                            if (!hasCraftingPair)
                                AddItemToSlot(GameSettings.Instance.Player.GetComponent<InventorySystem>().currentItemSlected);
                        }
                        else
                        {
                            AddItemToSlot(GameSettings.Instance.Player.GetComponent<InventorySystem>().currentItemSlected);
                            break;
                        }
                    }
                    else
                    {
                        AddItemToSlot(GameSettings.Instance.Player.GetComponent<InventorySystem>().currentItemSlected);
                        break;
                    }
                    break;

                default:
                    AddItemToSlot(GameSettings.Instance.Player.GetComponent<InventorySystem>().currentItemSlected);
                    break;
            }

            GameSettings.Instance.Player.GetComponent<InventorySystem>().currentItemSlected = null;

        }

        yield return null;
    }


    //1 = item put on top, 2 = item in slot currently | 1 on top of 2
    public void CraftItem(CraftingPair pair, InventoryItem object1, InventoryItem object2)
    {

        InteractionSystem interactionSystem = GameSettings.Instance.Player.GetComponent<InteractionSystem>();

        GameSettings.Instance.Player.GetComponent<InventorySystem>().audioObject.clip = pair.craftingAudio;
        GameSettings.Instance.Player.GetComponent<InventorySystem>().audioObject.Play();

        //weapons
        if (object2.connectedObject.GetType() == typeof(CraftedWeapon) && object1.connectedObject.GetComponent<WeaponPiece>() != null)
        {
            if (((CraftedWeapon)object2.connectedObject).AddPiece(object1.connectedObject.GetComponent<WeaponPiece>(), object2))
            {
                if (this != interactionSystem.GetComponent<InventorySystem>().rHand)
                {
                    interactionSystem.GetComponent<PlayerController>().bodyAnim.SetBool((object1.slotIn.itemsInSlot[0].connectedObject.CustomHoldAnimation != "") ? object1.slotIn.itemsInSlot[0].connectedObject.CustomHoldAnimation : "isHoldingSmall", value: false);
                    interactionSystem.GetComponent<PlayerController>().bodyAnim.SetBool("isHoldingLarge", value: false);

                    interactionSystem.GetComponent<PlayerController>().offHandIK.data.target = null;
                    interactionSystem.GetComponent<PlayerController>().builder.layers[1].active = false;
                    interactionSystem.GetComponent<PlayerController>().builder.Build();
                }

                object1.slotIn.RemoveItemFromSlot(object1.slotIn.itemsInSlot[0], true);


            }
        }


        //regular item crafting
        else if (object1.connectedObject.objectCategory != OBJECT_CATEGORY.WEAPON_BASE && object2.connectedObject.objectCategory != OBJECT_CATEGORY.WEAPON_BASE)
        {
            if (pair.shouldDestroyItem1)
            {
                interactionSystem.SetDrop(object1.connectedObject.type == pair.objectsRequired[0] ? object1.slotIn : object2.slotIn);
                GameSettings.Instance.worldInstance.RemoveProp(object1.connectedObject.type == pair.objectsRequired[0] ? object1.connectedObject.GetWorldID() : object2.connectedObject.GetWorldID(), true);
            }
            else if (pair.shouldDestroyItem2)
            {
                interactionSystem.SetDrop(object1.connectedObject.type == pair.objectsRequired[1] ? object1.slotIn : object2.slotIn);
                GameSettings.Instance.worldInstance.RemoveProp(object1.connectedObject.type == pair.objectsRequired[1] ? object1.connectedObject.GetWorldID() : object2.connectedObject.GetWorldID(), true);
            }


            InteractableObject newProp = GameSettings.Instance.worldInstance.AddNewProp(GameSettings.Instance.Player.GetComponent<PlayerController>().RHandLocation.transform.position, Quaternion.identity, GameSettings.Instance.PropDatabase[pair.outCome].gameObject);

            interactionSystem.FinalizePickup(newProp, false);

            interactionSystem.canCraft = true;

            GameSettings.Instance.Player.GetComponent<PlayerController>().builder.layers[0].rig.weight = 1f;
            GameSettings.Instance.Player.GetComponent<PlayerController>().builder.Build();

            if (this != interactionSystem.GetComponent<InventorySystem>().rHand && pair.shouldDestroyItem1)
            {
                interactionSystem.GetComponent<PlayerController>().bodyAnim.SetBool((object1.slotIn.itemsInSlot[0].connectedObject.CustomHoldAnimation != "") ? object1.slotIn.itemsInSlot[0].connectedObject.CustomHoldAnimation : "isHoldingSmall", value: false);
                interactionSystem.GetComponent<PlayerController>().bodyAnim.SetBool("isHoldingLarge", value: false);

                interactionSystem.GetComponent<PlayerController>().offHandIK.data.target = null;
                interactionSystem.GetComponent<PlayerController>().builder.layers[1].active = false;
                interactionSystem.GetComponent<PlayerController>().builder.Build();
            }
        }



        GameSettings.Instance.Player.GetComponent<InventorySystem>().isCrafting = false;

    }

}


using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Animations.Rigging;

public class PlayerAnimationEventController : MonoBehaviour
{
    public PlayerController player;

    private void Awake()
    {
        GetComponent<BoneRenderer>().drawBones = false;
    }
    public void ThrowHoldable_AnimEvent()
    {
        player.GetComponent<InteractionSystem>().SetThrow();
    }

    public void DrinkFinished_AnimEvent()
    {
        player.GetComponent<InventorySystem>().rHand.itemsInSlot[0].connectedObject.GetComponent<ConsumableObject>().ConsumeOne();
    }
    public void PutInMainHand_AnimEvent()
    {
        player.GetComponent<InteractionSystem>().canGrab = true;
    }
    public void PutInNextAvailableContainer_AnimEvent()
    {
       /* InventorySlot nextAvailableSlotInContainers = player.GetComponent<InventorySystem>().GetNextContainerObjectNotFull().GetNextAvailableSlot();

        nextAvailableSlotInContainers.SwitchObjectToThisSlot(player.LHolding);

        player.LHolding = null;

        player.GetComponent<InteractionSystem>().canGrab = true;*/



    }

    public void DoneCrafting_AnimEvent()
    {
        /*InventorySystem inventory = player.GetComponent<InventorySystem>();
        InteractionSystem interactionSystem = player.GetComponent<InteractionSystem>();

        foreach (CraftingPair pair in GameSettings.Instance.possibleCraftingPairs)

            if (interactionSystem.GetObjectInLeftHand().type == pair.leftHand && interactionSystem.GetObjectInRightHand().type == pair.rightHand)
            {
                string IDObjectToDestroyR = inventory.rHand.itemsInSlot[0].connectedObject.GetWorldID();

                interactionSystem.SetDrop(inventory.rHand);

                GameSettings.Instance.worldInstance.RemoveProp(IDObjectToDestroyR, true);

                InteractableObject newProp = GameSettings.Instance.worldInstance.AddNewProp(player.RHandLocation.transform.position, Quaternion.identity, GameSettings.Instance.PropDatabase[pair.rightHandOutcome].gameObject);

                interactionSystem.FinalizePickup(newProp, false);


                if (pair.shouldDestroyLeftHandItem)
                {
                    string IDObjectToDestroyL = inventory.lHand.itemsInSlot[0].connectedObject.GetWorldID();

                    inventory.lHand.RemoveItemFromSlot(inventory.lHand.itemsInSlot[0], true);

                    GameSettings.Instance.worldInstance.RemoveProp(IDObjectToDestroyL, true);
                }

                interactionSystem.canCraft = true;
                break;
            }

        player.builder.layers[0].rig.weight = 1f;
        player.builder.Build();*/
    }


}

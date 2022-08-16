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
}

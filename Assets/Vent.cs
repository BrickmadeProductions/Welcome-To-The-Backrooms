using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Vent : InteractableObject
{
    public bool hasBeenOpened = false;

    public override void Init()
    {
        SetMetaData("hasBeenOpened", "false");
    }

    public override void OnLoadFinished()
    {
        if (GetMetaData("hasBeenOpened") == "true")
        {
            GetComponent<Rigidbody>().constraints = RigidbodyConstraints.None;
        }
    }

    public override void OnSaveFinished()
    {
        
    }

    public override void Pickup(InteractionSystem player, bool RightHand)
    {
        
    }

    public override void Use(InteractionSystem player, bool LMB)
    {
        if (player.GetObjectInRightHand() != null)
            if (player.GetObjectInRightHand().connectedInvItem.connectedObject.type == OBJECT_TYPE.SCREWDRIVER)
            {
                GetComponent<Rigidbody>().constraints = RigidbodyConstraints.None;
                SetMetaData("hasBeenOpened", "true");
            }
    }

}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InventorySlot : MonoBehaviour
{
    public HoldableObject objectInSlot;

    public void SwitchObjectToThisSlot(HoldableObject holdable)
    {
        objectInSlot = holdable;

        objectInSlot.transform.parent = transform;
        objectInSlot.transform.position = transform.position;

        /*objectInSlot.transform.localScale *= 0.6f;*/

        objectInSlot.transform.localRotation = Quaternion.identity;

        objectInSlot.holdableObject.isKinematic = true;

    }

    public void SwitchHandAndSlot()
    {
        HoldableObject heldItem = GameSettings.Instance.Player.GetComponent<PlayerController>().RHolding;
        HoldableObject objectInThisSlot = objectInSlot;

        if (heldItem != null)
        {
            heldItem.StartCoroutine(heldItem.playItemAnimation("Close"));

            SwitchObjectToThisSlot(heldItem);
        }

        if (objectInThisSlot != null)
        {
            GameSettings.Instance.Player.GetComponent<PlayerController>().RHolding = null;
            GameSettings.Instance.Player.GetComponent<InteractionSystem>().SetHolding(objectInThisSlot);
        }
        

       


        

       /* GameSettings.Instance.Player.GetComponent<PlayerController>().RHolding.transform.localScale /= 0.6f;*/

        //sometimes the hand might have dropped something
        
        

    }


}

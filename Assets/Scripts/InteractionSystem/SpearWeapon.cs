using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpearWeapon : HoldableObject
{
    

    public override void Throw(Vector3 force)
    {
        base.Throw(force);
        GetComponent<RotateAtVelocity>().Flying = true;
        holdableObject.isKinematic = false;
        Debug.Log("Gran And Set To False");
        
    }

    public override void Hold(InteractionSystem player)
    {
        base.Hold(player);
        GetComponent<RotateAtVelocity>().Flying = false;
        holdableObject.isKinematic = true;
    }

}

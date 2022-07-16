using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpearWeapon : HoldableObject
{
    public bool Flying;
    public LayerMask tipLayerMask;

    public void OnCollisionEnter(Collision hit)
    {
        if (hit.gameObject.layer != 11)
        {
            Flying = false;
            holdableObject.isKinematic = true;
        }
    }

    public void OnTriggerEnter(Collider hit)
    {
        if (hit.gameObject.layer == tipLayerMask)
        {
            Flying = false;
            holdableObject.isKinematic = true;
        }
    }

    void LateUpdate()
    {
        if (Flying)
        {
            transform.right = -holdableObject.velocity.normalized;
        }
    }

    public override void Throw(Vector3 force)
    {
        base.Throw(force);
        Flying = true;
        holdableObject.isKinematic = false;
        Debug.Log("Gran And Set To False");
        
    }

    public override void Hold(InteractionSystem player)
    {
        base.Hold(player);
        Flying = false;
        holdableObject.isKinematic = true;
    }

}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ThrowWeapon : HoldableObject
{
    public bool Flying;
    public LayerMask tipLayerMask;
    public float rotationAmount;
    public bool stuckInWall;

    public void OnTriggerEnter(Collider hit)
    {
        if (hit.gameObject.layer != 11)
        {
            stuckInWall = true;
        }
    }

    public void OnTriggerStay(Collider hit)
    {
        if (hit.gameObject.layer != 11 && stuckInWall)
        {
            Flying = false;
            holdableObject.isKinematic = true;
        }
    }
    public void OnTriggerExit(Collider hit)
    { 
        if (transform.parent == null)
        {
            holdableObject.isKinematic = false;
            stuckInWall = false;
        }
        

    }

    void LateUpdate()
    {
        if (Flying)
        {
          /* transform.right =
            Vector3.Slerp(-transform.right, holdableObject.velocity.normalized, Time.deltaTime * 15);*/
        }
    }

    public override void Throw(Vector3 force)
    {
        base.Throw(force);
        holdableObject.AddForceAtPosition(force * rotationAmount, transform.up * 2);
        Flying = true;
        holdableObject.isKinematic = false;
        //Debug.Log("Gran And Set To False");
        
    }

    public override void Hold(InteractionSystem player)
    {
        base.Hold(player);
        Flying = false;
        holdableObject.isKinematic = true;
    }

}

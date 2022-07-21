using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ThrowWeapon : HoldableObject
{
    public bool Flying;
    public LayerMask tipLayerMask;
    public float rotationAmount;
    public bool stuckInWall;
    public AudioClip[] stuckSounds;


    public void OnTriggerEnter(Collider hit)
    {
        
        if (hit.gameObject.layer != 13 && hit.gameObject.layer != 11 && hit.gameObject.layer != 8 && hit.gameObject.layer != 6 && !stuckInWall && Flying)
        {
            //Debug.Log(hit.name);
            AudioSource.PlayClipAtPoint(stuckSounds[Random.Range(0, stuckSounds.Length)], hit.transform.position);

            Flying = false;

            holdableObject.constraints = RigidbodyConstraints.FreezeAll;

            stuckInWall = true;
        }
    }

/*    public void OnTriggerStay(Collider hit)
    {
        if (hit.gameObject.layer != 11 && hit.gameObject.layer != 8 && hit.gameObject.layer != 6 && stuckInWall)
        {
            
        }
    }*/
    public void OnTriggerExit(Collider hit)
    { 
        
        holdableObject.constraints = RigidbodyConstraints.None;
        stuckInWall = false;
        
        

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
        holdableObject.angularVelocity = holdableObject.transform.right * rotationAmount;
        //holdableObject.AddForceAtPosition(, transform.up * 2);
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

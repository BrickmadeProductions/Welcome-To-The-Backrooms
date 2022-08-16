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
    FixedJoint ObjectConnectionJoint;


    public void OnTriggerEnter(Collider hit)
    {
        if (hit.gameObject.layer != 13 && hit.gameObject.layer != 11 && hit.gameObject.layer != 8 && hit.gameObject.layer != 6 && !stuckInWall && Flying)
        {
            AudioSource.PlayClipAtPoint(stuckSounds[Random.Range(0, stuckSounds.Length)], hit.transform.position);

            Flying = false;

            if (hit.transform.parent != null)
            {
                if (hit.transform.parent.gameObject.GetComponent<Rigidbody>() != null)
                {
                    if (ObjectConnectionJoint == null)
                    {
                        ObjectConnectionJoint = gameObject.AddComponent<FixedJoint>();
                        // sets joint position to point of contact
                        ObjectConnectionJoint.anchor = GetComponentInChildren<Weapon>().GetComponent<Collider>().ClosestPoint(transform.position);
                        // conects the joint to the other object

                        ObjectConnectionJoint.connectedBody = hit.transform.parent.gameObject.GetComponent<Rigidbody>();

                        // Stops objects from continuing to collide and creating more joints
                        ObjectConnectionJoint.enableCollision = false;
                    }

                }
                else
                {
                    holdableObject.constraints = RigidbodyConstraints.FreezeAll;
                }
            }
           

            stuckInWall = true;
        }


        
        
    }

    /*public void OnTriggerStay(Collider hit)
    {
        if (hit.gameObject.layer != 11 && hit.gameObject.layer != 8 && hit.gameObject.layer != 6 && stuckInWall)
        {
            
        }
    }*/
    public void OnTriggerExit(Collider hit)
    {
        if (ObjectConnectionJoint != null)
        {
            Destroy(ObjectConnectionJoint);
            ObjectConnectionJoint = null;
        }

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

    public override void Use(InteractionSystem player, bool LMB)
    {
        base.Use(player, LMB);
    }

    public override void Throw(Vector3 force)
    {
        holdableObject.constraints = RigidbodyConstraints.None;
        base.Throw(force);
        holdableObject.angularVelocity = holdableObject.transform.right * rotationAmount * rotationAmount;
        Flying = true;


    }

    public override void Hold(InteractionSystem player, bool RightHand)
    {
        base.Hold(player, RightHand);
        Flying = false;
        holdableObject.constraints = RigidbodyConstraints.FreezeAll;
    }

}

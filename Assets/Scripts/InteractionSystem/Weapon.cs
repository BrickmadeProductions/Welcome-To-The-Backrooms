using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Weapon : MonoBehaviour
{
    public float damage;
    public HoldableObject connetedObject;

    public void OnTriggerEnter(Collider other)
    {
        /*
         * Stabable Layer - 18
         */
        Vector3 collisionPoint = other.gameObject.GetComponent<Collider>().ClosestPointOnBounds(transform.position);

        if (gameObject.layer == 13 && other.gameObject.layer == 18 && connetedObject.animationPlaying) //13 is the layer the player holds items in their hand
        {
            AttackableEntityLimb limb = other.GetComponent<AttackableEntityLimb>();

            if (limb != null)
            {
                if (!limb.attachedEntity.stunned)
                {
                    StartCoroutine(limb.attachedEntity.StunTimer());
                }
                limb.attachedEntity.health -= (damage * limb.damageMultiplier);
                limb.Stabbed(collisionPoint);
            }
                
            
            
        }



        }

    public void OnTriggerExit(Collider other)
    {
       //Debug.Log("TriggerExit");
    }

}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Weapon : MonoBehaviour
{
    public float damage;
    public bool Thrown;
    public HoldableObject connetedObject;
    public Renderer WeaponBloodRenderer;
    public float bloodAmount = 0;

    public void OnTriggerEnter(Collider other)
    {
        /*
         * Stabable Layer - 18
         */
        Vector3 collisionPoint = other.gameObject.GetComponent<Collider>().ClosestPointOnBounds(transform.position);

        //melee attacks
        if (gameObject.layer == 13 && other.gameObject.layer == 18 && connetedObject.animationPlaying)
        {
            AttackableEntityLimb limb = other.GetComponent<AttackableEntityLimb>();

            if (limb != null)
            {
                Entity entityHit = limb.attachedEntity;

                if (!entityHit.stunned && Random.Range(0f, 1f) < 0.3f)
                {
                    StartCoroutine(entityHit.StunTimer());
                }

                entityHit.health -= (damage * limb.damageMultiplier);

                limb.Stabbed(collisionPoint);

                if (bloodAmount < 1)
                {
                    bloodAmount += 0.04f;
                    WeaponBloodRenderer.material.SetFloat("_Wetness", bloodAmount);
                }
                
            }
                
            
            
        }
        //thrown attacks
        else if (gameObject.layer == 9 && other.gameObject.layer == 18 && Thrown)
        {
            //Debug.Log("Player Attack");
            AttackableEntityLimb limb = other.GetComponent<AttackableEntityLimb>();
            if (limb != null)
            {
                Entity entityHit = limb.attachedEntity;

                if (!entityHit.stunned && Random.Range(0f, 1f) < 0.6f)
                {
                    StartCoroutine(entityHit.StunTimer());
                }

                //add more for thrown attacks
                entityHit.health -= (damage * limb.damageMultiplier * 1.5f);
                limb.Stabbed(collisionPoint);

                if (bloodAmount < 1)
                {
                    bloodAmount += 0.04f;
                    WeaponBloodRenderer.material.SetFloat("_Wetness", bloodAmount);
                }
            }
        }


    }

    public void OnTriggerExit(Collider other)
    {
       //Debug.Log("TriggerExit");
    }

}

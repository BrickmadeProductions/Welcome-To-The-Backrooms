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

<<<<<<< Updated upstream

        if (gameObject.layer == 13 && other.gameObject.layer == 18 && connetedObject.animationPlaying)
=======
        Debug.Log(connetedObject.animationPlaying);
        if (gameObject.layer == 23 && other.gameObject.layer == 18 && other.gameObject.layer != 11 && connetedObject.animationPlaying)
>>>>>>> Stashed changes
        {
            AttackableEntityLimb limb = other.GetComponent<AttackableEntityLimb>();
            Debug.Log("Player Attackedss");
            if (limb != null)
            {
                if (!limb.attachedEntity.stunned && Random.Range(0f, 1f) < 0.3f)
                {
                    StartCoroutine(limb.attachedEntity.StunTimer());
                }
                limb.attachedEntity.health -= (damage * limb.damageMultiplier);
                limb.Stabbed(collisionPoint);
                if (bloodAmount < 1)
                {
                    bloodAmount += 0.04f;
                    WeaponBloodRenderer.material.SetFloat("_Wetness", bloodAmount);
                }
                
            }
                
            
            
        }
        else if (other.gameObject.layer == 18 && Thrown)
        {
<<<<<<< Updated upstream
            //Debug.Log("Player Attack");
            AttackableEntityLimb limb = other.GetComponent<AttackableEntityLimb>();
            if (limb != null)
            {
                if (!limb.attachedEntity.stunned && Random.Range(0f, 1f) < 0.3f)
=======
            Debug.Log("Player Attack Throw");
            if (other.gameObject.layer == 18 && other.gameObject.layer != 11 && (connetedObject.GetComponent<ThrowWeapon>().Flying || connetedObject.GetComponent<ThrowWeapon>().stuckInWall))
            {
                Debug.Log("Player Attack");
                AttackableEntityLimb limb = other.GetComponent<AttackableEntityLimb>();
                if (limb != null)
>>>>>>> Stashed changes
                {
                    StartCoroutine(limb.attachedEntity.StunTimer());
                }
                limb.attachedEntity.health -= (damage * limb.damageMultiplier);
                limb.Stabbed(collisionPoint);
                if (bloodAmount < 1)
                {
                    bloodAmount += 0.04f;
                    WeaponBloodRenderer.material.SetFloat("_Wetness", bloodAmount);
                }
            }
        }
<<<<<<< Updated upstream


=======
>>>>>>> Stashed changes
    }

    public void OnTriggerExit(Collider other)
    {
       //Debug.Log("TriggerExit");
    }

}

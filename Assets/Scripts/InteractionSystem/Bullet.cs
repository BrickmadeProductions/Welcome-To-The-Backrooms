using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bullet : MonoBehaviour
{
    public float damage;
    public GameObject Self;
    public HoldableObject connetedObject;
    public Renderer WeaponBloodRenderer;
    public float BulletForceMultiplier;
    public int PassLimit;
    public float PassSpeedMultiplier;
    public float bloodAmount = 0;

    private int Passes;

    public void OnTriggerEnter(Collider other)
    {
        /*
         * Stabable Layer - 18
         */
        Vector3 collisionPoint = other.gameObject.GetComponent<Collider>().ClosestPointOnBounds(transform.position);

        Debug.Log(connetedObject.animationPlaying);
        if (other.gameObject.layer == 18 && other.gameObject.layer != 11)
        {
            Debug.Log("Player Attacked");
            AttackableEntityLimb limb = other.GetComponent<AttackableEntityLimb>();
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

        if (other.gameObject.GetComponentInParent<Rigidbody>())
        {
            other.gameObject.GetComponentInParent<Rigidbody>().AddForceAtPosition(transform.forward * (Self.GetComponent<Rigidbody>().velocity.magnitude  * BulletForceMultiplier), collisionPoint);
        }

        if (Passes >= PassLimit && other.gameObject.layer != 11)
        {
            Destroy(Self);
        }
        else if(other.gameObject.layer != 11)
        {
            Passes = Passes++;
            Self.GetComponent<Rigidbody>().AddForce(Self.GetComponent<Rigidbody>().velocity * PassSpeedMultiplier);

        }
    }
}

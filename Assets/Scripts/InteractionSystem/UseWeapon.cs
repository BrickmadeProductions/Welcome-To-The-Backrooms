using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UseWeapon : HoldableObject
{
    public LayerMask tipLayerMask;
    public AudioClip[] stuckSounds;
    public float ProjectileForce;
    public GameObject Projectile;
    public Transform Barrel;

    //TODO:
    //MAKE PROPER GUN ANIMS
    //
    public GameObject Container;

    public Transform HoldTransform;

    public Transform ADSTransform;

    public void Fire()
    {

        if (Barrel.GetComponent<Light>())
        {
            Barrel.GetComponent<Light>().enabled = true;
            Barrel.GetComponent<Light>().enabled = false;
        }

        GameObject newProjectile = Instantiate(Projectile);
        newProjectile.GetComponentInChildren<Bullet>().connetedObject = this;
        newProjectile.transform.position = Barrel.position;
        newProjectile.transform.rotation = Barrel.rotation;
        newProjectile.GetComponent<Rigidbody>().AddForce(Barrel.forward * ProjectileForce);
    }

    public void ADS()
    {
    }

    void Update()
    {
        if (Input.GetMouseButtonUp(1))
        {
            Container.transform.position = HoldTransform.position;
            Container.transform.rotation = HoldTransform.rotation;
        }
        if (Input.GetMouseButtonDown(1))
        {
            Container.transform.position = ADSTransform.position;
            Container.transform.rotation = ADSTransform.rotation;
        }
    }

    public override void Use(InteractionSystem player, bool LMB) //LMB
    {
        base.Use(player, LMB);

        if (LMB)
        {
            Debug.Log("LMB");
            Fire();
        }
        else
        {
            Debug.Log("RMB");
            ADS();
        }
    }
    public override void Throw(Vector3 force)
    {
        base.Throw(force);
    }

    public override void Hold(InteractionSystem player, bool RightHand)
    {
        base.Hold(player, RightHand);
        rb.constraints = RigidbodyConstraints.FreezeAll;
    }

}
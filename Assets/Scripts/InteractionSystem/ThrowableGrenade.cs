using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ThrowableGrenade : HoldableObject
{
    public float fuseTimeInSeconds;
    public Collider triggerRadius;

    public GameObject explosionParticles;

    public LayerMask explosionMask;

    bool hasExploded = false;
    bool hasPulledPin = false;

    public override void Use(InteractionSystem player, bool LMB)
    {
        

        if (!hasPulledPin)
            StartCoroutine(Fuse());

    }
    public override void Drop(Vector3 force)
    {
        base.Drop(force);

        rb.angularVelocity = new Vector3(Random.Range(-45, 45), Random.Range(-45, 45), Random.Range(-45, 45));
    }

    IEnumerator Fuse()
    {
        GetComponents<AudioSource>()[1].Play();

        hasPulledPin = true;

        yield return new WaitForSeconds(fuseTimeInSeconds);

        hasExploded = true;

        if (transform.GetComponentInParent<PlayerController>())
        {
            if (transform.GetComponentInParent<InventorySystem>().GetAllInvSlots().Contains(connectedInvItem.slotIn))
            {
                transform.GetComponentInParent<PlayerController>().playerHealth.TakeDamage(200f, 1f, 8f, true, DAMAGE_TYPE.PLAYER);
                transform.GetComponentInParent<InteractionSystem>().SetDrop(connectedInvItem.slotIn);

                
            }
                
        }
        

        triggerRadius.enabled = true;

        GameObject particles = Instantiate(explosionParticles);
        particles.transform.position = transform.position;

        GetComponents<AudioSource>()[0].Play();

        transform.GetChild(0).GetChild(0).gameObject.SetActive(false);
        transform.GetChild(0).GetChild(1).gameObject.SetActive(false);

        yield return new WaitForSeconds(0.25f);

        triggerRadius.enabled = false;

        yield return new WaitForSeconds(3f);


        Destroy(particles);

        GameSettings.Instance.worldInstance.RemoveProp(GetWorldID(), true);

    }

    private void OnTriggerEnter(Collider other)
    {
        if (hasExploded)
        {
            float distanceFromGrenade = Vector3.Distance(transform.position, other.transform.position);
            float damage = 200f / Mathf.Clamp((distanceFromGrenade), 1f, 200f);

            Rigidbody rb;

            if (other.GetComponentInParent<Rigidbody>())
            {
                rb = other.GetComponentInParent<Rigidbody>();
            }
            else if (other.GetComponentInChildren<Rigidbody>())
            {
                rb = other.GetComponentInChildren<Rigidbody>();
            }
            else if (other.GetComponent<Rigidbody>())
            {
                rb = other.GetComponent<Rigidbody>();
            }
            else return;

            /* RaycastHit[] rayHits = Physics.RaycastAll(transform.position, transform.position - rb.transform.position, explosionMask);
             Debug.Log(rayHits.Length);*/
            //no walls intersecting

            rb.AddExplosionForce(10000f, transform.position + Vector3.down * 2f, 10f);

            if (other.GetComponent<AttackableEntityLimb>())
            {
                other.GetComponent<AttackableEntityLimb>().Hit(other.transform.position, damage);
            }
            if (other.GetComponentInParent<HoldableObject>())
            {
                other.GetComponentInParent<HoldableObject>().TakeDamage(damage / 3f);
            }
            
        }

       

    }
}

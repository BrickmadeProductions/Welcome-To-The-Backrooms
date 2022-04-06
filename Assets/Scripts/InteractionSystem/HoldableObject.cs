using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HoldableObject : InteractableObject
{
    
    public int inventoryWeight = 1;
    public Rigidbody holdableObject;
    public AudioClip[] hitGroundClips;
    
    
    // Start is called before the first frame update
    void Awake()
    {
        holdableObject = GetComponent<Rigidbody>();
        StartCoroutine("waitToPlaySound");
    }

    IEnumerator waitToPlaySound()
    {
        yield return new WaitForSeconds(2);
        playSounds = true;
    }
    public override void Throw(Vector3 force)
    {
        holdableObject.AddForceAtPosition(force, transform.position);
    }

    public override void Use(InteractionSystem player)
    {

    }

    public override void Grab(InteractionSystem player)
    {
        player.currentlyLookingAt.gameObject.SetActive(false);

        player.inventorySlots.Add(this);

        transform.SetParent(player.currentlyLookingAt.gameObject.transform);

        Debug.Log("Added Object " + name);
        player.currentlyLookingAt = null;
    }

    Vector3 pushAmt; // global "pushed by RBs" accumulator

    private void OnCollisionEnter(Collision collision)
    {

        //Debug.Log(collision.relativeVelocity.magnitude);

        if (collision.gameObject.tag == "Player")
        {
            Rigidbody rb = collision.rigidbody;
            if (rb != null)
            {
                //pushAmt += rb.velocity * 1f;
            }
        }
        else
        {
            if (transform.gameObject.GetComponent<AudioSource>() != null && playSounds)
            {
                if (collision.relativeVelocity.magnitude >= 4) { 

                    if (!transform.gameObject.GetComponent<AudioSource>().isPlaying)
                    {
                        transform.gameObject.GetComponent<AudioSource>().clip = hitGroundClips[Random.Range(0, hitGroundClips.Length)];
                        transform.gameObject.GetComponent<AudioSource>().pitch = 1f + Random.Range(-0.15f, 0.15f);
                        transform.gameObject.GetComponent<AudioSource>().Play();

                        if (breakablePrefab != null)
                        {

                            if (collision.relativeVelocity.magnitude >= 5)
                                durability -= collision.relativeVelocity.magnitude;

                            if (durability < 0)
                            {

                                Instantiate(breakablePrefab, transform.position, transform.rotation);
                                transform.gameObject.SetActive(false);
                                Destroy(gameObject);
                            }


                        }



                    }
                }
            }               
        }
        
    }
    
    

    private void FixedUpdate()
    {
        transform.position += pushAmt;
        pushAmt *= 0.95f; // fake friction
    }

    
}

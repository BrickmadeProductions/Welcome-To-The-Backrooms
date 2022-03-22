using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HoldableObject : MonoBehaviour
{
    float durability = 40;
    public int inventoryWeight = 1;
    public Rigidbody holdableObject;
    public AudioClip[] hitGroundClips;

    public GameObject breakablePrefab;
    
    // Start is called before the first frame update
    void Awake()
    {
        holdableObject = GetComponent<Rigidbody>();
    }
    public void Throw(Vector3 force)
    {
        holdableObject.AddForceAtPosition(force, transform.position);
    }

    Vector3 pushAmt; // global "pushed by RBs" accumulator

    private void OnCollisionEnter(Collision collision)
    {
        Debug.Log(collision.relativeVelocity.magnitude);
        if (collision.gameObject.tag == "Player")
        {
            Rigidbody rb = collision.rigidbody;
            if (rb != null)
            {
                pushAmt += rb.velocity * 1f;
            }
        }
        else
        {
            if (transform.gameObject.GetComponent<AudioSource>() != null)
            {
                if (!transform.gameObject.GetComponent<AudioSource>().isPlaying)
                {

                    transform.gameObject.GetComponent<AudioSource>().clip = hitGroundClips[Random.Range(0, hitGroundClips.Length)];
                    transform.gameObject.GetComponent<AudioSource>().pitch = 1f + Random.Range(-0.1f, 0.1f);
                    transform.gameObject.GetComponent<AudioSource>().Play();

                    if (breakablePrefab != null)
                    {
                        if (collision.relativeVelocity.magnitude >= 5)
                            durability -= collision.relativeVelocity.magnitude;

                        if (durability <= 0)
                        {
                            Instantiate(breakablePrefab, transform.position, transform.rotation);
                            transform.gameObject.SetActive(false);
                        }
                       

                    }

                    
                }
            }               
        }
        
    }
    
    public virtual void Use()
    {

    }

    private void FixedUpdate()
    {
        transform.position += pushAmt;
        pushAmt *= 0.95f; // fake friction
    }

}

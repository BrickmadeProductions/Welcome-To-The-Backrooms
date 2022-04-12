using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class HoldableObject : InteractableObject
{
    
    public int inventoryWeight = 1;
    public Rigidbody holdableObject;
    bool broken;
    public bool large;
    public bool animationPlaying;

    public List<AnimationClip> animationClips;
    public List<string> animationBools;

    // Start is called before the first frame update
    void Awake()
    {
        broken = false;
        holdableObject = GetComponent<Rigidbody>();
        StartCoroutine(waitToPlaySound());
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
    IEnumerator playAnimation(string boolName, int animChosen)
    {
        GameSettings.Instance.Player.GetComponent<PlayerController>().bodyAnim.SetBool(boolName, true);
        animationPlaying = true;
        yield return new WaitForSeconds(animationClips[animChosen].length);
        animationPlaying = false;
        GameSettings.Instance.Player.GetComponent<PlayerController>().bodyAnim.SetBool(boolName, false);
    }
    public override void Use(InteractionSystem player)
    {

        if (!animationPlaying)
        {
            int animChosen = Random.Range(0, animationBools.Count);

            StartCoroutine(playAnimation(animationBools[animChosen], animChosen));

        }
        
            

        
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
                        transform.gameObject.GetComponent<AudioSource>().clip = hitClips[Random.Range(0, hitClips.Length)];
                        transform.gameObject.GetComponent<AudioSource>().pitch = 1f + Random.Range(-0.15f, 0.15f);
                        transform.gameObject.GetComponent<AudioSource>().Play();

                        if (breakablePrefabs.Length > 0)
                        {

                            if (collision.relativeVelocity.magnitude >= 5)
                                durability -= collision.relativeVelocity.magnitude;

                            //break
                            if (durability < 0 && !broken && breakClips.Length > 0)
                            {
                                AudioSource.PlayClipAtPoint(breakClips[Random.Range(0, breakClips.Length)], transform.position);

                                foreach (GameObject prefab in breakablePrefabs)
                                {
                                    GameObject p = Instantiate(prefab, transform.position, transform.rotation);
                                    p.GetComponent<Rigidbody>().velocity = GetComponent<Rigidbody>().velocity / 2; //friction
                                }
                                

                                Destroy(gameObject);
                                broken = true;
                                
                                
                            }


                        }



                    }
                }
            }               
        }
        
    }
    
    

    private void FixedUpdate() 
    {
        if (SceneManager.GetActiveScene().name != "HomeScreen")

            if (GameSettings.Instance.Player.GetComponent<PlayerController>().bodyAnim.GetCurrentAnimatorStateInfo(0).normalizedTime >= 1)
            {
                GameSettings.Instance.Player.GetComponent<PlayerController>().bodyAnim.SetBool("Stab1", false);
                GameSettings.Instance.Player.GetComponent<PlayerController>().bodyAnim.SetBool("Slice", false);
            }
        
        transform.position += pushAmt;
        pushAmt *= 0.95f; // fake friction
    }

    
}

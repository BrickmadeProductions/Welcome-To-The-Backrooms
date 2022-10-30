using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PartygoerAI : Entity
{
    // Angular speed in radians per sec.

    bool strangling = false;
    public GameObject grabLocation;
    public GameObject ragDoll;
    int currentWalkPhase = 1;

    [ColorUsage(true, true)]
    public Color emissionColor;
    public Material emission;
    public Renderer partyGoerRenderer;
    public override void OnEventStart()
    {
        emission.EnableKeyword("_EMISSION");
        emission.SetColor("_EmissionColor", emissionColor);
        
        
    }
    public override void OnEventEnd()
    {
        emission.DisableKeyword("_EMISSION");
        emission.SetColor("_EmissionColor", Color.black);
        
    }

    public override void Init()
    {
        emission = partyGoerRenderer.material;

        if (GameSettings.Instance.worldInstance.currentWorldEvent == GAMEPLAY_EVENT.LIGHTS_OUT)
        {
            emission.EnableKeyword("_EMISSION");
            emission.SetColor("_EmissionColor", emissionColor);
           
        }
        else
        {
            emission.DisableKeyword("_EMISSION");
            emission.SetColor("_EmissionColor", Color.black);
            
        }
        
    }

    public override IEnumerator AI()
    {
        while (true)
        {
            if (canAttack && GameSettings.Instance.GetComponent<CheatSheet>().AIEnabled)
            {
                if (currentTarget != null)
                {
                    Vector3 targetDirection = currentTarget.position - transform.position;

                    float singleStep = speed * Time.deltaTime;

                    Vector3 newDirection = Vector3.RotateTowards(transform.forward, targetDirection, singleStep, 0.0f);

                    int twitchNoise = Random.Range(0, movementNoises.Length);

                    //only twitch towards target if farther than 4 units

                
                    if (Vector3.Distance(currentTarget.position, transform.position) > 2 && !strangling && !stunned)
                    {
                        entityAnimator.SetBool("Attack", false);
                        entityAnimator.SetBool("Idle", false);

                        if (canSeeTarget)
                        {
                            //attack start
                            if (!attackNoiseSource.isPlaying)
                            {
                                attackNoiseSource.pitch = Random.Range(0.9f, 1.1f);
                                attackNoiseSource.Play();

                            }
                            //ambience stop
                            if (GetComponent<AudioSource>().isPlaying)
                                GetComponent<AudioSource>().Stop();

                            float step = speed * Time.deltaTime;
                            transform.position = Vector3.MoveTowards(transform.position, GameSettings.Instance.Player.transform.position + new Vector3(Random.Range(-2f, 2f), 0, Random.Range(-2f, 2f)), step);

                            //correct floating
                            RaycastHit[] hits;
                            float distance = 10f;

                            hits = Physics.RaycastAll(transform.position + new Vector3(0, 2f, 0), Vector3.down, distance, sightMask);

                            if (hits.Length > 0)
                            {
                                foreach (RaycastHit hit in hits)
                                {
                                    //only if floor
                                    if (hit.transform.gameObject.layer == 19)
                                    {
                                        transform.position = hit.point;
                                        continue;
                                    }
                                    
                                
                                }
                           
                            }
                        

                            transform.rotation = Quaternion.LookRotation(newDirection);

                            transform.rotation = new Quaternion(0, transform.rotation.y, 0, transform.rotation.w);
                            movementNoiseSource.clip = movementNoises[twitchNoise];
                            movementNoiseSource.pitch = Random.Range(0.9f, 1.1f);
                            movementNoiseSource.Play();

                            if (currentWalkPhase == 1)
                            {
                                entityAnimator.SetBool("Twitch" + 3, true);
                                currentWalkPhase = 3;
                            }
                            
                            else
                            {
                                entityAnimator.SetBool("Twitch" + 1, true);
                                currentWalkPhase = 1;
                            }

                            yield return new WaitForSecondsRealtime(Random.Range(0.15f, 0.5f));

                            //ambient
                            GetComponent<AudioSource>().pitch = Random.Range(0.5f, 1.2f);

                            if (entityAnimator.GetBool("Twitch" + 1))
                                entityAnimator.SetBool("Twitch" + 1, false);
                            else
                            {
                                entityAnimator.SetBool("Twitch" + 3, false);
                            }
                        }
                        else
                        {
                            //ambience
                            if (!GetComponent<AudioSource>().isPlaying)
                                GetComponent<AudioSource>().Play();

                            /*if (attackNoiseSource.isPlaying)
                                attackNoiseSource.Stop();*/

                            yield return new WaitForSecondsRealtime(1f);
                        }
                    }
                    //attack
                    else if (!strangling && !stunned)
                    {

                        if (GetComponent<AudioSource>().isPlaying)
                            GetComponent<AudioSource>().Stop();

                        //attack start
                        if (!attackNoiseSource.isPlaying)
                        {
                            attackNoiseSource.pitch = Random.Range(0.9f, 1.1f);
                            attackNoiseSource.Play();

                        }

                        strangling = true;

                        entityAnimator.SetBool("Twitch1", false);
                        entityAnimator.SetBool("Twitch2", false);
                        entityAnimator.SetBool("Twitch3", false);

                        entityAnimator.SetBool("Attack", true);
                    

                        yield return new WaitForSecondsRealtime(1f);


                    }

                    else if (strangling)
                    {

                        GameSettings.Instance.Player.GetComponent<PlayerController>().playerHealth.TakeDamage(damage, sanityMultiplier, 1f);

                        //damage per second
                        yield return new WaitForSecondsRealtime(0.4f);
                    }
                    else
                    {
                        strangling = false;
                        entityAnimator.SetBool("Attack", false);
                        entityAnimator.SetBool("Idle", true);

                        yield return new WaitForSecondsRealtime(3);
                    }

                    //ambient
                    GetComponent<AudioSource>().pitch = Random.Range(0.5f, 1.2f);

                }
                else
                {
                    yield return new WaitForSecondsRealtime(0.1f);
                }
            }
            else
            {
                yield return new WaitForSecondsRealtime(0.1f);
            }

        }
    }
    public override void Despawn()
    {
        GameSettings.Instance.Player.GetComponent<PlayerController>().bodyAnim.SetBool("Choking", false);
        GameSettings.Instance.Player.GetComponent<PlayerController>().playerHealth.canRun = true;
        GameSettings.Instance.Player.GetComponent<PlayerController>().playerHealth.canJump = true;
        GameSettings.Instance.Player.GetComponent<PlayerController>().playerHealth.canWalk = true;

        //Debug.Log("Despawned " + type.ToString() + "-" + runTimeID);
        GameObject ragDollInstance = Instantiate(ragDoll);

        ragDollInstance.transform.position = transform.position;
        ragDollInstance.transform.rotation = transform.rotation;

        GameSettings.Instance.worldInstance.RemoveEntity(type.ToString() + "-" + runTimeID);

    }

    public override void UpdateEntity()
    {

        if (stunned)
        {
            strangling = false;
        }

        if (!GameSettings.Instance.Player.GetComponent<PlayerController>().dead)
        {
            if (strangling)
            {
                GameSettings.Instance.Player.GetComponent<PlayerController>().currentPotentialDeathCase = DEATH_CASE.ENTITY;

                Vector3 targetDirection = GameSettings.Instance.Player.GetComponent<PlayerController>().head.transform.transform.position - eyes.transform.position;

                GameSettings.Instance.Player.GetComponent<PlayerController>().transform.position = grabLocation.transform.position - new Vector3(0f, 2.3f, 0f);
                GameSettings.Instance.Player.GetComponent<PlayerController>().playerHealth.canRun = false;
                GameSettings.Instance.Player.GetComponent<PlayerController>().playerHealth.canJump = false;
                GameSettings.Instance.Player.GetComponent<PlayerController>().playerHealth.canWalk = false;

                Vector3 attackDirection = Vector3.RotateTowards(transform.forward, targetDirection, 1f, 1f);
                eyes.transform.rotation = Quaternion.LookRotation(attackDirection);

                GameSettings.Instance.Player.GetComponent<PlayerController>().bodyAnim.SetBool("Choking", true);


            }
            else if (!strangling)
            {
                GameSettings.Instance.Player.GetComponent<PlayerController>().currentPotentialDeathCase = DEATH_CASE.UNKNOWN;

                GameSettings.Instance.Player.GetComponent<PlayerController>().playerHealth.canJump = true;
                GameSettings.Instance.Player.GetComponent<PlayerController>().playerHealth.canWalk = true;
                GameSettings.Instance.Player.GetComponent<PlayerController>().bodyAnim.SetBool("Choking", false);
            }
        }
       


    }
}

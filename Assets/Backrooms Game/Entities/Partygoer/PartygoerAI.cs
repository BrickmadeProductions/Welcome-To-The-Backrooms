using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PartygoerAI : Entity
{
    // Angular speed in radians per sec.

    bool strangling = false;
    public GameObject grabLocation;
    public GameObject ragDoll;

    bool AvoidObsticles()
    {
        RaycastHit hitData;
        if (Physics.Raycast(transform.position + new Vector3(0f, 5f, 0f), transform.TransformDirection(Vector3.forward), out hitData, 10f))
        {
            transform.rotation = Quaternion.Euler(transform.rotation.eulerAngles.x, transform.rotation.eulerAngles.y + 90f, transform.rotation.eulerAngles.z);
            AvoidObsticles();
        }
        else
        {
            return true;
        }
        return false;
    }

    public override IEnumerator AI()
    {
        while (true)
        {
            if (canAttack)
            {

                Vector3 targetDirection = GameSettings.Instance.Player.transform.transform.position - transform.position;

                float singleStep = speed * Time.deltaTime;

                Vector3 newDirection = Vector3.RotateTowards(transform.forward, targetDirection, singleStep, 0.0f);

                int twitchPhase = Random.Range(0, 3);
                int twitchNoise = Random.Range(0, movementNoises.Length);

                //only twitch towards player if farther than 4 units
                if (Vector3.Distance(GameSettings.Instance.Player.transform.position, transform.position) > 3 && !strangling && !stunned)
                {
                    if (canSeePlayer)
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

                        transform.position = new Vector3(transform.position.x, 0.1f, transform.position.z);

                        transform.rotation = Quaternion.LookRotation(newDirection);

                        transform.rotation = new Quaternion(0, transform.rotation.y, 0, transform.rotation.w);
                        movementNoiseSource.clip = movementNoises[twitchNoise];
                        movementNoiseSource.pitch = Random.Range(0.9f, 1.1f);
                        movementNoiseSource.Play();

                        entityAnimator.SetBool("Twitch" + twitchPhase, true);

                        yield return new WaitForSeconds(0.55f);

                        //ambient
                        GetComponent<AudioSource>().pitch = Random.Range(0.5f, 1.2f);

                        entityAnimator.SetBool("Twitch" + twitchPhase, false);
                    }
                    else
                    {
                        //ambience
                        if (!GetComponent<AudioSource>().isPlaying)
                            GetComponent<AudioSource>().Play();

                        if (attackNoiseSource.isPlaying)
                            attackNoiseSource.Stop();

                        yield return new WaitForSeconds(1f);
                    }
                }
                //attack
                else if (strangling == false && canSeePlayer)
                {

                    if (GetComponent<AudioSource>().isPlaying)
                        GetComponent<AudioSource>().Stop();

                    strangling = true;

                    entityAnimator.SetBool("Twitch0", false);
                    entityAnimator.SetBool("Twitch1", false);
                    entityAnimator.SetBool("Twitch2", false);

                    entityAnimator.SetBool("Attack", true);
                    

                    yield return new WaitForSeconds(1f);


                }
                else if (strangling == true)
                {


                    GameSettings.Instance.Player.GetComponent<PlayerController>().playerHealth.health -= damage;
                    GameSettings.Instance.Player.GetComponent<PlayerController>().playerHealth.sanity *= sanityMultiplier;

                    GameSettings.Instance.Player.GetComponent<PlayerController>().playerHealth.ChangeHeartRate(1.5f);

                    //damage per second
                    yield return new WaitForSeconds(1f);
                }
                else
                {
                    
                    yield return new WaitForSeconds(3f);
                }

                //ambient
                GetComponent<AudioSource>().pitch = Random.Range(0.5f, 1.2f);

                //entityAnimator.SetBool("Twitch" + twitchPhase, false);

            }
            else
            {
                yield return new WaitForSeconds(3f);
            }

        }
    }
    public override void Despawn()
    {
        GameSettings.Instance.Player.GetComponent<CharacterController>().enabled = true;
        GameSettings.Instance.Player.GetComponent<PlayerController>().bodyAnim.SetBool("Choking", false);
        GameSettings.Instance.Player.GetComponent<PlayerController>().grabbed = false;
        //GameSettings.Instance.Player.GetComponent<PlayerController>().rotationY = 0;
        Debug.Log("Despawned " + type);
        GameObject ragDollInstance = Instantiate(ragDoll);
        ragDollInstance.transform.position = transform.position;
        Destroy(gameObject);

    }

    public override void UpdateEntity()
    {
        //Debug.DrawRay(transform.position + new Vector3(0f, 5f, 0f), transform.TransformDirection(Vector3.forward) * 10f, Color.blue);

        if (strangling == true)
        {

            Vector3 targetDirection = GameSettings.Instance.Player.GetComponent<PlayerController>().head.transform.transform.position - eyes.transform.position;

            GameSettings.Instance.Player.GetComponent<PlayerController>().transform.position = grabLocation.transform.position - new Vector3(0.05f, 1.2f, 0.1f);
            GameSettings.Instance.Player.GetComponent<PlayerController>().grabbed = true;

            Vector3 attackDirection = Vector3.RotateTowards(transform.forward, targetDirection, 1f, 1f);
            eyes.transform.rotation = Quaternion.LookRotation(attackDirection);

            GameSettings.Instance.Player.GetComponent<PlayerController>().bodyAnim.SetBool("Choking", true);


        }


    }
}

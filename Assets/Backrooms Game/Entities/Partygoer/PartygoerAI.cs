using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PartygoerAI : Entity
{
    // Angular speed in radians per sec.
    public float speed;
    bool strangling = false;
    public GameObject grabLocation;

    public override void Despawn()
    {
        Destroy(this);
    }

    private void Update()
    {
        if (strangling == true)
        {
            
            Vector3 targetDirection = GameSettings.Instance.Player.transform.transform.position - transform.position;

            GameSettings.Instance.Player.GetComponent<PlayerController>().transform.position = grabLocation.transform.position - new Vector3(0.05f, 1.2f, 0.1f);
            GameSettings.Instance.Player.transform.rotation = Quaternion.LookRotation(Vector3.RotateTowards(GameSettings.Instance.Player.transform.forward, transform.position, 1f, 0.0f));
            GameSettings.Instance.Player.GetComponent<PlayerController>().grabbed = true;
            GameSettings.Instance.Player.GetComponent<CharacterController>().enabled = false;

            Vector3 attackDirection = Vector3.RotateTowards(transform.forward, targetDirection, 1f, 0.0f);
            transform.rotation = Quaternion.LookRotation(attackDirection);

            GameSettings.Instance.Player.GetComponent<PlayerController>().bodyAnim.SetBool("Choking", true);

            //GameSettings.Instance.Player.GetComponent<PlayerController>().playerHealth.canMoveHead = false;

            transform.rotation = new Quaternion(0, transform.rotation.y, 0, transform.rotation.w);

        }
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
                int twitchNoise = Random.Range(0, noises.Length);

                //only twitch towards player if farther than 2 units, then attack
                if (Vector3.Distance(GameSettings.Instance.Player.transform.position, transform.position) > 4 && !strangling)
                {
                    float step = speed * Time.deltaTime;
                    transform.position = Vector3.MoveTowards(transform.position, GameSettings.Instance.Player.transform.position + new Vector3(Random.Range(-2f, 2f), 0, Random.Range(-2f, 2f)), step);

                    transform.position = new Vector3(transform.position.x, 0.1f, transform.position.z);

                    transform.rotation = Quaternion.LookRotation(newDirection);

                    transform.rotation = new Quaternion(0, transform.rotation.y, 0, transform.rotation.w);
                    noiseSource.clip = noises[twitchNoise];
                    noiseSource.pitch = Random.Range(0.9f, 1.1f);
                    noiseSource.Play();

                    entityAnimator.SetBool("Twitch" + twitchPhase, true);

                    yield return new WaitForSeconds(1f);

                    //ambient
                    GetComponent<AudioSource>().pitch = Random.Range(0.5f, 1.2f);

                    entityAnimator.SetBool("Twitch" + twitchPhase, false);
                }
                else if (strangling == false)
                {
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

                    GameSettings.Instance.Player.GetComponent<PlayerController>().playerHealth.ChangeHeartRate(2f);

                    /*Debug.Log("Health: " + GameSettings.Instance.Player.GetComponent<PlayerController>().playerHealth.health);
                    Debug.Log("Sanity: " + GameSettings.Instance.Player.GetComponent<PlayerController>().playerHealth.sanity);
                    Debug.Log("Attacked");*/

                    yield return new WaitForSeconds(1);
                }
                else
                {
                    
                    yield return new WaitForSeconds(3f);
                }

                //ambient
                GetComponent<AudioSource>().pitch = Random.Range(0.5f, 1.2f);

                entityAnimator.SetBool("Twitch" + twitchPhase, false);
            }
            else
            {
                yield return new WaitForSeconds(3f);
            }

        }
    }

    
}

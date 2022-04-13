using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PartygoerAI : Entity
{
    // Angular speed in radians per sec.
    public float speed;

    public override void Despawn()
    {
        Destroy(this);
    }
    public override IEnumerator attackFunc()
    {
        while (true)
        {
            if (canAttack)
            {
                foreach (Collider box in attackHitboxs)
                    box.enabled = true;


                float step = speed * Time.deltaTime;
                transform.position = Vector3.MoveTowards(transform.position, GameSettings.Instance.Player.transform.position + new Vector3(Random.Range(-2f, 2f), 0, Random.Range(-2f, 2f)), step);

                transform.position = new Vector3(transform.position.x, 0.1f, transform.position.z);
                

                Vector3 targetDirection = GameSettings.Instance.Player.transform.transform.position - transform.position;
                float singleStep = speed * Time.deltaTime;
                Vector3 newDirection = Vector3.RotateTowards(transform.forward, targetDirection, singleStep, 0.0f);
                transform.rotation = Quaternion.LookRotation(newDirection);

                transform.rotation = new Quaternion(0, transform.rotation.y, 0, transform.rotation.w);

                int twitchPhase = Random.Range(0, 3);
                int twitchNoise = Random.Range(0, noises.Length);

                noiseSource.clip = noises[twitchPhase];
                noiseSource.pitch = Random.Range(0.9f, 1.1f);
                noiseSource.Play();

                //only twitch towards player if farther than 15 units
                /*if (Vector3.Distance(GameSettings.Instance.Player.transform.position, transform.position) > 5)
                {
                    entityAnimator.SetBool("Twitch" + twitchPhase, true);

                    yield return new WaitForSeconds(1f);

                    //ambient
                    GetComponent<AudioSource>().pitch = Random.Range(0.5f, 1.2f);

                    entityAnimator.SetBool("Twitch" + twitchPhase, false);
                }*/
                entityAnimator.SetBool("Twitch" + twitchPhase, true);

                yield return new WaitForSeconds(1f);

                //ambient
                GetComponent<AudioSource>().pitch = Random.Range(0.5f, 1.2f);

                entityAnimator.SetBool("Twitch" + twitchPhase, false);

                /*else
                {
                    entityAnimator.SetBool("Twitch0", false);
                    entityAnimator.SetBool("Twitch1", false);
                    entityAnimator.SetBool("Twitch2", false);
                    entityAnimator.SetBool("Twitch3", false);

                    yield return new WaitForSeconds(3f);

                    entityAnimator.SetBool("Attack", true);
                }*/


                foreach (Collider box in attackHitboxs)
                    box.enabled = true;
            }
            else
            {
                yield return new WaitForSeconds(3f);
            }

        }
    }

    
}

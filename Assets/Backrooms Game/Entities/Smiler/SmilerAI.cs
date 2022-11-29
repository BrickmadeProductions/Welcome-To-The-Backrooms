using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SmilerAI : Entity
{
    public ParticleSystem explosionEffect;
    public override void Init()
    {
        
    }
    public override IEnumerator AI()
    {
        while (true)
        {
            if (currentTarget != null && GameSettings.Instance.GetComponent<CheatSheet>().AIEnabled)
            {

                if (canAttack && Vector3.Distance(currentTarget.transform.position, transform.position) < entityViewDistance && canSeeTarget)
                {
                    Vector3 targetDirection = currentTarget.transform.position - transform.position;

                    float singleStep = speed * Time.deltaTime;

                    Vector3 newDirection = Vector3.RotateTowards(transform.forward, targetDirection, singleStep, 0.0f);

                    transform.rotation = Quaternion.LookRotation(newDirection);

                    transform.rotation = new Quaternion(0, transform.rotation.y, 0, transform.rotation.w);

                    Debug.Log(GameSettings.GetLocalPlayer().currentPlayerState.ToString());
                    
                    if (!playerCanSee || GameSettings.GetLocalPlayer().currentPlayerState == PlayerController.PLAYERSTATES.RUN)
                    {
                        entityAnimator.SetTrigger("StartAttack");
                        entityAnimator.SetBool("Attack", true);

                        float step = speed * Time.deltaTime;
                        transform.position = Vector3.MoveTowards(transform.position, currentTarget.transform.position + new Vector3(Random.Range(-2f, 2f), 0, Random.Range(-2f, 2f)), step);
                        
                    }
                    else
                    {
                        entityAnimator.ResetTrigger("StartAttack");
                        entityAnimator.SetBool("Attack", false);

                    }


                }
                else
                {
                    entityAnimator.ResetTrigger("StartAttack");
                    entityAnimator.SetBool("Attack", false);
                    
                }

            }
            else
            {
                entityAnimator.ResetTrigger("StartAttack");
                entityAnimator.SetBool("Attack", false);
            }


            yield return new WaitForEndOfFrame();

        }
        
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.layer == 16)
        {
            if (other.GetComponent<WTTBLightData>() != null)
            {
                other.GetComponent<WTTBLightData>().hasPower = false;
                other.GetComponent<WTTBLightData>().SetState(false);
            }
                

        }
    }
    private void OnCollisionEnter(Collision collision)
    {
        if (collision.collider.gameObject.layer == 11)
        {
            GameSettings.GetLocalPlayer().playerHealth.TakeDamage(damage, sanityMultiplier, 1f, true, DAMAGE_TYPE.ENTITY);
        }
    }
    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject.layer == 16)
        {
            if (other.GetComponent<WTTBLightData>() != null && GameSettings.Instance.worldInstance.currentWorldEvent != GAMEPLAY_EVENT.LIGHTS_OUT)
            {
                other.GetComponent<WTTBLightData>().hasPower = true;
                other.GetComponent<WTTBLightData>().SetState(true);
                other.GetComponent<WTTBLightData>().Light.intensity = other.GetComponent<WTTBLightData>().defaultIntensity;
            }
        }
    }

    public override void UpdateEntity()
    {

    }

    public override void Despawn()
    {
        Destroy(gameObject);
    }

    public override void OnEventStart()
    {
        
    }

    public override void OnEventEnd()
    {
       
    }

    public override void OnSaveFinished()
    {
        
    }

    public override void OnLoadFinished()
    {
        
    }
}

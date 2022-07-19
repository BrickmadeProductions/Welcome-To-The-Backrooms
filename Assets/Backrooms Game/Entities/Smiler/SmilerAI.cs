using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SmilerAI : Entity
{
    public ParticleSystem explosionEffect;

    public override IEnumerator AI()
    {
        while (true)
        {
            if (playerCanSee && canSeePlayer && GameSettings.Instance.GetComponent<CheatSheet>().AIEnabled)
            {
                entityAnimator.SetBool("Attack", true);
            }
            else
            {
                entityAnimator.SetBool("Attack", false);
            }

            //Debug.Log(name + "Sees Me" + playerCanSee);
            //Debug.Log(name + "Sees Player" + canSeePlayer);
            if (canAttack && Vector3.Distance(GameSettings.Instance.Player.transform.position, transform.position) < entityViewDistance)
            {

                Vector3 targetDirection = GameSettings.Instance.Player.transform.transform.position - transform.position;

                float singleStep = speed * Time.deltaTime;

                Vector3 newDirection = Vector3.RotateTowards(transform.forward, targetDirection, singleStep, 0.0f);

                transform.rotation = Quaternion.LookRotation(newDirection);

                transform.rotation = new Quaternion(0, transform.rotation.y, 0, transform.rotation.w);


            }
            
            yield return new WaitForEndOfFrame();

        }
        
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.layer == 16)
        {
            GameObject explosion = Instantiate(explosionEffect.gameObject);
            explosion.transform.position = transform.position + new Vector3(0, 1.44f, 0);
            Kill();
        }
    }

    public override void UpdateEntity()
    {

    }

    public override void Kill()
    {
        Destroy(gameObject);
    }

    

    
}

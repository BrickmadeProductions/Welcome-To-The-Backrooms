using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AttackableEntityLimb : MonoBehaviour
{
    public Entity attachedEntity;
    public float damageMultiplier;
    
    public void Hit(Vector3 collisionPoint, float damage)
    {
        //Debug.Log("Player Attacked " + attachedEntity.gameObject.name + " " + name);

        if (!GetComponent<AudioSource>().isPlaying)
        {
            attachedEntity.health -= (damage * damageMultiplier);

            if (GameSettings.Instance.BloodAndGore)
                Instantiate(attachedEntity.bloodPrefab, collisionPoint, Quaternion.identity);
            
            attachedEntity.hurtNoisesSource.clip = attachedEntity.hurtNoises[Random.Range(0, attachedEntity.hurtNoises.Length)];
            attachedEntity.hurtNoisesSource.Play();

            /*if (attachedEntity.GetComponent<Rigidbody>() != null)
            {
                attachedEntity.GetComponent<Rigidbody>().velocity += -attachedEntity.transform.forward.normalized * 100f;
                attachedEntity.GetComponent<Rigidbody>().velocity += attachedEntity.transform.up.normalized * 100f;
            }*/
            

            if (attachedEntity.type == ENTITY_TYPE.PARTYGOER && attachedEntity.stunned)
            {
                ((PartygoerAI)attachedEntity).partyGoerType = 0;
                ((PartygoerAI)attachedEntity).SetMetaData("partyGoerType", ((PartygoerAI)attachedEntity).partyGoerType.ToString());
                ((PartygoerAI)attachedEntity).partygoerBalloon.transform.parent = null;
                ((PartygoerAI)attachedEntity).partygoerBalloon.transform.Find("StringArmature").Find("Bone.007").gameObject.GetComponent<Rigidbody>().constraints = RigidbodyConstraints.None;

            }

        }

       

    }
}

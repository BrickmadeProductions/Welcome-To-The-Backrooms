using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AttackableEntityLimb : MonoBehaviour
{
    public Entity attachedEntity;
    public float damageMultiplier;
    
    public void Stabbed(Vector3 collisionPoint)
    {
        //Debug.Log("Player Attacked " + attachedEntity.gameObject.name + " " + name);

        if (!GetComponent<AudioSource>().isPlaying)
        {
            if (GameSettings.Instance.BloodAndGore)
                Instantiate(attachedEntity.bloodPrefab, collisionPoint, Quaternion.identity);
            
            attachedEntity.hurtNoisesSource.clip = attachedEntity.hurtNoises[Random.Range(0, attachedEntity.hurtNoises.Length)];
            attachedEntity.hurtNoisesSource.Play();

        }
        
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AttackableEntityLimb : MonoBehaviour
{
    public Entity attachedEntity;
    public float damageMultiplier;
    
    public void Stabbed(Vector3 collisionPoint)
    {
        Debug.Log("Player Attacked " + attachedEntity.gameObject.name + " " + name);

        if (!GetComponent<AudioSource>().isPlaying)
        {
            GameObject blood = Instantiate(attachedEntity.bloodPrefab, collisionPoint, Quaternion.identity);
            blood.transform.localRotation = Quaternion.Euler(0,0, Random.Range(-360f, 360f));
            AudioSource.PlayClipAtPoint(attachedEntity.hurtNoises[Random.Range(0, attachedEntity.hurtNoises.Length)], transform.position);

        }
        
    }
}

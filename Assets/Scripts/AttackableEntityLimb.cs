using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AttackableEntityLimb : MonoBehaviour
{
    public Entity attachedEntity;
    public float damageMultiplier;
    
    public void PlayerStabClip()
    {
        Debug.Log("Player Attacked " + attachedEntity.gameObject.name + " " + name);

        if (!GetComponent<AudioSource>().isPlaying)
        {
            GameObject blood = Instantiate(attachedEntity.bloodPrefab, transform);
            blood.transform.rotation = Quaternion.Euler(Random.Range(-360f, 360f), Random.Range(-360f, 360f), Random.Range(-360f, 360f));
            AudioSource.PlayClipAtPoint(attachedEntity.hurtNoises[Random.Range(0, attachedEntity.hurtNoises.Length)], transform.position);

        }
        
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WaterDrop : MonoBehaviour
{
    public AudioClip[] dropClips;
    private void OnParticleCollision(GameObject other)
    {
        if (Vector3.Distance(GameSettings.Instance.Player.transform.position, transform.position) < 25)
        {
            GetComponent<AudioSource>().clip = dropClips[Random.Range(0, dropClips.Length)];
            GetComponent<AudioSource>().Play();
        }
        
    }
}

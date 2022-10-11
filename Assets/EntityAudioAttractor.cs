using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[RequireComponent(typeof(AudioSource))]
public class EntityAudioAttractor : MonoBehaviour
{
    public Transform target;
    AudioSource audioSource;

    /// <summary>
    /// Higher priority = more likely to run towards if in multiple
    /// </summary>
    public int priority;

    

    void Awake()
    {
        audioSource = GetComponent<AudioSource>();
    }

    void Update()
    {

        //ScaleWithAudio();

    }

    private void OnCollisionEnter(Collision collision)
    {
        
    }

}

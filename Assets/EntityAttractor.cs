using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class EntityAttractor : MonoBehaviour
{
    public Transform target;

    /// <summary>
    /// Higher priority = more likely to run towards if in multiple
    /// </summary>
    public int priority;

    
    void Update()
    {

        //ScaleWithAudio();

    }

    private void OnCollisionEnter(Collision collision)
    {
        
    }

}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SmilerExplode : MonoBehaviour
{
    private void Awake()
    {
        StartCoroutine(explode());
    }
    IEnumerator explode()
    {
        GetComponent<ParticleSystem>().Play();
        yield return new WaitForSeconds(3f);
        Destroy(gameObject);
        
    }
}

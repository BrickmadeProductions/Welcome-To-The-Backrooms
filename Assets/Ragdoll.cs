using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Ragdoll : MonoBehaviour
{
    // Start is called before the first frame update
    void Awake()
    {
        StartCoroutine(RagDoll());
    }

    IEnumerator RagDoll()
    {

        yield return new WaitForSeconds(320f);
        Destroy(gameObject);

    }

}

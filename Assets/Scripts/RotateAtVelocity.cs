using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RotateAtVelocity : MonoBehaviour
{
    public bool Is_Tip;
    public Rigidbody rigidBody;
    public bool Flying;


    public void OnCollisionEnter(Collision hit)
    {
        if (hit.gameObject.layer != 11)
        {
            Flying = false;
            rigidBody.isKinematic = true;  
        }
    }

    void LateUpdate()
    {
        if (Flying) {
            transform.right = rigidBody.velocity.normalized;
        }
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WTTB_ExtraCollisionData : MonoBehaviour
{
    public bool isCollidingTrigger = false;
    public bool isColliding = false;

    private void OnCollisionStay(Collision collision)
    {
        if (collision.gameObject.layer != 14)
            isColliding = true;
    }
    private void OnCollisionExit(Collision collision)
    {
        isColliding = false;
    }

    private void OnTriggerStay(Collider other)
    {
        if (other.gameObject.layer != 14)
            isCollidingTrigger = true;
        
    }
    private void OnTriggerExit(Collider other)
    {
        isCollidingTrigger = false;
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Elevator : MonoBehaviour
{
    public InteractableButton up;
    public InteractableButton down;
    public Collider doorTrigger;

    bool moving = false;

    float moveHeight = 7.97f;
    float movedAmount = 0;

    bool upOrDownBool = true;

    private void OnTriggerEnter(Collider other)
    {
        other.gameObject.transform.parent = this.gameObject.transform;
    }

    private void OnTriggerExit(Collider other)
    {
        other.gameObject.transform.parent = null;
    }

    private void Update()
    {
        if (up.pressed && !moving)
        {
            upOrDownBool = true;
            moving = true;
            
        }
        if (down.pressed && !moving)
        {
            upOrDownBool = false;
            moving = true;

        }
        if (moving && movedAmount < moveHeight)
        {
            Debug.Log("Going Up");
            float amount = upOrDownBool ? 0.04f : -0.04f;
            transform.position = new Vector3(transform.position.x, transform.position.y + amount, transform.position.z);
            movedAmount += amount;
        }
        else
        {
            moving = false;
            movedAmount = 0;
        }

        
    }
}

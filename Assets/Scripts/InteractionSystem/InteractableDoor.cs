using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InteractableDoor : InteractableObject
{
    

    public override void Throw(Vector3 force)
    {
        
    }

    public override void Use(InteractionSystem player)
    {
        gameObject.GetComponent<Rigidbody>().AddForce(player.transform.forward * 2, ForceMode.Impulse);
    }
    
    public override void Grab(InteractionSystem player)
    {
        
    }
}
    

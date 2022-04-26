using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InteractableButton : InteractableObject
{
    public bool pressed;
    private bool justPressed;
    public override void Grab(InteractionSystem player)
    {
        
    }

    public override void Throw(Vector3 force)
    {
        
    }

    public override void Use(InteractionSystem player, bool LMB)
    {
        justPressed = true;
        pressed = true;
    }

    public void Update()
    {
        if (justPressed)
        {
            pressed = false;
            justPressed = false;
            
        }
            
    }
}

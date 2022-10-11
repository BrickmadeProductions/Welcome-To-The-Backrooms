using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ElevatorCallButton : InteractableButton
{
    public GameObject leftDoor;
    public GameObject rightDoor;
    bool open;

    private void Awake()
    {
        open = false;
    }

    public override void Drop(Vector3 force)
    {
        
    }

    public override void Use(InteractionSystem player, bool LMB)
    {
        open = !open;
        if (open)
        {
            leftDoor.transform.position = new Vector3(leftDoor.transform.position.x, leftDoor.transform.position.y, leftDoor.transform.position.z + 1.5f);
            rightDoor.transform.position = new Vector3(rightDoor.transform.position.x, rightDoor.transform.position.y, rightDoor.transform.position.z - 1.5f);
        }
        else
        {
            leftDoor.transform.position = new Vector3(leftDoor.transform.position.x, leftDoor.transform.position.y, leftDoor.transform.position.z - 1.5f);
            rightDoor.transform.position = new Vector3(rightDoor.transform.position.x, rightDoor.transform.position.y, rightDoor.transform.position.z + 1.5f);
        }
    }
}

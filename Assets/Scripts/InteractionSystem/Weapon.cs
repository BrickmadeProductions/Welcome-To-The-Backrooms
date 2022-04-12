using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Weapon : MonoBehaviour
{
    public int damage;
    public HoldableObject connetedObject;

    public void OnTriggerEnter(Collider other)
    {

        if (other.gameObject.tag == "Entity" && gameObject.layer == 13 && connetedObject.animationPlaying) //13 is the layer the player holds items in their hand
        {
            Debug.Log("Player Attack");
            other.gameObject.GetComponent<EntityStats>().health -= damage;
        }
        

    }

    public void OnTriggerExit(Collider other)
    {
        Debug.Log("TriggerExit");
    }

}

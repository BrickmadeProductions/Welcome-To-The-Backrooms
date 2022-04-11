using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Weapon : MonoBehaviour
{
    public int dammage;

    public void OnTriggerEnter(Collider other)
    {

        if (other.gameObject.tag == "Entity")
        {
            other.gameObject.GetComponent<EntityStats>().health -= dammage;
        }
        Debug.Log("StartCoroutine");

    }

    public void OnTriggerExit(Collider other)
    {
        Debug.Log("TriggerExit");
    }

}

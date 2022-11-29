using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AttackHitbox : MonoBehaviour
{
    public bool playerInHitbox;
    public int damage;
    public float sanityMultiplier;
    
    private IEnumerator attackFunc()
    {
        while (playerInHitbox == true)
        {
            /*GameSettings.GetLocalPlayer().playerHealth.health -= damage;
            GameSettings.GetLocalPlayer().playerHealth.sanity *= sanityMultiplier;

            *//*Debug.Log("Health: " + GameSettings.GetLocalPlayer().playerHealth.health);
            Debug.Log("Sanity: " + GameSettings.GetLocalPlayer().playerHealth.sanity);
            Debug.Log("Attacked");*/

            yield return new WaitForSeconds(1);
        }
    }

    public void Update()
    {
        //Debug.Log("PlayerInTrigger:  " + InHitbox);
    }

    public void OnTriggerEnter(Collider other)
    {
        playerInHitbox = true;
        if(other.gameObject.tag == "Player")
        {
            StartCoroutine(attackFunc());
        }
        //Debug.Log("StartCoroutine");

    }

    public void OnTriggerExit(Collider other)
    {
        //Debug.Log("TriggerExit");
        playerInHitbox = false;
    }
}

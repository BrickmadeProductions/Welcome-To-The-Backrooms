using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class entityAttack : MonoBehaviour
{
    public bool InHitbox;
    public int dammage;
    public float sanityMultiplier;

    private IEnumerator attackFunc()
    {
        while (InHitbox == true)
        {
            GameSettings.Instance.Player.GetComponent<PlayerController>().playerHealth.health -= dammage;
            GameSettings.Instance.Player.GetComponent<PlayerController>().playerHealth.sanity *= sanityMultiplier;

            Debug.Log("Health: " + GameSettings.Instance.Player.GetComponent<PlayerController>().playerHealth.health);
            Debug.Log("Sanity: " + GameSettings.Instance.Player.GetComponent<PlayerController>().playerHealth.sanity);
            Debug.Log("Attacked");



            yield return new WaitForSeconds(1);
        }
    }

    public void Update()
    {
        Debug.Log("PlayerInTrigger:  " + InHitbox);
    }

    public void OnTriggerEnter(Collider other)
    {

        InHitbox = true;
        if(other.gameObject.tag == "Player")
        {
            StartCoroutine(attackFunc());
        }
        Debug.Log("StartCoroutine");

    }

    public void OnTriggerExit(Collider other)
    {
        Debug.Log("TriggerExit");
        InHitbox = false;
    }
}

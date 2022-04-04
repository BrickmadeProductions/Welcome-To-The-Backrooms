using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class entityAttack : MonoBehaviour
{
    public bool InHitbox;

    private IEnumerator attackFunc()
    {
        while (InHitbox == true)
        {
            GameSettings.Instance.Player.GetComponent<PlayerController>().playerHealth.health -= 2;
            GameSettings.Instance.Player.GetComponent<PlayerController>().playerHealth.sanity *= 0.95f;

            Debug.Log("Health: " + GameSettings.Instance.Player.GetComponent<PlayerController>().playerHealth.health);
            Debug.Log("Sanity: " + GameSettings.Instance.Player.GetComponent<PlayerController>().playerHealth.sanity);
            Debug.Log("Attacked");



            yield return new WaitForSeconds(1);
        }
    }

    /*
    public void OnTriggerStay(Collider other)
    {
        Debug.Log("Trigger");
        InHitbox = true;
    }

    */

    public void Update()
    {
        Debug.Log("PlayerInTrigger:  " + InHitbox);
    }

    public void OnTriggerEnter(Collider other)
    {

        InHitbox = true;
        StartCoroutine(attackFunc());
        Debug.Log("StartCoroutine");

    }

    public void OnTriggerExit(Collider other)
    {
        Debug.Log("TriggerExit");
        InHitbox = false;
    }
}

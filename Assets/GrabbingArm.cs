using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GrabbingArm : MonoBehaviour
{
    public bool isHolding;

    public void OnTriggerEnter(Collider collision)
    {
        Debug.Log(collision.gameObject.name);
        if (collision.GetComponent<Collider>().gameObject.layer == 11)
        {
            GameSettings.Instance.Player.GetComponent<PlayerController>().playerHealth.canRun = false;
            GameSettings.Instance.Player.GetComponent<PlayerController>().playerHealth.canJump = false;
            GameSettings.Instance.Player.GetComponent<PlayerController>().playerHealth.canWalk = false;
            isHolding = true;
        }
            
    }
    public void OnTriggerExit(Collider collision)
    {
        if (collision.gameObject.layer == 11)
        {
            GameSettings.Instance.Player.GetComponent<PlayerController>().currentPotentialDeathCase = DEATH_CASE.UNKNOWN;

            GameSettings.Instance.Player.GetComponent<PlayerController>().playerHealth.canJump = true;
            GameSettings.Instance.Player.GetComponent<PlayerController>().playerHealth.canWalk = true;
            GameSettings.Instance.Player.GetComponent<PlayerController>().bodyAnim.SetBool("Choking", false);
            isHolding = false;
        }
       
    }
    public void OnTriggerStay(Collider collision)
    {
        if (!GameSettings.Instance.Player.GetComponent<PlayerController>().dead && collision.gameObject.layer == 11)
        {
            if (isHolding)
            {
                GameSettings.Instance.Player.GetComponent<PlayerController>().currentPotentialDeathCase = DEATH_CASE.ENTITY;

                GameSettings.Instance.Player.GetComponent<PlayerController>().transform.position = transform.position;
                


                GameSettings.Instance.Player.GetComponent<PlayerController>().bodyAnim.SetBool("Choking", true);


            }
        }
    }


}

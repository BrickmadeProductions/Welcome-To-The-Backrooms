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
            GameSettings.GetLocalPlayer().playerHealth.canRun = false;
            GameSettings.GetLocalPlayer().playerHealth.canJump = false;
            GameSettings.GetLocalPlayer().playerHealth.canWalk = false;
            isHolding = true;
        }
            
    }
    public void OnTriggerExit(Collider collision)
    {
        if (collision.gameObject.layer == 11)
        {
            GameSettings.GetLocalPlayer().playerHealth.canJump = true;
            GameSettings.GetLocalPlayer().playerHealth.canWalk = true;
            GameSettings.GetLocalPlayer().bodyAnim.SetBool("Choking", false);
            isHolding = false;
        }
       
    }
    public void OnTriggerStay(Collider collision)
    {
        if (!GameSettings.GetLocalPlayer().dead && collision.gameObject.layer == 11)
        {
            if (isHolding)
            {
                GameSettings.GetLocalPlayer().transform.position = transform.position;
                


                GameSettings.GetLocalPlayer().bodyAnim.SetBool("Choking", true);


            }
        }
    }


}

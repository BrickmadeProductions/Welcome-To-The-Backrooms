using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PortalPistol : HoldableObject
{
    public GameObject portal;
    bool canUse = true;
    public LayerMask wallLayerMask;
    public override void Use(InteractionSystem player, bool LMB)
    {
        base.Use(player, LMB);

        if (LMB && canUse)
        {
            GameObject newPortal = Instantiate(portal, player.GetComponent<PlayerController>().playerCamera.transform.position + player.GetComponent<PlayerController>().playerCamera.transform.forward * 10f, Quaternion.identity);
            
            newPortal.transform.LookAt(player.GetComponent<PlayerController>().playerCamera.transform.position);
            newPortal.transform.rotation = Quaternion.Euler(0f, newPortal.transform.rotation.eulerAngles.y, 0f);

            if (Physics.Raycast(player.GetComponent<PlayerController>().playerCamera.transform.position, player.GetComponent<PlayerController>().playerCamera.transform.forward, out var RayHitInfo, 10f, wallLayerMask))
            {
                newPortal.transform.position = RayHitInfo.point;
                newPortal.transform.LookAt(RayHitInfo.point + RayHitInfo.normal.normalized, Vector3.up);
            }


            StartCoroutine(coolDown(newPortal));
            
        }
    }
    IEnumerator coolDown(GameObject oldPortal)
    {
        canUse = false;
        yield return new WaitForSeconds(2f);
        canUse = true;
        yield return new WaitForSeconds(25f);
        Destroy(oldPortal);
    }



}

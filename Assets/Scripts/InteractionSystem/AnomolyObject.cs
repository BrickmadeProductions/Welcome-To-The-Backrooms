using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AnomolyObject : InteractableObject
{
    public int maxPointsThatCanBeObtained;
    public int currentPointsPulled = 0;
    public override void Pickup(InteractionSystem player, bool RightHand)
    {
        
    }

    public override void Init()
    {
        
    }

    public override void OnLoadFinished()
    {
        SetMetaData("currentPointsPulled", currentPointsPulled.ToString());
    }

    public override void OnSaveFinished()
    {
        
    }

    public override void Use(InteractionSystem player, bool LMB)
    {
        if (!GameSettings.GetLocalPlayer().skillSetSystem.isCurrentlyUpgrading)
        {
            
            if (currentPointsPulled < maxPointsThatCanBeObtained)
            {
                currentPointsPulled++;

                SetMetaData("currentPointsPulled", currentPointsPulled.ToString());

                GameSettings.GetLocalPlayer().skillSetSystem.ProgressSkill(SKILL_TYPE.NO_CLIP, GameSettings.GetLocalPlayer().skillSetSystem.ProgressByDeminsingSkillPoints());
                GameSettings.Instance.GetComponent<NotificationSystem>().QueueNotification("YOU HAVE INSPECTED AN ANOMALY AND GAINED 1 SKILL POINT, USE [J] TO OPEN SKILL MENU");
            
                if (currentPointsPulled >= maxPointsThatCanBeObtained) StartCoroutine(WaitToDestroy(!GameSettings.GetLocalPlayer().skillSetSystem.isCurrentlyUpgrading));
            }
        }
       
        
    }

    IEnumerator WaitToDestroy(bool info)
    {
        yield return new WaitUntil(() => info);
        GameSettings.Instance.worldInstance.RemoveProp(GetWorldID(), true);
    }

}

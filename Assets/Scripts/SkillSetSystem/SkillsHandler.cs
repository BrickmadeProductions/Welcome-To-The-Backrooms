using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class SkillsHandler : MonoBehaviour
{
    
    void NoClipWall_Success()
    {

        GameSettings.Instance.cutSceneHandler.BeginCutScene(CUT_SCENE.NO_CLIP_SUCCESS);
        //DEMO
        //GameSettings.Instance.cutSceneHandler.BeginCutScene(CUT_SCENE.NO_CLIP_SUCCESS_WALL_DEMO);
    }
    void NoClipWall_Fail()
    {
        GameSettings.Instance.cutSceneHandler.BeginCutScene(CUT_SCENE.NO_CLIP_FAIL_WALL);
        
    }
    void NoClipHandler()
    {
        //noclip handler
        if (
            Input.GetButtonDown("Use") 
            && GameSettings.GetLocalPlayer().GetComponent<InteractionSystem>().currentlyLookingAt != null 
            && GameSettings.GetLocalPlayer().skillSetSystem.GetCurrentLevelOfSkillType(SKILL_TYPE.NO_CLIP) > 0
            && GameSettings.GetLocalPlayer().GetComponent<InteractionSystem>().currentlyLookingAt.tag == "NoClipable"
            )
        {
            if (GameSettings.GetLocalPlayer().bodyAnim.GetFloat("xWalk") > 8)
            {
                bool success = false;

                switch (GameSettings.GetLocalPlayer().skillSetSystem.GetCurrentLevelOfSkillType(SKILL_TYPE.NO_CLIP))
                {
                    case 0:

                        //NO ABILITY

                        break;

                    case 1:

                        if (UnityEngine.Random.Range(0f, 1f) > 0.8f)
                        {
                            success = true;
                        }
                        else

                            success = false;

                        break;

                    case 2:

                        if (UnityEngine.Random.Range(0f, 1f) > 0.7f)
                        {
                            success = true;
                        }
                        else

                            success = false;

                        break;

                    case 3:
                        if (UnityEngine.Random.Range(0f, 1f) > 0.3f)
                        {
                            success = true;
                        }
                        else

                            success = false;

                        break;

                    case 4:
                        if (UnityEngine.Random.Range(0f, 1f) > 0.1f)
                        {
                            success = true;
                        }
                        else

                            success = false;

                        break;

                }

                if (success)
                {
                    NoClipWall_Success();
                }
                else

                    NoClipWall_Fail();


            }

            else
            {
                GameSettings.Instance.GetComponent<NotificationSystem>().QueueNotification("MAYBE I SHOULD GET A RUNNING START?");
            }


        }
        
    }
    // Update is called once per frame
    void Update()
    {

        NoClipHandler();

    }
}

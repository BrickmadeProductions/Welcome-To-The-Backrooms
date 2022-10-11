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
        //GameSettings.Instance.cutSceneHandler.BeginCutScene(CUT_SCENE.NO_CLIP_SUCCESS_WALL_DEMO);
        GameSettings.Instance.cutSceneHandler.BeginCutScene(CUT_SCENE.NO_CLIP_SUCCESS_WALL);
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
            && GameSettings.Instance.Player.GetComponent<InteractionSystem>().currentlyLookingAt != null 
            && GameSettings.Instance.Player.GetComponent<PlayerController>().skillSetSystem.GetCurrentLevelOfSkillType(SKILL_TYPE.NO_CLIP) > 0
            && GameSettings.Instance.Player.GetComponent<InteractionSystem>().currentlyLookingAt.tag == "NoClipable"
            && GameSettings.Instance.Player.GetComponent<PlayerController>().bodyAnim.GetFloat("xWalk") > 8
            )
        {
           
            bool success = false;

            switch (GameSettings.Instance.Player.GetComponent<PlayerController>().skillSetSystem.GetCurrentLevelOfSkillType(SKILL_TYPE.NO_CLIP))
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
    }
    // Update is called once per frame
    void Update()
    {

        NoClipHandler();

    }
}

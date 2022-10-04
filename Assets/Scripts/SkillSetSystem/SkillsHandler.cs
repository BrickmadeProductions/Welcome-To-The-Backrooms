using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class SkillsHandler : MonoBehaviour
{

    // Update is called once per frame
    void Update()
    {

        //noclip handler
        if (Input.GetButtonDown("Use") && GameSettings.Instance.Player.GetComponent<InteractionSystem>().currentlyLookingAt != null && GameSettings.Instance.Player.GetComponent<PlayerController>().skillSetSystem.GetCurrentLevelOfSkillType(SKILL_TYPE.NO_CLIP) > 0)
        {
            if (GameSettings.Instance.Player.GetComponent<InteractionSystem>().currentlyLookingAt.tag == "NoClipable")
                
            
            switch (GameSettings.Instance.Player.GetComponent<PlayerController>().skillSetSystem.GetCurrentLevelOfSkillType(SKILL_TYPE.NO_CLIP))
            {
                case 0:

                        //NO ABILITY

                    break;

                case 1:

                        if (UnityEngine.Random.Range(0f, 1f) > 0.8f)
                            GameSettings.Instance.LoadScene(SCENE.LEVEL1);

                    break;

                case 2:
                        if (UnityEngine.Random.Range(0f, 1f) > 0.7f)
                            GameSettings.Instance.LoadScene(SCENE.LEVEL1);

                    break;

                case 3:
                        if (UnityEngine.Random.Range(0f, 1f) > 0.3f)
                            GameSettings.Instance.LoadScene(SCENE.LEVEL1);

                    break;

            }
        }

    }
}

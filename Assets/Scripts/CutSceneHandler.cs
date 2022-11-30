using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Animations.Rigging;
using UnityEngine.Video;
using static CUT_SCENE;

public enum CUT_SCENE
{
    NO_CLIP_SUCCESS_WALL_DEMO,
    NO_CLIP_SUCCESS,
    NO_CLIP_FAIL_WALL,
    LIGHTS_OUT,
    WAKE_UP,
    GO_TO_SLEEP,
    KNOCKED_OUT_PARTYGOER
}

public class CutSceneHandler : MonoBehaviour
{
    public AudioClip noClipSuccessAudio;
    public AudioClip noClipFailAudio;

    public DemoHandler demoHandler;

    public void BeginCutScene(CUT_SCENE cutsceneIndex) 
    {
        StartCoroutine(BeginCutSceneAsync(cutsceneIndex));
    }
    IEnumerator CloseOutSound()
    {
        float pass = 20000f;

        bool freqSmash = true;

        while (freqSmash)
        {
            if (pass > 5000f)
            {
                pass -= 1500f;
                GameSettings.Instance.audioHandler.master.SetFloat("cutoffFrequency", pass);
            }
            else
            {
                freqSmash = false;
            }
            yield return new WaitForSecondsRealtime(0.5f);
        }
        bool freqFix = true;

        GameSettings.Instance.audioHandler.master.GetFloat("cutoffFrequency", out var fixPass);

        while (freqFix)
        {
            if (fixPass < 20000f)
            {
                fixPass += 1500f;
                GameSettings.Instance.audioHandler.master.SetFloat("cutoffFrequency", fixPass);
            }
            else
            {
                freqFix = false;
            }
            yield return new WaitForSecondsRealtime(0.5f);
        }
    }
    public IEnumerator BeginCutSceneAsync(CUT_SCENE cutsceneIndex)
    {

        GameSettings.GetLocalPlayer().GetComponent<InteractionSystem>().Cursor.gameObject.SetActive(false);

        foreach (GenericMenu menu in GameSettings.Instance.GameplayMenuDataBase)
        {
            //toggle off all gameplay menus
            if (menu.menuOpen)
            {
                menu.ToggleMenu();
            }
        }

        GameSettings.Instance.setCutScene(true);

        switch (cutsceneIndex)
        {
            case LIGHTS_OUT:


            case NO_CLIP_SUCCESS_WALL_DEMO:

                
                GameSettings.GetLocalPlayer().bodyAnim.SetBool("NoClip_Success", true);

                

                GameSettings.GetLocalPlayer().GetComponent<PlayerHealthSystem>().earStatusAudio.clip = noClipSuccessAudio;
                GameSettings.GetLocalPlayer().GetComponent<PlayerHealthSystem>().earStatusAudio.Play();

                yield return new WaitForSecondsRealtime(0.05f);

                GameSettings.GetLocalPlayer().GetComponent<Blinking>().eyeLids.GetComponent<Animator>().SetBool("eyesClosed", true);

                GameSettings.GetLocalPlayer().GetComponent<Blinking>().eyeLids.GetComponent<Animator>().SetBool("eyesClosed", true);

                yield return new WaitForSecondsRealtime(2f);

                GameSettings.GetLocalPlayer().GetComponent<Blinking>().eyeLids.SetActive(false);

                Destroy(GameSettings.GetLocalPlayer().gameObject);

                demoHandler.PlayDemoEnd();

                yield return new WaitForSecondsRealtime(6.5f);



                GameSettings.Instance.ResetGame();

                

                break;

            case NO_CLIP_SUCCESS:

                GameSettings.GetLocalPlayer().hasStartingNoClipping = true;
                GameSettings.GetLocalPlayer().isNoClipping = true;

                GameSettings.GetLocalPlayer().playerSkin.enabled = false;

                GameSettings.GetLocalPlayer().builder.layers[0].active = false;
                GameSettings.GetLocalPlayer().builder.layers[1].active = false;
                GameSettings.GetLocalPlayer().builder.layers[2].active = false;

                StartCoroutine(CloseOutSound());

                GameSettings.GetLocalPlayer().GetComponent<PlayerHealthSystem>().ChangeHeartRate(15f);
                GameSettings.GetLocalPlayer().GetComponent<PlayerHealthSystem>().sanity -= 15f;

                GameSettings.GetLocalPlayer().rb.AddForce(GameSettings.GetLocalPlayer().transform.forward * 5f);

                GameSettings.Instance.audioHandler.ResetSoundTrackLoopState();

                GameSettings.GetLocalPlayer().bodyAnim.SetBool("NoClip_Success", true);

                GameSettings.GetLocalPlayer().GetComponent<PlayerHealthSystem>().earStatusAudio.clip = noClipSuccessAudio;
                GameSettings.GetLocalPlayer().GetComponent<PlayerHealthSystem>().earStatusAudio.Play();

                yield return new WaitForSecondsRealtime(0.05f);

                GameSettings.GetLocalPlayer().GetComponent<Blinking>().eyeLids.GetComponent<Animator>().SetBool("eyesClosed", true);

                yield return new WaitForSecondsRealtime(1f);

                GameSettings.Instance.LoadScene(GameSettings.ReturnNextRandomLevel());

                yield return new WaitForSecondsRealtime(1f);

                GameSettings.GetLocalPlayer().GetComponent<Blinking>().eyeLids.GetComponent<Animator>().SetBool("eyesClosed", false);
                
                yield return new WaitForSecondsRealtime(3f);

                GameSettings.GetLocalPlayer().bodyAnim.SetBool("NoClip_Success", false);

                GameSettings.GetLocalPlayer().GetComponent<InteractionSystem>().Cursor.gameObject.SetActive(true);

                GameSettings.GetLocalPlayer().builder.layers[0].active = true;
                GameSettings.GetLocalPlayer().builder.layers[1].active = true;
                GameSettings.GetLocalPlayer().builder.layers[2].active = true;

                GameSettings.GetLocalPlayer().playerSkin.enabled = true;

                break;

            case KNOCKED_OUT_PARTYGOER:

                GameSettings.GetLocalPlayer().hasStartingNoClipping = true;
                GameSettings.GetLocalPlayer().isNoClipping = true;

                GameSettings.GetLocalPlayer().playerSkin.enabled = false;

                GameSettings.GetLocalPlayer().builder.layers[0].active = false;
                GameSettings.GetLocalPlayer().builder.layers[1].active = false;
                GameSettings.GetLocalPlayer().builder.layers[2].active = false;

                StartCoroutine(CloseOutSound());

                GameSettings.GetLocalPlayer().GetComponent<PlayerHealthSystem>().ChangeHeartRate(15f);
                GameSettings.GetLocalPlayer().GetComponent<PlayerHealthSystem>().sanity -= 15f;

                GameSettings.GetLocalPlayer().rb.AddForce(GameSettings.GetLocalPlayer().transform.forward * 5f);

                GameSettings.Instance.audioHandler.ResetSoundTrackLoopState();

                GameSettings.GetLocalPlayer().bodyAnim.SetBool("NoClip_Success", true);

                GameSettings.GetLocalPlayer().GetComponent<PlayerHealthSystem>().earStatusAudio.clip = noClipSuccessAudio;
                GameSettings.GetLocalPlayer().GetComponent<PlayerHealthSystem>().earStatusAudio.Play();

                yield return new WaitForSecondsRealtime(0.05f);

                GameSettings.GetLocalPlayer().GetComponent<Blinking>().eyeLids.GetComponent<Animator>().SetBool("eyesClosed", true);

                yield return new WaitForSecondsRealtime(1f);

                GameSettings.Instance.LoadScene(SCENE.LEVELFUN);

                yield return new WaitForSecondsRealtime(2f);

                GameSettings.GetLocalPlayer().GetComponent<Blinking>().eyeLids.GetComponent<Animator>().SetBool("eyesClosed", false);

                GameSettings.GetLocalPlayer().bodyAnim.SetBool("NoClip_Success", false);

                GameSettings.GetLocalPlayer().GetComponent<InteractionSystem>().Cursor.gameObject.SetActive(true);

                GameSettings.GetLocalPlayer().builder.layers[0].active = true;
                GameSettings.GetLocalPlayer().builder.layers[1].active = true;
                GameSettings.GetLocalPlayer().builder.layers[2].active = true;

                GameSettings.GetLocalPlayer().playerSkin.enabled = true;

                break;

            case NO_CLIP_FAIL_WALL:

                GameSettings.GetLocalPlayer().GetComponent<PlayerHealthSystem>().health -= 5f;
                GameSettings.GetLocalPlayer().GetComponent<PlayerHealthSystem>().sanity -= 2f;

                GameSettings.GetLocalPlayer().rb.AddForce(-GameSettings.GetLocalPlayer().transform.forward * 100f);

                GameSettings.GetLocalPlayer().bodyAnim.SetBool("NoClip_Fail", true);

                GameSettings.GetLocalPlayer().GetComponent<PlayerHealthSystem>().earStatusAudio.clip = noClipFailAudio;
                GameSettings.GetLocalPlayer().GetComponent<PlayerHealthSystem>().earStatusAudio.Play();

                yield return new WaitForSecondsRealtime(0.05f);

                GameSettings.GetLocalPlayer().GetComponent<Blinking>().eyeLids.GetComponent<Animator>().SetBool("eyesClosed", true);


                yield return new WaitForSecondsRealtime(0.5f);

                GameSettings.GetLocalPlayer().bodyAnim.SetBool("NoClip_Fail", false);

                yield return new WaitForSecondsRealtime(0.5f);

                GameSettings.GetLocalPlayer().GetComponent<Blinking>().eyeLids.GetComponent<Animator>().SetBool("eyesClosed", false);

                GameSettings.GetLocalPlayer().GetComponent<InteractionSystem>().Cursor.gameObject.SetActive(true);
                break;
        }



        GameSettings.Instance.setCutScene(false);
        
    }
}

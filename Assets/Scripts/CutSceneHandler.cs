using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Video;
using static CUT_SCENE;

public enum CUT_SCENE
{
    NO_CLIP_SUCCESS_WALL_DEMO,
    NO_CLIP_SUCCESS_WALL,
    NO_CLIP_FAIL_WALL,
    LIGHTS_OUT,
    WAKE_UP,
    GO_TO_SLEEP,
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

        GameSettings.Instance.Player.GetComponent<InteractionSystem>().Cursor.gameObject.SetActive(false);

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

                GameSettings.Instance.Player.GetComponent<PlayerController>().bodyAnim.SetBool("NoClip_Success", true);

                GameSettings.Instance.Player.GetComponent<PlayerHealthSystem>().earStatusAudio.clip = noClipSuccessAudio;
                GameSettings.Instance.Player.GetComponent<PlayerHealthSystem>().earStatusAudio.Play();

                yield return new WaitForSecondsRealtime(0.05f);

                GameSettings.Instance.Player.GetComponent<Blinking>().eyeLids.GetComponent<Animator>().SetBool("eyesClosed", true);

                GameSettings.Instance.Player.GetComponent<Blinking>().eyeLids.GetComponent<Animator>().SetBool("eyesClosed", true);

                yield return new WaitForSecondsRealtime(2f);

                GameSettings.Instance.Player.GetComponent<Blinking>().eyeLids.SetActive(false);

                Destroy(GameSettings.Instance.Player.gameObject);

                demoHandler.PlayDemoEnd();

                yield return new WaitForSecondsRealtime(6.5f);

                GameSettings.Instance.ResetGame();

                

                break;

            case NO_CLIP_SUCCESS_WALL:

                StartCoroutine(CloseOutSound());

                GameSettings.Instance.Player.GetComponent<PlayerHealthSystem>().health -= 10f;
                GameSettings.Instance.Player.GetComponent<PlayerHealthSystem>().ChangeHeartRate(15f);
                GameSettings.Instance.Player.GetComponent<PlayerHealthSystem>().sanity -= 15f;

                GameSettings.Instance.Player.GetComponent<PlayerController>().rb.AddForce(GameSettings.Instance.Player.transform.forward * 5f);

                GameSettings.Instance.audioHandler.ResetSoundTrackLoopState();

                GameSettings.Instance.Player.GetComponent<PlayerController>().bodyAnim.SetBool("NoClip_Success", true);

                GameSettings.Instance.Player.GetComponent<PlayerHealthSystem>().earStatusAudio.clip = noClipSuccessAudio;
                GameSettings.Instance.Player.GetComponent<PlayerHealthSystem>().earStatusAudio.Play();

                yield return new WaitForSecondsRealtime(0.05f);

                GameSettings.Instance.Player.GetComponent<Blinking>().eyeLids.GetComponent<Animator>().SetBool("eyesClosed", true);

                yield return new WaitForSecondsRealtime(3f);

                GameSettings.Instance.Player.GetComponent<Blinking>().eyeLids.GetComponent<Animator>().SetBool("eyesClosed", false);

                GameSettings.Instance.LoadScene(GameSettings.Instance.worldInstance.ReturnNextRandomLevel());

                yield return new WaitForSecondsRealtime(3f);

                GameSettings.Instance.Player.GetComponent<PlayerController>().bodyAnim.SetBool("NoClip_Success", false);

                GameSettings.Instance.Player.GetComponent<InteractionSystem>().Cursor.gameObject.SetActive(true);

                yield return new WaitForSecondsRealtime(3f);

                break;

            case NO_CLIP_FAIL_WALL:

                GameSettings.Instance.Player.GetComponent<PlayerHealthSystem>().health -= 5f;
                GameSettings.Instance.Player.GetComponent<PlayerHealthSystem>().sanity -= 2f;

                GameSettings.Instance.Player.GetComponent<PlayerController>().rb.AddForce(-GameSettings.Instance.Player.transform.forward * 100f);

                GameSettings.Instance.Player.GetComponent<PlayerController>().bodyAnim.SetBool("NoClip_Fail", true);

                GameSettings.Instance.Player.GetComponent<PlayerHealthSystem>().earStatusAudio.clip = noClipFailAudio;
                GameSettings.Instance.Player.GetComponent<PlayerHealthSystem>().earStatusAudio.Play();

                yield return new WaitForSecondsRealtime(0.05f);

                GameSettings.Instance.Player.GetComponent<Blinking>().eyeLids.GetComponent<Animator>().SetBool("eyesClosed", true);


                yield return new WaitForSecondsRealtime(0.5f);

                GameSettings.Instance.Player.GetComponent<PlayerController>().bodyAnim.SetBool("NoClip_Fail", false);

                yield return new WaitForSecondsRealtime(0.5f);

                GameSettings.Instance.Player.GetComponent<Blinking>().eyeLids.GetComponent<Animator>().SetBool("eyesClosed", false);

                GameSettings.Instance.Player.GetComponent<InteractionSystem>().Cursor.gameObject.SetActive(true);
                break;
        }


        GameSettings.Instance.setCutScene(false);
        
    }
}

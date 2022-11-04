using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class CassetPlayer : HoldableObject
{
    public List<AudioClipData> savedClips;

    public AudioClip currentClipProcessing;
    bool finishedAddingToList = false;

    Coroutine playingClip = null;

    public override void OnLoadFinished()
    {
        if (GetMetaData("STORY_OBJECT") == "JAS")
        {
            savedClips.Add(GameSettings.Instance.JASAudioData);
            SetStat("Clip", "Jeremiah and Seth");
        }

        
    }
    public override void Drop(Vector3 force)
    {
        //Debug.Log("DROPED");
        base.Drop(force);
        GetComponent<AudioSource>().clip = null;
        GetComponent<AudioSource>().Stop();

        if (playingClip != null)
        {
            StopCoroutine(playingClip);
            playingClip = null;
        }
        
    }

    public override void Use(InteractionSystem player, bool LMB)
    {
        base.Use(player, LMB);

        currentClipProcessing = GameSettings.Instance.audioHandler.RecordToAudioClip();
        GetComponent<Animator>().SetBool("isRecording", true);

    }
    void ProcessClip()

    {   if (currentClipProcessing == null)
        {
            return;
        }

        if (currentClipProcessing.length < 1f)
        {
            finishedAddingToList = true;
            return;
        }
        //Capture the current clip data
        var position = Microphone.GetPosition(GameSettings.Instance.audioHandler.microphone);

        var soundData = new float[currentClipProcessing.samples * currentClipProcessing.channels];
        currentClipProcessing.GetData(soundData, 0);

        //Create shortened array for the data that was used for recording
        var newData = new float[position * currentClipProcessing.channels];


        GameSettings.Instance.audioHandler.StopRecording();



        //Copy the used samples to a new array
        for (int i = 0; i < newData.Length; i++)
        {
            newData[i] = soundData[i];
        }

        //One does not simply shorten an AudioClip,
        //    so we make a new one with the appropriate length
        AudioClipData newClip = new AudioClipData
        {

            clip = AudioClip.Create(currentClipProcessing.name,
                                        position,
                                        currentClipProcessing.channels,
                                        currentClipProcessing.frequency,
                                        false),

            subtitles = new List<SubtitleSection>()
        };

        newClip.clip.name = "Tape";

        newClip.clip.SetData(newData, 0);        //Give it the data from the old clip

        newClip.subtitles.Add(new SubtitleSection()
        {
            description = "...",
            timeStampEnd = newClip.clip.length
        });

        //Replace the old clip
        savedClips.Add(newClip);

        Debug.Log("Added Audio");
        //AudioClip.Destroy(currentClipProcessing);
        
        currentClipProcessing = null;

        finishedAddingToList = true;
        GetComponent<Animator>().SetBool("isRecording", false);

    }

    private void FixedUpdate()
    {
        if (GameSettings.Instance.Player.GetComponent<InventorySystem>().rHand.itemsInSlot.Count > 0)
        {
            if (GameSettings.Instance.Player.GetComponent<InventorySystem>().rHand.itemsInSlot[0].connectedObject == this)
            {
                if (Input.GetMouseButtonUp(0) && !finishedAddingToList && !animationPlaying)
                {
                    ProcessClip();
                }
                else if (finishedAddingToList)
                {
                    finishedAddingToList = false;
                }
                if (Input.GetButtonDown("Replay_Casset") && !GetComponent<AudioSource>().isPlaying && playingClip == null)
                {
                    playingClip = StartCoroutine(PlayClip(0));
                }
            }
            

        }
      
    }

    IEnumerator PlayClip(int selection)
    {
        GetComponent<AudioSource>().clip = savedClips[selection].clip;
        GetComponent<AudioSource>().Play();

        int subtitleSection = 0;

        if (savedClips[selection].subtitles.Count > 0)
        {
            while (GetComponent<AudioSource>().isPlaying)
            {
                GameSettings.Instance.GetComponent<NotificationSystem>().QueueNotification(savedClips[selection].subtitles[subtitleSection].description);

                yield return new WaitForSecondsRealtime(savedClips[selection].subtitles[subtitleSection].timeStampEnd);

                subtitleSection++;
            }
        }
        else
        {
            yield return new WaitUntil(() => GetComponent<AudioSource>().isPlaying);
        }

       
    }
}

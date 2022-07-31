using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CassetPlayer : HoldableObject
{
    public List<AudioClip> savedClips;

    public AudioClip currentClipProcessing;
    bool finishedAddingToList = false;
    private void Start()
    {
        savedClips = new List<AudioClip>();
    }

    public override void Use(InteractionSystem player, bool LMB)
    {
        base.Use(player, LMB);

        currentClipProcessing = GameSettings.Instance.audioHandler.RecordToAudioClip();
        GetComponent<Animator>().SetBool("isRecording", true);

    }
    void ProcessClip()
    {
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
        var newClip = AudioClip.Create(currentClipProcessing.name,
                                        position,
                                        currentClipProcessing.channels,
                                        currentClipProcessing.frequency,
                                        false);
        newClip.name = "Tape";

        newClip.SetData(newData, 0);        //Give it the data from the old clip

        //Replace the old clip
        savedClips.Add(newClip);

        Debug.Log("Added Audio");
        //AudioClip.Destroy(currentClipProcessing);
        
        currentClipProcessing = null;

        finishedAddingToList = true;
        GetComponent<Animator>().SetBool("isRecording", false);

    }

    private void Update()
    {
        if (Input.GetMouseButtonUp(0) && !finishedAddingToList)
        {
            ProcessClip();
        }
        else if (finishedAddingToList)
        {
            finishedAddingToList = false;
        }
        if (Input.GetButtonDown("Replay_Casset") && !GetComponent<AudioSource>().isPlaying)
        {
            GetComponent<AudioSource>().clip = savedClips[0];
            GetComponent<AudioSource>().Play();
        }
    }
}
